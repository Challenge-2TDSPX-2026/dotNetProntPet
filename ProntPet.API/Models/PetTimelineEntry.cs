using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProntPet.API.Data.Mongo;

namespace ProntPet.API.Models;

public enum TimelineEntryType
{
    Observation,
    Consultation,
    Exam,
    Procedure,
    Vaccination,
    Medication
}

/// <summary>
/// Evento/anotação clínica na linha do tempo de um pet. Documento no MongoDB;
/// <see cref="IdPet"/> referencia o pet no Oracle.
/// </summary>
public class PetTimelineEntry : MongoEntity
{
    public int IdPet { get; set; }

    [BsonRepresentation(BsonType.String)]
    public TimelineEntryType Type { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public List<string> Tags { get; set; } = new();

    /// <summary>Campos livres.</summary>
    public Dictionary<string, string> Attributes { get; set; } = new();

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime OccurredAt { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    public void Update(TimelineEntryType type, string title, string? description,
                       List<string> tags, Dictionary<string, string> attributes, DateTime occurredAt)
    {
        this.Type = type;
        this.Title = title;
        this.Description = description;
        this.Tags = tags;
        this.Attributes = attributes;
        this.OccurredAt = occurredAt;
    }
}