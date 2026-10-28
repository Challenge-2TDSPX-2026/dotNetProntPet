using ProntPet.Common;
using ProntPet.API.Models;

namespace ProntPet.Dtos;

/// <summary>Paginação, ordenação e filtros da listagem da timeline de um pet.</summary>
public class PetTimelineQueryParameters : QueryParameters
{
    public PetTimelineQueryParameters()
    {
        // Padrão da timeline: registros mais recentes primeiro.
        SortBy = "occurredAt";
        Ascending = false;
    }

    /// <summary>Filtra pelo tipo do registro (Observation, Consultation, Exam, Procedure, Vaccination, Medication).</summary>
    public TimelineEntryType? Type { get; set; }

    /// <summary>Filtra registros que contenham esta tag (igualdade exata).</summary>
    public string? Tag { get; set; }

    /// <summary>Busca textual (sem diferenciar maiúsculas) no título e na descrição.</summary>
    public string? Search { get; set; }

    /// <summary>Data/hora inicial (inclusive) do campo occurredAt.</summary>
    public DateTime? From { get; set; }

    /// <summary>Data/hora final do occurredAt. Se informar só a data, o dia inteiro é incluído.</summary>
    public DateTime? To { get; set; }
}