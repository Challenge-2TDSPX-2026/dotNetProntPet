using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

/// <summary>
/// Regras de negócio relacionadas a prontuários médicos.
/// </summary>
public interface IMedicalRecordService
{
    Task<ServiceResult<PagedResult<MedicalRecord>>> GetByPetAsync(int idPet, MedicalRecordQueryParameters query);
    Task<ServiceResult<MedicalRecord>> GetByIdAsync(int id);
    Task<ServiceResult<MedicalRecord>> CreateAsync(MedicalRecordRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, MedicalRecordRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
