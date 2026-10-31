using ProntPet.Common;

namespace ProntPet.Dtos;

/// <summary>Paginação, ordenação e filtros da listagem de vacinações de um pet.</summary>
public class VaccinationQueryParameters : QueryParameters
{
    public VaccinationQueryParameters()
    {
        // Padrão: vacinas aplicadas mais recentemente primeiro.
        SortBy = "applicationDate";
        Ascending = false;
    }

    /// <summary>Nome da vacina contém o texto (sem diferenciar maiúsculas).</summary>
    public string? VaccineName { get; set; }

    /// <summary>Lote contém o texto (sem diferenciar maiúsculas).</summary>
    public string? Lot { get; set; }

    /// <summary>Aplicada a partir desta data (inclusive). Formato yyyy-MM-dd.</summary>
    public DateOnly? AppliedFrom { get; set; }

    /// <summary>Aplicada até esta data (inclusive). Formato yyyy-MM-dd.</summary>
    public DateOnly? AppliedTo { get; set; }

    /// <summary>true = somente vencidas; false = somente dentro da validade.</summary>
    public bool? Expired { get; set; }
}
