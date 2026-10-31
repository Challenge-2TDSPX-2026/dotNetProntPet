using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class ConsultationService : IConsultationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ConsultationService> _logger;

    private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);
    private readonly Counter<int> _consultationsCreatedCounter;

    public ConsultationService(AppDbContext context, ILogger<ConsultationService> logger, IMeterFactory meterFactory)
    {
        _context = context;
        _logger = logger;

        var meter = meterFactory.Create(TelemetryConstants.MeterName);
        _consultationsCreatedCounter = meter.CreateCounter<int>("consultations_created_total", description: "Total de consultas registradas");
    }

    public async Task<ServiceResult<PagedResult<Consultation>>> GetByMedicalRecordAsync(
        int idRecord, ConsultationQueryParameters query)
    {
        using var activity = ActivitySource.StartActivity("GetConsultationsByMedicalRecord");
        activity?.SetTag("medical_record.id", idRecord);
        activity?.SetTag("page.number", query.PageNumber);
        activity?.SetTag("page.size", query.PageSize);

        if (query.From.HasValue && query.To.HasValue && query.From > query.To)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Intervalo de datas inválido");
            return ServiceResult<PagedResult<Consultation>>.ValidationError(
                "A data inicial (from) não pode ser maior que a data final (to).");
        }

        IQueryable<Consultation> consultations = _context.Consultations
            .AsNoTracking()
            .Where(c => c.IdMedicalRecord == idRecord);

        if (query.IdClinic.HasValue)
        {
            var idClinic = query.IdClinic.Value;
            consultations = consultations.Where(c => c.IdClinic == idClinic);
        }

        if (query.From.HasValue)
        {
            var from = query.From.Value.ToDateTime(TimeOnly.MinValue);
            consultations = consultations.Where(c => c.ConsultationDate >= from);
        }

        if (query.To.HasValue)
        {
            var toExclusive = query.To.Value.ToDateTime(TimeOnly.MinValue).AddDays(1);
            consultations = consultations.Where(c => c.ConsultationDate < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            consultations = consultations.Where(c =>
                (c.Symptoms != null && c.Symptoms.ToLower().Contains(search)) ||
                (c.Diagnosis != null && c.Diagnosis.ToLower().Contains(search)) ||
                (c.Observations != null && c.Observations.ToLower().Contains(search)));
        }

        IOrderedQueryable<Consultation> ordered = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "idclinic" => consultations.OrderByDirection(c => c.IdClinic, query.Ascending),
            "diagnosis" => consultations.OrderByDirection(c => c.Diagnosis, query.Ascending),
            _ => consultations.OrderByDirection(c => c.ConsultationDate, query.Ascending)
        };

        var page = await ordered.ThenBy(c => c.Id).ToPagedResultAsync(query);
        return ServiceResult<PagedResult<Consultation>>.Ok(page);
    }

    public async Task<ServiceResult<Consultation>> GetByIdAsync(int id)
    {
        using var activity = ActivitySource.StartActivity("GetConsultationById");
        activity?.SetTag("consultation.id", id);

        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation == null)
        {
            _logger.LogWarning("Consulta de id {ConsultationId} não encontrada.", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Consulta não encontrada");
            return ServiceResult<Consultation>.NotFound($"Consulta de id {id} não encontrada!");
        }

        return ServiceResult<Consultation>.Ok(consultation);
    }

    public async Task<ServiceResult<Consultation>> CreateAsync(ConsultationRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreateConsultation");

        var consultation = request.ToEntity();
        activity?.SetTag("medical_record.id", consultation.IdMedicalRecord);
        activity?.SetTag("clinic.id", consultation.IdClinic);

        var recordExists = await _context.MedicalRecords
            .AnyAsync(mc => mc.Id == consultation.IdMedicalRecord);

        if (!recordExists)
        {
            _logger.LogWarning(
                "Tentativa de registrar consulta para prontuário inexistente. MedicalRecordId: {MedicalRecordId}",
                consultation.IdMedicalRecord);
            activity?.SetStatus(ActivityStatusCode.Error, "Prontuário não encontrado");
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
            activity?.SetStatus(ActivityStatusCode.Error, "Clínica não encontrada");
            return ServiceResult<Consultation>.NotFound(
                $"A clínica de id {consultation.IdClinic} não foi encontrada!");
        }

        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Consulta criada com sucesso: {@Consultation}", consultation);

        _consultationsCreatedCounter.Add(1);
        activity?.SetTag("consultation.id", consultation.Id);

        return ServiceResult<Consultation>.Ok(consultation);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, ConsultationRequest request)
    {
        using var activity = ActivitySource.StartActivity("UpdateConsultation");
        activity?.SetTag("consultation.id", id);

        var updatedConsultation = request.ToEntity();

        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation == null)
        {
            _logger.LogWarning("Tentativa de atualizar consulta inexistente. ConsultationId: {ConsultationId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Consulta não encontrada");
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
        using var activity = ActivitySource.StartActivity("DeleteConsultation");
        activity?.SetTag("consultation.id", id);

        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation == null)
        {
            _logger.LogWarning("Tentativa de remover consulta inexistente. ConsultationId: {ConsultationId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Consulta não encontrada");
            return ServiceResult<bool>.NotFound($"Consulta de id {id} não encontrada!");
        }

        _context.Consultations.Remove(consultation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Consulta {ConsultationId} removida com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
