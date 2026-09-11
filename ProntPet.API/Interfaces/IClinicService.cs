using ProntPet.Common;
using ProntPet.Models;

namespace ProntPet.Services;

/// <summary>
/// Regras de negócio relacionadas ao cadastro de clínicas veterinárias.
/// </summary>
public interface IClinicService
{
    Task<List<Clinic>> GetAllAsync();
    Task<ServiceResult<Clinic>> GetByIdAsync(int id);
    Task<ServiceResult<Clinic>> CreateAsync(ClinicRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, ClinicRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
