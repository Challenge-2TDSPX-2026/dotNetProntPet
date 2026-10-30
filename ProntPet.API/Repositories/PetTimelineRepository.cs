using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ProntPet.Common;
using ProntPet.API.Data.Mongo;
using ProntPet.Dtos;
using ProntPet.API.Models;

namespace ProntPet.API.Repositories;

public class PetTimelineRepository : MongoRepository<PetTimelineEntry>, IPetTimelineRepository
{
    public PetTimelineRepository(MongoDbContext context, IOptions<MongoDbSettings> options)
        : base(context, options.Value.PetTimelineCollectionName)
    {
    }

    public async Task<PagedResult<PetTimelineEntry>> GetPagedByPetAsync(
        int idPet, PetTimelineQueryParameters query, CancellationToken ct = default)
    {
        var filter = BuildFilter(idPet, query);

        var total = await Collection.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await Collection.Find(filter)
            .Sort(BuildSort(query))
            .Skip(query.Skip)
            .Limit(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<PetTimelineEntry>(items, (int)total, query.PageNumber, query.PageSize);
    }

    private static FilterDefinition<PetTimelineEntry> BuildFilter(int idPet, PetTimelineQueryParameters query)
    {
        var fb = Builders<PetTimelineEntry>.Filter;
        var filters = new List<FilterDefinition<PetTimelineEntry>> { fb.Eq(x => x.IdPet, idPet) };

        if (query.Type.HasValue)
            filters.Add(fb.Eq(x => x.Type, query.Type.Value));

        if (!string.IsNullOrWhiteSpace(query.Tag))
            filters.Add(fb.AnyEq(x => x.Tags, query.Tag.Trim()));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var regex = new BsonRegularExpression(Regex.Escape(query.Search.Trim()), "i");
            filters.Add(fb.Or(fb.Regex(x => x.Title, regex), fb.Regex(x => x.Description, regex)));
        }

        if (query.From.HasValue)
            filters.Add(fb.Gte(x => x.OccurredAt, query.From.Value.ToUtcAssumingUtc()));

        if (query.To.HasValue)
        {
            var to = query.To.Value.ToUtcAssumingUtc();
            // Só data (meia-noite) => inclui o dia inteiro; com hora => limite inclusivo.
            filters.Add(to.TimeOfDay == TimeSpan.Zero
                ? fb.Lt(x => x.OccurredAt, to.AddDays(1))
                : fb.Lte(x => x.OccurredAt, to));
        }

        return fb.And(filters);
    }

    private static SortDefinition<PetTimelineEntry> BuildSort(PetTimelineQueryParameters query)
    {
        var sb = Builders<PetTimelineEntry>.Sort;

        SortDefinition<PetTimelineEntry> primary = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => query.Ascending ? sb.Ascending(x => x.Title) : sb.Descending(x => x.Title),
            "type" => query.Ascending ? sb.Ascending(x => x.Type) : sb.Descending(x => x.Type),
            "createdat" => query.Ascending ? sb.Ascending(x => x.CreatedAt) : sb.Descending(x => x.CreatedAt),
            _ => query.Ascending ? sb.Ascending(x => x.OccurredAt) : sb.Descending(x => x.OccurredAt)
        };

        // Desempate pelo Id garante páginas estáveis (sem repetir/pular registros entre páginas).
        return sb.Combine(primary, sb.Ascending(x => x.Id));
    }
}