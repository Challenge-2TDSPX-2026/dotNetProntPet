using ProntPet.API.Models;

namespace ProntPet.API.Repositories;

public interface IPetTimelineRepository : IRepository<PetTimelineEntry>
{
    Task<List<PetTimelineEntry>> GetByPetAsync(int idPet, CancellationToken ct = default);
}