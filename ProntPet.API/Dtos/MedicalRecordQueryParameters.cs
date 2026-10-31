using ProntPet.Common;

namespace ProntPet.Dtos;

/// <summary>Paginação, ordenação e filtros da listagem de prontuários de um pet.</summary>
public class MedicalRecordQueryParameters : QueryParameters
{
    public MedicalRecordQueryParameters()
    {
        // Padrão: prontuários atualizados mais recentemente primeiro.
        SortBy = "lastUpdate";
        Ascending = false;
    }

    /// <summary>Tipo sanguíneo igual ao texto (sem diferenciar maiúsculas).</summary>
    public string? BloodType { get; set; }

    /// <summary>Filtra por pets castrados (true) ou não castrados (false).</summary>
    public bool? IsCastrated { get; set; }

    /// <summary>Busca textual (sem diferenciar maiúsculas) em alergias e doenças crônicas.</summary>
    public string? Search { get; set; }
}
