using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

/// <summary>
/// Regras de negócio relacionadas ao cadastro e gestão de tutores.
/// </summary>
public interface ITutorService
{
    Task<List<Tutor>> GetAllAsync();
    Task<ServiceResult<Tutor>> GetByIdAsync(int id);
    Task<ServiceResult<Tutor>> CreateAsync(TutorRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, TutorRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
