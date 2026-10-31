using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ProntPet.Common;

public static class QueryableExtensions
{
    /// <summary>Ordena crescente ou decrescente conforme o parâmetro, sem repetir o ternário em cada Service.</summary>
    public static IOrderedQueryable<T> OrderByDirection<T, TKey>(
        this IQueryable<T> source, Expression<Func<T, TKey>> keySelector, bool ascending) =>
        ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);

    /// <summary>
    /// Conta o total e busca somente a página pedida (Skip/Take). A query deve chegar já ordenada
    /// (com desempate por Id) para que as páginas sejam estáveis.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source, QueryParameters query, CancellationToken ct = default)
    {
        var total = await source.CountAsync(ct);
        var items = await source.Skip(query.Skip).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<T>(items, total, query.PageNumber, query.PageSize);
    }
}
