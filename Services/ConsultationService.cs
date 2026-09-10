using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Models;

namespace ProntPet.Services;

public class ConsultationService : IConsultationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ConsultationService> _logger;

    public ConsultationService(AppDbContext context, ILogger<ConsultationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Consultation>> GetByMedicalRecordAsync(int idRecord)
    {
        return await _context.Consultations
            .Where(c => c.IdMedicalRecord == idRecord)
            .ToListAsync();
    }

    public async Task<ServiceResult<Consultation>> GetByIdAsync(int id)
    {
        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation == null)
        {
            _logger.LogWarning("Consulta de id {ConsultationId} não encontrada.", id);
            return ServiceResult<Consultation>.NotFound($"Consulta de id {id} não encontrada!");
        }

        return ServiceResult<Consultation>.Ok(consultation);
    }

    public async Task<ServiceResult<Consultation>> CreateAsync(ConsultationRequest request)
    {
        var consultation = request.ToEntity();

        var recordExists = await _context.MedicalRecords
            .AnyAsync(mc => mc.Id == consultation.IdMedicalRecord);

        if (!recordExists)
        {
            _logger.LogWarning(
                "Tentativa de registrar consulta para prontuário inexistente. MedicalRecordId: {MedicalRecordId}",
                consultation.IdMedicalRecord);
            return ServiceResult<Consultation>.NotFound(
                $"O protuário de id {consultation.IdMedicalRecord} não foi encontrado!");
        }

        var clinicExists = await _context.Clinics
            .AnyAsync(c => c.Id == consultation.IdClinic);

        if (!clinicExists)
        {
            _logger.LogWarning(
                "Tentativa de registrar consulta para clínica inexistente. ClinicId: {ClinicId}",
                consultation.IdClinic);
            return ServiceResult<Consultation>.NotFound(
                $"A clínica de id {consultation.IdClinic} não foi encontrada!");
        }

        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Consulta criada com sucesso: {@Consultation}", consultation);

        return ServiceResult<Consultation>.Ok(consultation);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, ConsultationRequest request)
    {
        var updatedConsultation = request.ToEntity();

        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation == null)
        {
            _logger.LogWarning("Tentativa de atualizar consulta inexistente. ConsultationId: {ConsultationId}", id);
            return ServiceResult<bool>.NotFound($"Consulta de id {id} não encontrada!");
        }

        consultation.Update(updatedConsultation.ConsultationDate, updatedConsultation.Symptoms,
                             updatedConsultation.Diagnosis, updatedConsultation.Observations);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Consulta {ConsultationId} atualizada com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation == null)
        {
            _logger.LogWarning("Tentativa de remover consulta inexistente. ConsultationId: {ConsultationId}", id);
            return ServiceResult<bool>.NotFound($"Consulta de id {id} não encontrada!");
        }

        _context.Consultations.Remove(consultation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Consulta {ConsultationId} removida com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
