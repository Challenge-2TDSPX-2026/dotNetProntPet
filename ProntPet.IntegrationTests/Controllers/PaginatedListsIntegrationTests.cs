using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProntPet.IntegrationTests.Fixtures;
using ProntPet.Models;

namespace ProntPet.IntegrationTests.Controllers;

/// <summary>
/// Testes de ponta a ponta das listagens paginadas (envelope PagedResponse + links HATEOAS).
/// </summary>
[Collection("ApiCollection")]
public class PaginatedListsIntegrationTests
{
    private readonly HttpClient _client;

    public PaginatedListsIntegrationTests(ApiFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CriarTutorAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var response = await _client.PostAsJsonAsync("/api/tutor", new
        {
            Name = "Tutor Paginação",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "senha123",
            Address = "Rua de Teste, 123"
        });
        response.EnsureSuccessStatusCode();

        var tutor = await response.Content.ReadFromJsonAsync<Tutor>();
        return tutor!.Id;
    }

    private async Task CriarPetAsync(int idTutor, string nome, string especie)
    {
        var response = await _client.PostAsJsonAsync("/api/pet", new
        {
            IdTutor = idTutor,
            Name = nome,
            Species = especie,
            Breed = "SRD",
            BirthDate = (DateOnly?)null,
            Weight = 5.5,
            Sex = "M"
        });
        response.EnsureSuccessStatusCode();
    }

    private static List<string> Rels(JsonElement links) =>
        links.EnumerateArray().Select(l => l.GetProperty("rel").GetString()!).ToList();

    [Fact]
    public async Task ListarPetsDoTutor_ComPaginacao_RetornaEnvelopeELinksDeNavegacao()
    {
        // Arrange
        var idTutor = await CriarTutorAsync();
        await CriarPetAsync(idTutor, "Ana", "Gato");
        await CriarPetAsync(idTutor, "Bia", "Gato");
        await CriarPetAsync(idTutor, "Cris", "Cachorro");

        // Act
        var response = await _client.GetAsync($"/api/pet/tutor/{idTutor}?pageSize=2&sortBy=name&ascending=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(2, json.GetProperty("items").GetArrayLength());
        Assert.Equal(3, json.GetProperty("totalRecords").GetInt32());
        Assert.Equal(2, json.GetProperty("totalPages").GetInt32());
        Assert.Equal(1, json.GetProperty("pageNumber").GetInt32());

        var rels = Rels(json.GetProperty("links"));
        Assert.Contains("self", rels);
        Assert.Contains("next_page", rels);
        Assert.DoesNotContain("prev_page", rels);

        // O link da próxima página preserva o tamanho da página e a ordenação.
        var next = json.GetProperty("links").EnumerateArray()
            .First(l => l.GetProperty("rel").GetString() == "next_page")
            .GetProperty("href").GetString()!;
        Assert.Contains("pageNumber=2", next);
        Assert.Contains("pageSize=2", next);
        Assert.Contains("sortBy=name", next);
    }

    [Fact]
    public async Task ListarPetsDoTutor_FiltroPorEspecie_RetornaSomenteCorrespondentes()
    {
        // Arrange
        var idTutor = await CriarTutorAsync();
        await CriarPetAsync(idTutor, "Ana", "Gato");
        await CriarPetAsync(idTutor, "Rex", "Cachorro");

        // Act
        var response = await _client.GetAsync($"/api/pet/tutor/{idTutor}?species=cachorro");

        // Assert
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, json.GetProperty("totalRecords").GetInt32());
        Assert.Equal("Rex", json.GetProperty("items")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ListarPetsDoTutor_CadaItemTrazLinksHateoas()
    {
        // Arrange
        var idTutor = await CriarTutorAsync();
        await CriarPetAsync(idTutor, "Ana", "Gato");

        // Act
        var response = await _client.GetAsync($"/api/pet/tutor/{idTutor}");

        // Assert
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var rels = Rels(json.GetProperty("items")[0].GetProperty("links"));

        Assert.Contains("self", rels);
        Assert.Contains("update_pet", rels);
        Assert.Contains("delete_pet", rels);
        Assert.Contains("tutor", rels);
        Assert.Contains("vaccinations", rels);
    }

    [Fact]
    public async Task ListarTutores_PageSizeAcimaDoMaximo_EAjustadoPara50()
    {
        var response = await _client.GetAsync("/api/tutor?pageSize=999");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(50, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task ListarTutores_ItemNaoExpoeSenhaETrazLinks()
    {
        // Arrange
        await CriarTutorAsync();

        // Act
        var response = await _client.GetAsync("/api/tutor?pageSize=1");

        // Assert
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var item = json.GetProperty("items")[0];

        Assert.False(item.TryGetProperty("password", out _));
        Assert.Contains("self", Rels(item.GetProperty("links")));
    }

    [Fact]
    public async Task ListarVacinacoes_IntervaloDeDatasInvalido_RetornaBadRequest()
    {
        var response = await _client.GetAsync(
            "/api/vaccination/pet/1?appliedFrom=2026-10-20&appliedTo=2026-10-10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CriarTutor_RespostaIncluiLinkSelf()
    {
        // Arrange
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Act
        var response = await _client.PostAsJsonAsync("/api/tutor", new
        {
            Name = "Tutor Links",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "senha123",
            Address = "Rua de Teste, 123"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("self", Rels(json.GetProperty("links")));
    }
}
