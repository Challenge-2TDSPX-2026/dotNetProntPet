using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.API.Models;

namespace ProntPet.Services;

/// <summary>Regras de negócio da linha do tempo clínica dos pets (MongoDB).</summary>
public interface IPetTimelineService
{
    Task<List<PetTimelineEntry>> GetByPetAsync(int idPet);
    Task<ServiceResult<PetTimelineEntry>> GetByIdAsync(string id);
    Task<ServiceResult<PetTimelineEntry>> CreateAsync(PetTimelineRequest request);
    Task<ServiceResult<bool>> UpdateAsync(string id, PetTimelineUpdateRequest request);
    Task<ServiceResult<bool>> DeleteAsync(string id);
}