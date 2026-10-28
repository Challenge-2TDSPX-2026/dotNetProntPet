namespace ProntPet.Common;

/// <summary>Envelope de resposta das listagens: itens, metadados de paginação e links HATEOAS.</summary>
public record PagedResponse<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalPages,
    int TotalRecords,
    List<LinkDto> Links)
{
    public static PagedResponse<T> From(PagedResult<T> result, List<LinkDto> links) =>
        new(result.Items, result.PageNumber, result.PageSize, result.TotalPages, result.TotalRecords, links);
}