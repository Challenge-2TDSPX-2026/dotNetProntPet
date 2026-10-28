using ProntPet.Common;
using ProntPet.API.Models;

namespace ProntPet.Dtos;

public record class PetTimelineResponse(
    string Id,
    int IdPet,
    TimelineEntryType Type,
    string Title,
    string? Description,
    List<string> Tags,
    Dictionary<string, string> Attributes,
    DateTime OccurredAt,
    DateTime CreatedAt,
    List<LinkDto> Links
)
{
    public static PetTimelineResponse FromEntity(PetTimelineEntry entry, List<LinkDto> links) => new(
        entry.Id!,
        entry.IdPet,
        entry.Type,
        entry.Title,
        entry.Description,
        entry.Tags,
        entry.Attributes,
        entry.OccurredAt,
        entry.CreatedAt,
        links);
}