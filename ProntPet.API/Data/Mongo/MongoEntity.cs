using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProntPet.API.Data.Mongo;

/// <summary>Base das entidades persistidas no MongoDB.</summary>
public abstract class MongoEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
}