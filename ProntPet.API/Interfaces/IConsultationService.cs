using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

/// <summary>
/// Regras de negócio relacionadas a consultas veterinárias.
/// </summary>
public interface IConsultationService
{
    Task<ServiceResult<PagedResult<Consultation>>> GetByMedicalRecordAsync(int idRecord, ConsultationQueryParameters query);
    Task<ServiceResult<Consultation>> GetByIdAsync(int id);
    Task<ServiceResult<Consultation>> CreateAsync(ConsultationRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, ConsultationRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
