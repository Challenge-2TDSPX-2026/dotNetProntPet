using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProntPet.API.Data.Mongo;
using ProntPet.API.Models;

namespace ProntPet.API.Repositories;

public class PetTimelineRepository : MongoRepository<PetTimelineEntry>, IPetTimelineRepository
{
    public PetTimelineRepository(MongoDbContext context, IOptions<MongoDbSettings> options)
        : base(context, options.Value.PetTimelineCollectionName)
    {
    }

    public async Task<List<PetTimelineEntry>> GetByPetAsync(int idPet, CancellationToken ct = default) =>
        await Collection.Find(x => x.IdPet == idPet)
                        .SortByDescending(x => x.OccurredAt)
                        .ToListAsync(ct);
}