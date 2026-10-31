using ProntPet.Common;
using ProntPet.dtos;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

/// <summary>
/// Regras de negócio relacionadas ao cadastro e gestão de pets.
/// </summary>
public interface IPetService
{
    Task<ServiceResult<PagedResult<Pet>>> GetByTutorAsync(int idTutor, PetQueryParameters query);
    Task<ServiceResult<Pet>> GetByIdAsync(int id);
    Task<ServiceResult<Pet>> CreateAsync(PetRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, PetUpdateRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
