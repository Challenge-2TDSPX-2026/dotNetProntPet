using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.Models;

namespace ProntPet.Services;

public class ClinicService : IClinicService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClinicService> _logger;

    private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);
    private readonly Counter<int> _clinicsCreatedCounter;

    public ClinicService(AppDbContext context, ILogger<ClinicService> logger, IMeterFactory meterFactory)
    {
        _context = context;
        _logger = logger;

        var meter = meterFactory.Create(TelemetryConstants.MeterName);
        _clinicsCreatedCounter = meter.CreateCounter<int>("clinics_created_total", description: "Total de clínicas cadastradas");
    }

    public async Task<List<Clinic>> GetAllAsync()
    {
        using var activity = ActivitySource.StartActivity("GetAllClinics");

        return await _context.Clinics.ToListAsync();
    }

    public async Task<ServiceResult<Clinic>> GetByIdAsync(int id)
    {
        using var activity = ActivitySource.StartActivity("GetClinicById");
        activity?.SetTag("clinic.id", id);

        var clinic = await _context.Clinics.FindAsync(id);

        if (clinic == null)
        {
            _logger.LogWarning("Clínica de id {ClinicId} não encontrada.", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Clínica não encontrada");
            return ServiceResult<Clinic>.NotFound($"Clínica de id {id} não encontrada!");
        }

        return ServiceResult<Clinic>.Ok(clinic);
    }

    public async Task<ServiceResult<Clinic>> CreateAsync(ClinicRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreateClinic");

        var clinic = request.ToEntity();
        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Clínica criada com sucesso: {@Clinic}", clinic);

        _clinicsCreatedCounter.Add(1);
        activity?.SetTag("clinic.id", clinic.Id);

        return ServiceResult<Clinic>.Ok(clinic);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, ClinicRequest request)
    {
        using var activity = ActivitySource.StartActivity("UpdateClinic");
        activity?.SetTag("clinic.id", id);

        var clinic = await _context.Clinics.FindAsync(id);

        if (clinic == null)
        {
            _logger.LogWarning("Tentativa de atualizar clínica inexistente. ClinicId: {ClinicId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Clínica não encontrada");
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
        using var activity = ActivitySource.StartActivity("DeleteClinic");
        activity?.SetTag("clinic.id", id);

        var clinic = await _context.Clinics.FindAsync(id);

        if (clinic == null)
        {
            _logger.LogWarning("Tentativa de remover clínica inexistente. ClinicId: {ClinicId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Clínica não encontrada");
            return ServiceResult<bool>.NotFound($"Clínica de id {id} não encontrada!");
        }

        _context.Clinics.Remove(clinic);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Clínica {ClinicId} removida com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
