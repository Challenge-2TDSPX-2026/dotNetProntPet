using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MedicalRecordService> _logger;

    private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);
    private readonly Counter<int> _medicalRecordsCreatedCounter;

    public MedicalRecordService(AppDbContext context, ILogger<MedicalRecordService> logger, IMeterFactory meterFactory)
    {
        _context = context;
        _logger = logger;

        var meter = meterFactory.Create(TelemetryConstants.MeterName);
        _medicalRecordsCreatedCounter = meter.CreateCounter<int>("medical_records_created_total", description: "Total de prontuários criados");
    }

    public async Task<List<MedicalRecord>> GetByPetAsync(int idPet)
    {
        using var activity = ActivitySource.StartActivity("GetMedicalRecordsByPet");
        activity?.SetTag("pet.id", idPet);

        return await _context.MedicalRecords.Where(mc => mc.IdPet == idPet).ToListAsync();
    }

    public async Task<ServiceResult<MedicalRecord>> GetByIdAsync(int id)
    {
        using var activity = ActivitySource.StartActivity("GetMedicalRecordById");
        activity?.SetTag("medical_record.id", id);

        var record = await _context.MedicalRecords.FindAsync(id);

        if (record == null)
        {
            _logger.LogWarning("Prontuário de id {MedicalRecordId} não encontrado.", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Prontuário não encontrado");
            return ServiceResult<MedicalRecord>.NotFound($"Prontuário de id {id} não encontrado!");
        }

        return ServiceResult<MedicalRecord>.Ok(record);
    }

    public async Task<ServiceResult<MedicalRecord>> CreateAsync(MedicalRecordRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreateMedicalRecord");
        activity?.SetTag("pet.id", request.IdPet);

        var petExists = await _context.Pets.AnyAsync(p => p.Id == request.IdPet);

        if (!petExists)
        {
            _logger.LogWarning("Tentativa de criar prontuário para pet inexistente. PetId: {PetId}", request.IdPet);
            activity?.SetStatus(ActivityStatusCode.Error, "Pet não encontrado");
            return ServiceResult<MedicalRecord>.NotFound($"O pet de id {request.IdPet} não foi encontrado!");
        }

        var record = request.ToEntity();

        // data da última atualização é setado no momento da criação
        record.LastUpdate = DateTime.UtcNow;

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Prontuário criado com sucesso: {@MedicalRecord}", record);

        _medicalRecordsCreatedCounter.Add(1);
        activity?.SetTag("medical_record.id", record.Id);

        return ServiceResult<MedicalRecord>.Ok(record);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, MedicalRecordRequest request)
    {
        using var activity = ActivitySource.StartActivity("UpdateMedicalRecord");
        activity?.SetTag("medical_record.id", id);

        var record = await _context.MedicalRecords.FindAsync(id);

        if (record == null)
        {
            _logger.LogWarning("Tentativa de atualizar prontuário inexistente. MedicalRecordId: {MedicalRecordId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Prontuário não encontrado");
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
        using var activity = ActivitySource.StartActivity("DeleteMedicalRecord");
        activity?.SetTag("medical_record.id", id);

        var record = await _context.MedicalRecords.FindAsync(id);

        if (record == null)
        {
            _logger.LogWarning("Tentativa de remover prontuário inexistente. MedicalRecordId: {MedicalRecordId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Prontuário não encontrado");
            return ServiceResult<bool>.NotFound($"Protuário de id {id} não encontrado!");
        }

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Prontuário {MedicalRecordId} removido com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
