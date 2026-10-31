using ProntPet.Common;

namespace ProntPet.Dtos;

/// <summary>Paginação, ordenação e filtros da listagem de consultas de um prontuário.</summary>
public class ConsultationQueryParameters : QueryParameters
{
    public ConsultationQueryParameters()
    {
        // Padrão: consultas mais recentes primeiro.
        SortBy = "consultationDate";
        Ascending = false;
    }

    /// <summary>Filtra pela clínica onde a consulta ocorreu.</summary>
    public int? IdClinic { get; set; }

    /// <summary>Consultas a partir desta data (inclusive). Formato yyyy-MM-dd.</summary>
    public DateOnly? From { get; set; }

    /// <summary>Consultas até esta data (inclusive). Formato yyyy-MM-dd.</summary>
    public DateOnly? To { get; set; }

    /// <summary>Busca textual (sem diferenciar maiúsculas) em sintomas, diagnóstico e observações.</summary>
    public string? Search { get; set; }
}
