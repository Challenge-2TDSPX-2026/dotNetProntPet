using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class TutorService : ITutorService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TutorService> _logger;

    private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);
    private readonly Counter<int> _tutorsCreatedCounter;

    public TutorService(AppDbContext context, ILogger<TutorService> logger, IMeterFactory meterFactory)
    {
        _context = context;
        _logger = logger;

        var meter = meterFactory.Create(TelemetryConstants.MeterName);
        _tutorsCreatedCounter = meter.CreateCounter<int>("tutors_created_total", description: "Total de tutores cadastrados");
    }

    public async Task<List<Tutor>> GetAllAsync()
    {
        using var activity = ActivitySource.StartActivity("GetAllTutors");

        return await _context.Tutors.ToListAsync();
    }

    public async Task<ServiceResult<Tutor>> GetByIdAsync(int id)
    {
        using var activity = ActivitySource.StartActivity("GetTutorById");
        activity?.SetTag("tutor.id", id);

        var tutor = await _context.Tutors.FindAsync(id);

        if (tutor == null)
        {
            _logger.LogWarning("Tutor de id {TutorId} não encontrado.", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Tutor não encontrado");
            return ServiceResult<Tutor>.NotFound($"Tutor de id {id} não encontrado!");
        }

        return ServiceResult<Tutor>.Ok(tutor);
    }

    public async Task<ServiceResult<Tutor>> CreateAsync(TutorRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreateTutor");

        var tutor = request.ToEntity();
        _context.Tutors.Add(tutor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor criado com sucesso: {TutorId} - {TutorName}", tutor.Id, tutor.Name);

        _tutorsCreatedCounter.Add(1);
        activity?.SetTag("tutor.id", tutor.Id);

        return ServiceResult<Tutor>.Ok(tutor);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, TutorRequest request)
    {
        using var activity = ActivitySource.StartActivity("UpdateTutor");
        activity?.SetTag("tutor.id", id);

        var tutor = await _context.Tutors.FindAsync(id);

        if (tutor == null)
        {
            _logger.LogWarning("Tentativa de atualizar tutor inexistente. TutorId: {TutorId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Tutor não encontrado");
            return ServiceResult<bool>.NotFound($"Tutor de id {id} não encontrado!");
        }

        tutor.Update(request.Name, request.Cpf, request.Phone, request.Email, request.Password, request.Address);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor {TutorId} atualizado com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        using var activity = ActivitySource.StartActivity("DeleteTutor");
        activity?.SetTag("tutor.id", id);

        var tutor = await _context.Tutors.FindAsync(id);

        if (tutor == null)
        {
            _logger.LogWarning("Tentativa de remover tutor inexistente. TutorId: {TutorId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Tutor não encontrado");
            return ServiceResult<bool>.NotFound($"Tutor de id {id} não encontrado!");
        }

        _context.Tutors.Remove(tutor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor {TutorId} removido com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
