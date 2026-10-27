using MongoDB.Driver;
using ProntPet.API.Data.Mongo;

namespace ProntPet.API.Repositories;

/// <summary>Implementação concreta do Repository sobre uma coleção do MongoDB.</summary>
public abstract class MongoRepository<T> : IRepository<T> where T : MongoEntity
{
    protected readonly IMongoCollection<T> Collection;

    protected MongoRepository(MongoDbContext context, string collectionName)
    {
        Collection = context.GetCollection<T>(collectionName);
    }

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public virtual async Task<List<T>> GetAllAsync(CancellationToken ct = default) =>
        await Collection.Find(_ => true).ToListAsync(ct);

    public virtual async Task CreateAsync(T entity, CancellationToken ct = default) =>
        await Collection.InsertOneAsync(entity, cancellationToken: ct);

    public virtual async Task<bool> UpdateAsync(string id, T entity, CancellationToken ct = default)
    {
        entity.Id = id;
        var result = await Collection.ReplaceOneAsync(x => x.Id == id, entity, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var result = await Collection.DeleteOneAsync(x => x.Id == id, ct);
        return result.DeletedCount > 0;
    }
}