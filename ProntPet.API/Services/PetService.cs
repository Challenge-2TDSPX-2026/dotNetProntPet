using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class PetService : IPetService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PetService> _logger;

    // Tracing: ActivitySource é a classe nativa do .NET para criar "Spans".
    // Estático porque representa a fonte de todos os spans emitidos por este serviço,
    // não algo que precise de uma instância nova a cada requisição.
    private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);

    // Métricas: Counter nativo do .NET (System.Diagnostics.Metrics), exposto via OpenTelemetry.
    private readonly Counter<int> _petsCreatedCounter;

    public PetService(AppDbContext context, ILogger<PetService> logger, IMeterFactory meterFactory)
    {
        _context = context;
        _logger = logger;

        var meter = meterFactory.Create(TelemetryConstants.MeterName);
        _petsCreatedCounter = meter.CreateCounter<int>("pets_created_total", description: "Total de pets cadastrados");
    }

    public async Task<List<Pet>> GetByTutorAsync(int idTutor)
    {
        using var activity = ActivitySource.StartActivity("GetPetsByTutor");
        activity?.SetTag("tutor.id", idTutor);

        return await _context.Pets.Where(r => r.IdTutor == idTutor).ToListAsync();
    }

    public async Task<ServiceResult<Pet>> GetByIdAsync(int id)
    {
        using var activity = ActivitySource.StartActivity("GetPetById");
        activity?.SetTag("pet.id", id);

        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
        {
            _logger.LogWarning("Pet de id {PetId} não encontrado.", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Pet não encontrado");
            return ServiceResult<Pet>.NotFound($"Pet de id {id} não encontrado!");
        }

        return ServiceResult<Pet>.Ok(pet);
    }

    public async Task<ServiceResult<Pet>> CreateAsync(PetRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreatePet");
        activity?.SetTag("tutor.id", request.IdTutor);
        activity?.SetTag("pet.species", request.Species);

        var tutorExists = await _context.Tutors.AnyAsync(t => t.Id == request.IdTutor);

        if (!tutorExists)
        {
            _logger.LogWarning("Tentativa de criar pet para tutor inexistente. TutorId: {TutorId}", request.IdTutor);
            activity?.SetStatus(ActivityStatusCode.Error, "Tutor não encontrado");
            return ServiceResult<Pet>.NotFound($"O tutor de Id {request.IdTutor} não foi encontrado.");
        }

        if (request.Weight != null && request.Weight < 0)
        {
            _logger.LogWarning("Tentativa de criar pet com peso inválido. Weight: {Weight}", request.Weight);
            activity?.SetStatus(ActivityStatusCode.Error, "Peso inválido");
            return ServiceResult<Pet>.ValidationError("O peso do pet não pode ser menor que 0");
        }

        var pet = request.ToEntity();
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet criado com sucesso: {@Pet}", pet);

        // Métrica: incrementa o contador de pets criados, com a espécie como tag (label)
        // para permitir quebrar o total por espécie no backend de observabilidade.
        _petsCreatedCounter.Add(1, new KeyValuePair<string, object?>("species", pet.Species));

        activity?.SetTag("pet.id", pet.Id);

        return ServiceResult<Pet>.Ok(pet);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, PetUpdateRequest request)
    {
        using var activity = ActivitySource.StartActivity("UpdatePet");
        activity?.SetTag("pet.id", id);

        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
        {
            _logger.LogWarning("Tentativa de atualizar pet inexistente. PetId: {PetId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Pet não encontrado");
            return ServiceResult<bool>.NotFound($"Pet de id {id} não encontrado");
        }

        var updatedPet = request.ToEntity();

        pet.Update(updatedPet.Name, updatedPet.Species, updatedPet.Breed, updatedPet.BirthDate,
                    updatedPet.Weight, updatedPet.Sex);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet {PetId} atualizado com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        using var activity = ActivitySource.StartActivity("DeletePet");
        activity?.SetTag("pet.id", id);

        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
        {
            _logger.LogWarning("Tentativa de remover pet inexistente. PetId: {PetId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Pet não encontrado");
            return ServiceResult<bool>.NotFound($"Pet de id {id} não encontrado");
        }

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet {PetId} removido com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
