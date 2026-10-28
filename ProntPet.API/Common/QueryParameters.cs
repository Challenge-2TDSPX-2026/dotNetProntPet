using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ProntPet.Common;

/// <summary>
/// Parâmetros de paginação e ordenação comuns a todas as listagens.
/// Valores inválidos são corrigidos automaticamente (página &lt; 1 vira 1;
/// tamanho &lt; 1 vira 10; tamanho &gt; 50 vira 50).
/// Cada endpoint herda desta classe para acrescentar seus próprios filtros.
/// </summary>
public class QueryParameters
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>Página desejada (começa em 1).</summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>Itens por página (padrão 10, máximo 50).</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
    }

    /// <summary>Campo de ordenação. Cada endpoint define os campos aceitos e o padrão.</summary>
    public string? SortBy { get; set; }

    /// <summary>true = crescente (padrão); false = decrescente.</summary>
    public bool Ascending { get; set; } = true;

    /// <summary>Quantidade de registros a pular (uso interno nos repositórios).</summary>
    [BindNever]
    public int Skip => (PageNumber - 1) * PageSize;
}