using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.Dtos;
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

    public async Task<ServiceResult<PagedResult<Pet>>> GetByTutorAsync(int idTutor, PetQueryParameters query)
    {
        using var activity = ActivitySource.StartActivity("GetPetsByTutor");
        activity?.SetTag("tutor.id", idTutor);
        activity?.SetTag("page.number", query.PageNumber);
        activity?.SetTag("page.size", query.PageSize);

        IQueryable<Pet> pets = _context.Pets.AsNoTracking().Where(p => p.IdTutor == idTutor);

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLower();
            pets = pets.Where(p => p.Name != null && p.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Species))
        {
            var species = query.Species.Trim().ToLower();
            pets = pets.Where(p => p.Species.ToLower() == species);
        }

        if (!string.IsNullOrWhiteSpace(query.Breed))
        {
            var breed = query.Breed.Trim().ToLower();
            pets = pets.Where(p => p.Breed != null && p.Breed.ToLower().Contains(breed));
        }

        if (!string.IsNullOrWhiteSpace(query.Sex))
        {
            var sex = query.Sex.Trim().ToLower();
            pets = pets.Where(p => p.Sex.ToLower() == sex);
        }

        IOrderedQueryable<Pet> ordered = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "species" => pets.OrderByDirection(p => p.Species, query.Ascending),
            "breed" => pets.OrderByDirection(p => p.Breed, query.Ascending),
            "birthdate" => pets.OrderByDirection(p => p.BirthDate, query.Ascending),
            "weight" => pets.OrderByDirection(p => p.Weight, query.Ascending),
            _ => pets.OrderByDirection(p => p.Name, query.Ascending)
        };

        var page = await ordered.ThenBy(p => p.Id).ToPagedResultAsync(query);
        return ServiceResult<PagedResult<Pet>>.Ok(page);
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
