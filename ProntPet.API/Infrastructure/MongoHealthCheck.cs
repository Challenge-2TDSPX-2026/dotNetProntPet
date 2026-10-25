using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using ProntPet.API.Data.Mongo;

namespace ProntPet.API.Infrastructure;

public class MongoHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _context;

    public MongoHealthCheck(MongoDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB respondeu ao ping.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao conectar no MongoDB.", ex);
        }
    }
}