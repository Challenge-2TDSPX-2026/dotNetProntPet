namespace ProntPet.Common;

/// <summary>Resultado paginado sem links; o controller o converte em <see cref="PagedResponse{T}"/>.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalRecords, int PageNumber, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalRecords / (double)PageSize);

    /// <summary>Converte os itens (ex.: entidade para DTO) preservando os dados de paginação.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Items.Select(selector).ToList(), TotalRecords, PageNumber, PageSize);
}