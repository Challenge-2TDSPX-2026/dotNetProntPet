using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class VaccinationService : IVaccinationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<VaccinationService> _logger;

    public VaccinationService(AppDbContext context, ILogger<VaccinationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Vaccination>> GetByPetAsync(int idPet)
    {
        return await _context.Vaccinations.Where(v => v.IdPet == idPet).ToListAsync();
    }

    public async Task<ServiceResult<Vaccination>> GetByIdAsync(int id)
    {
        var vaccination = await _context.Vaccinations.FindAsync(id);

        if (vaccination == null)
        {
            _logger.LogWarning("Vacinação de id {VaccinationId} não encontrada.", id);
            return ServiceResult<Vaccination>.NotFound($"Vacinação de id {id} não encontrada!");
        }

        return ServiceResult<Vaccination>.Ok(vaccination);
    }

    public async Task<ServiceResult<Vaccination>> CreateAsync(VaccinationRequest request)
    {
        var petExists = await _context.Pets.AnyAsync(p => p.Id == request.IdPet);

        if (!petExists)
        {
            _logger.LogWarning("Tentativa de registrar vacinação para pet inexistente. PetId: {PetId}", request.IdPet);
            return ServiceResult<Vaccination>.NotFound($"O pet de id {request.IdPet} não foi encontrado!");
        }

        if (request.ExpirationDate <= request.ApplicationDate)
        {
            _logger.LogWarning(
                "Tentativa de registrar vacinação com datas inválidas. ApplicationDate: {ApplicationDate}, ExpirationDate: {ExpirationDate}",
                request.ApplicationDate, request.ExpirationDate);
            return ServiceResult<Vaccination>.ValidationError(
                "A data de expiração da vacina não pode ser anterior ou igual a data de aplicação");
        }

        var vaccination = request.ToEntity();
        _context.Vaccinations.Add(vaccination);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Vacinação criada com sucesso: {@Vaccination}", vaccination);

        return ServiceResult<Vaccination>.Ok(vaccination);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, VaccinationRequest request)
    {
        var vaccination = await _context.Vaccinations.FindAsync(id);

        if (vaccination == null)
        {
            _logger.LogWarning("Tentativa de atualizar vacinação inexistente. VaccinationId: {VaccinationId}", id);
            return ServiceResult<bool>.NotFound($"Vacinação de id {id} não encontrada!");
        }

        var updatedVaccination = request.ToEntity();

        vaccination.Update(updatedVaccination.VaccineName, updatedVaccination.ApplicationDate,
                            updatedVaccination.ExpirationDate, updatedVaccination.Lot);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Vacinação {VaccinationId} atualizada com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var vaccination = await _context.Vaccinations.FindAsync(id);

        if (vaccination == null)
        {
            _logger.LogWarning("Tentativa de remover vacinação inexistente. VaccinationId: {VaccinationId}", id);
            return ServiceResult<bool>.NotFound($"Vacinação de id {id} não encontrada!");
        }

        _context.Vaccinations.Remove(vaccination);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Vacinação {VaccinationId} removida com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
