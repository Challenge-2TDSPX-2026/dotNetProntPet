namespace ProntPet.API.Data.Mongo;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;

    public string PetTimelineCollectionName { get; set; } = "pet_timeline";
}