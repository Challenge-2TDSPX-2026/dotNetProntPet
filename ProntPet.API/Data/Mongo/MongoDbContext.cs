using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ProntPet.API.Data.Mongo;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;

        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

        var client = new MongoClient(clientSettings);
        Database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName) =>
        Database.GetCollection<T>(collectionName);
}