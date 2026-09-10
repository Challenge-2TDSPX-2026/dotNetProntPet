using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(AppDbContext context, ILogger<MedicalRecordService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MedicalRecord>> GetByPetAsync(int idPet)
    {
        return await _context.MedicalRecords.Where(mc => mc.IdPet == idPet).ToListAsync();
    }

    public async Task<ServiceResult<MedicalRecord>> GetByIdAsync(int id)
    {
        var record = await _context.MedicalRecords.FindAsync(id);

        if (record == null)
        {
            _logger.LogWarning("Prontuário de id {MedicalRecordId} não encontrado.", id);
            return ServiceResult<MedicalRecord>.NotFound($"Prontuário de id {id} não encontrado!");
        }

        return ServiceResult<MedicalRecord>.Ok(record);
    }

    public async Task<ServiceResult<MedicalRecord>> CreateAsync(MedicalRecordRequest request)
    {
        var petExists = await _context.Pets.AnyAsync(p => p.Id == request.IdPet);

        if (!petExists)
        {
            _logger.LogWarning("Tentativa de criar prontuário para pet inexistente. PetId: {PetId}", request.IdPet);
            return ServiceResult<MedicalRecord>.NotFound($"O pet de id {request.IdPet} não foi encontrado!");
        }

        var record = request.ToEntity();

        // data da última atualização é setado no momento da criação
        record.LastUpdate = DateTime.UtcNow;

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Prontuário criado com sucesso: {@MedicalRecord}", record);

        return ServiceResult<MedicalRecord>.Ok(record);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, MedicalRecordRequest request)
    {
        var record = await _context.MedicalRecords.FindAsync(id);

        if (record == null)
        {
            _logger.LogWarning("Tentativa de atualizar prontuário inexistente. MedicalRecordId: {MedicalRecordId}", id);
            return ServiceResult<bool>.NotFound($"O Protuário de id {id} não encontrado!");
        }

        record.Update(request.BloodType, request.Allergies,
                        request.ChronicDiseases, request.IsCastrated,
                        request.MicrochipCode, DateTime.UtcNow);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Prontuário {MedicalRecordId} atualizado com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var record = await _context.MedicalRecords.FindAsync(id);

        if (record == null)
        {
            _logger.LogWarning("Tentativa de remover prontuário inexistente. MedicalRecordId: {MedicalRecordId}", id);
            return ServiceResult<bool>.NotFound($"Protuário de id {id} não encontrado!");
        }

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Prontuário {MedicalRecordId} removido com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
