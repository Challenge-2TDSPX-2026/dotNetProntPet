using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.API.Models;

namespace ProntPet.Repositories;

public interface IPetTimelineRepository : IRepository<PetTimelineEntry>
{
    Task<PagedResult<PetTimelineEntry>> GetPagedByPetAsync(
        int idPet, PetTimelineQueryParameters query, CancellationToken ct = default);
}