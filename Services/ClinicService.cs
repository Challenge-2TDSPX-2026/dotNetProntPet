using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Models;

namespace ProntPet.Services;

public class ClinicService : IClinicService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClinicService> _logger;

    public ClinicService(AppDbContext context, ILogger<ClinicService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Clinic>> GetAllAsync()
    {
        return await _context.Clinics.ToListAsync();
    }

    public async Task<ServiceResult<Clinic>> GetByIdAsync(int id)
    {
        var clinic = await _context.Clinics.FindAsync(id);

        if (clinic == null)
        {
            _logger.LogWarning("Clínica de id {ClinicId} não encontrada.", id);
            return ServiceResult<Clinic>.NotFound($"Clínica de id {id} não encontrada!");
        }

        return ServiceResult<Clinic>.Ok(clinic);
    }

    public async Task<ServiceResult<Clinic>> CreateAsync(ClinicRequest request)
    {
        var clinic = request.ToEntity();
        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Clínica criada com sucesso: {@Clinic}", clinic);

        return ServiceResult<Clinic>.Ok(clinic);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, ClinicRequest request)
    {
        var clinic = await _context.Clinics.FindAsync(id);

        if (clinic == null)
        {
            _logger.LogWarning("Tentativa de atualizar clínica inexistente. ClinicId: {ClinicId}", id);
            return ServiceResult<bool>.NotFound($"Clínica de id {id} não encontrada!");
        }

        var updatedClinic = request.ToEntity();
        clinic.Update(updatedClinic.Name, updatedClinic.Address);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Clínica {ClinicId} atualizada com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var clinic = await _context.Clinics.FindAsync(id);

        if (clinic == null)
        {
            _logger.LogWarning("Tentativa de remover clínica inexistente. ClinicId: {ClinicId}", id);
            return ServiceResult<bool>.NotFound($"Clínica de id {id} não encontrada!");
        }

        _context.Clinics.Remove(clinic);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Clínica {ClinicId} removida com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
