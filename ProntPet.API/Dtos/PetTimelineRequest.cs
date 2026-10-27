using ProntPet.API.Models;

namespace ProntPet.Dtos;

public record class PetTimelineRequest(
    int IdPet,
    TimelineEntryType Type,
    string Title,
    string? Description,
    List<string>? Tags,
    Dictionary<string, string>? Attributes,
    DateTime? OccurredAt
)
{
    public PetTimelineEntry ToEntity() => new()
    {
        IdPet = this.IdPet,
        Type = this.Type,
        Title = this.Title.Trim(),
        Description = this.Description,
        Tags = this.Tags ?? new(),
        Attributes = this.Attributes ?? new(),
        OccurredAt = (this.OccurredAt ?? DateTime.UtcNow).ToUniversalTime(),
        CreatedAt = DateTime.UtcNow
    };
}

public record class PetTimelineUpdateRequest(
    TimelineEntryType Type,
    string Title,
    string? Description,
    List<string>? Tags,
    Dictionary<string, string>? Attributes,
    DateTime? OccurredAt
);