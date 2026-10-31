using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

/// <summary>
/// Regras de negócio relacionadas ao registro de vacinações.
/// </summary>
public interface IVaccinationService
{
    Task<ServiceResult<PagedResult<Vaccination>>> GetByPetAsync(int idPet, VaccinationQueryParameters query);
    Task<ServiceResult<Vaccination>> GetByIdAsync(int id);
    Task<ServiceResult<Vaccination>> CreateAsync(VaccinationRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, VaccinationRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
