using Microsoft.AspNetCore.Routing;

namespace ProntPet.Common;

/// <summary>
/// Monta os links HATEOAS de navegação entre páginas (self, first, prev, next, last).
/// Recebe uma função geradora de URL (no controller: v =&gt; Url.Link("NomeDaRota", v)),
/// o que mantém a classe independente do ASP.NET e fácil de testar.
/// </summary>
public static class PaginationLinkBuilder
{
    public static List<LinkDto> Build(
        Func<RouteValueDictionary, string?> hrefFactory,
        QueryParameters query,
        int totalPages,
        IDictionary<string, object?>? filters = null,
        string method = "GET")
    {
        string Href(int page)
        {
            // Os filtros (e valores de rota, como idPet) vão primeiro; a paginação é sempre sobrescrita.
            var values = filters is null
                ? new RouteValueDictionary()
                : new RouteValueDictionary(filters);

            values["pageNumber"] = page;
            values["pageSize"] = query.PageSize;
            values["sortBy"] = query.SortBy;      // valores nulos não entram na query string
            values["ascending"] = query.Ascending;

            return hrefFactory(values) ?? string.Empty;
        }

        var links = new List<LinkDto> { new(Href(query.PageNumber), LinkRelations.Self, method) };

        if (totalPages <= 0)
            return links;

        if (query.PageNumber > 1)
        {
            links.Add(new(Href(1), LinkRelations.FirstPage, method));
            // Se a página pedida passou do fim, "anterior" aponta para a última existente.
            links.Add(new(Href(Math.Min(query.PageNumber - 1, totalPages)), LinkRelations.PrevPage, method));
        }

        if (query.PageNumber < totalPages)
        {
            links.Add(new(Href(query.PageNumber + 1), LinkRelations.NextPage, method));
            links.Add(new(Href(totalPages), LinkRelations.LastPage, method));
        }

        return links;
    }
}