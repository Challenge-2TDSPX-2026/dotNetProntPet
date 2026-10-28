using Microsoft.AspNetCore.Routing;
using ProntPet.Common;

namespace ProntPet.UnitTests.Common;

public class PaginationContractsTests
{
    // Gera URLs falsas e previsíveis: /api/test?ascending=True&pageNumber=2&pageSize=10
    private static string? FakeUrl(RouteValueDictionary values) =>
        "/api/test?" + string.Join("&", values
            .Where(kv => kv.Value != null)
            .OrderBy(kv => kv.Key)
            .Select(kv => $"{kv.Key}={kv.Value}"));

    private static List<string> Rels(List<LinkDto> links) => links.Select(l => l.Rel).ToList();

    // ---------- QueryParameters ----------

    [Fact]
    public void QueryParameters_Padrao_PaginaUmTamanhoDezCrescente()
    {
        var q = new QueryParameters();

        Assert.Equal(1, q.PageNumber);
        Assert.Equal(10, q.PageSize);
        Assert.True(q.Ascending);
        Assert.Equal(0, q.Skip);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void QueryParameters_PageNumber_Corrigido(int entrada, int esperado)
    {
        var q = new QueryParameters { PageNumber = entrada };
        Assert.Equal(esperado, q.PageNumber);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(25, 25)]
    [InlineData(50, 50)]
    [InlineData(500, 50)]
    public void QueryParameters_PageSize_Corrigido(int entrada, int esperado)
    {
        var q = new QueryParameters { PageSize = entrada };
        Assert.Equal(esperado, q.PageSize);
    }

    [Fact]
    public void QueryParameters_Skip_CalculadoPelaPagina()
    {
        var q = new QueryParameters { PageNumber = 3, PageSize = 20 };
        Assert.Equal(40, q.Skip);
    }

    // ---------- PagedResult / PagedResponse ----------

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(21, 10, 3)]
    public void PagedResult_TotalPages_Calculado(int total, int pageSize, int esperado)
    {
        var result = new PagedResult<int>(new List<int>(), total, 1, pageSize);
        Assert.Equal(esperado, result.TotalPages);
    }

    [Fact]
    public void PagedResult_Map_PreservaPaginacao()
    {
        var result = new PagedResult<int>(new List<int> { 1, 2 }, 12, 2, 2);

        var mapped = result.Map(i => $"item-{i}");

        Assert.Equal(new[] { "item-1", "item-2" }, mapped.Items);
        Assert.Equal(12, mapped.TotalRecords);
        Assert.Equal(2, mapped.PageNumber);
        Assert.Equal(6, mapped.TotalPages);
    }

    [Fact]
    public void PagedResponse_From_CopiaDadosDoResultado()
    {
        var result = new PagedResult<string>(new List<string> { "a" }, 11, 2, 10);
        var links = new List<LinkDto> { new("/x", "self", "GET") };

        var response = PagedResponse<string>.From(result, links);

        Assert.Equal(2, response.PageNumber);
        Assert.Equal(2, response.TotalPages);
        Assert.Equal(11, response.TotalRecords);
        Assert.Same(links, response.Links);
    }

    // ---------- PaginationLinkBuilder ----------

    [Fact]
    public void Links_PrimeiraPagina_SelfNextLast()
    {
        var links = PaginationLinkBuilder.Build(FakeUrl, new QueryParameters { PageNumber = 1 }, totalPages: 3);

        Assert.Equal(new[] { "self", "next_page", "last_page" }, Rels(links));
        Assert.Contains("pageNumber=2", links[1].Href);
        Assert.Contains("pageNumber=3", links[2].Href);
    }

    [Fact]
    public void Links_PaginaDoMeio_TodosOsLinks()
    {
        var links = PaginationLinkBuilder.Build(FakeUrl, new QueryParameters { PageNumber = 2 }, totalPages: 3);

        Assert.Equal(new[] { "self", "first_page", "prev_page", "next_page", "last_page" }, Rels(links));
    }

    [Fact]
    public void Links_UltimaPagina_SelfFirstPrev()
    {
        var links = PaginationLinkBuilder.Build(FakeUrl, new QueryParameters { PageNumber = 3 }, totalPages: 3);

        Assert.Equal(new[] { "self", "first_page", "prev_page" }, Rels(links));
    }

    [Fact]
    public void Links_SemRegistros_ApenasSelf()
    {
        var links = PaginationLinkBuilder.Build(FakeUrl, new QueryParameters(), totalPages: 0);

        Assert.Single(links);
        Assert.Equal("self", links[0].Rel);
    }

    [Fact]
    public void Links_PaginaAlemDoFim_PrevApontaParaUltimaExistente()
    {
        var links = PaginationLinkBuilder.Build(FakeUrl, new QueryParameters { PageNumber = 9 }, totalPages: 3);

        var prev = links.Single(l => l.Rel == "prev_page");
        Assert.Contains("pageNumber=3", prev.Href);
    }

    [Fact]
    public void Links_PreservaFiltrosOrdenacaoETamanho()
    {
        var query = new QueryParameters { PageNumber = 1, PageSize = 25, SortBy = "title", Ascending = false };
        var filters = new Dictionary<string, object?> { ["idPet"] = 1, ["tag"] = "rotina", ["type"] = null };

        var links = PaginationLinkBuilder.Build(FakeUrl, query, totalPages: 2, filters);

        var next = links.Single(l => l.Rel == "next_page").Href;
        Assert.Contains("idPet=1", next);
        Assert.Contains("tag=rotina", next);
        Assert.Contains("pageSize=25", next);
        Assert.Contains("sortBy=title", next);
        Assert.Contains("ascending=False", next);
        Assert.DoesNotContain("type=", next); // filtro nulo não vai para a URL
    }
}