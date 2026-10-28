using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.Dtos;
using ProntPet.API.Models;
using ProntPet.Repositories;

namespace ProntPet.Services;

public class PetTimelineService : IPetTimelineService
{
    private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);

    private readonly IPetTimelineRepository _repository;
    private readonly AppDbContext _context; // Oracle usado só para validar a existência do pet
    private readonly ILogger<PetTimelineService> _logger;

    public PetTimelineService(IPetTimelineRepository repository, AppDbContext context,
                              ILogger<PetTimelineService> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<PetTimelineEntry>>> GetByPetAsync(
    int idPet, PetTimelineQueryParameters query)
    {
        using var activity = ActivitySource.StartActivity("GetPetTimelineByPet");
        activity?.SetTag("pet.id", idPet);
        activity?.SetTag("page.number", query.PageNumber);
        activity?.SetTag("page.size", query.PageSize);

        if (query.From.HasValue && query.To.HasValue &&
            query.From.Value.ToUtcAssumingUtc() > query.To.Value.ToUtcAssumingUtc())
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Intervalo de datas inválido");
            return ServiceResult<PagedResult<PetTimelineEntry>>.ValidationError(
                "A data inicial (from) não pode ser maior que a data final (to).");
        }

        var page = await _repository.GetPagedByPetAsync(idPet, query);
        return ServiceResult<PagedResult<PetTimelineEntry>>.Ok(page);
    }

    public async Task<ServiceResult<PetTimelineEntry>> GetByIdAsync(string id)
    {
        using var activity = ActivitySource.StartActivity("GetPetTimelineById");
        activity?.SetTag("timeline.id", id);

        var entry = await _repository.GetByIdAsync(id);

        if (entry == null)
        {
            _logger.LogWarning("Registro de timeline {TimelineId} não encontrado.", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Registro não encontrado");
            return ServiceResult<PetTimelineEntry>.NotFound($"Registro de timeline {id} não encontrado!");
        }

        return ServiceResult<PetTimelineEntry>.Ok(entry);
    }

    public async Task<ServiceResult<PetTimelineEntry>> CreateAsync(PetTimelineRequest request)
    {
        using var activity = ActivitySource.StartActivity("CreatePetTimelineEntry");
        activity?.SetTag("pet.id", request.IdPet);

        var petExists = await _context.Pets.AnyAsync(p => p.Id == request.IdPet);

        if (!petExists)
        {
            _logger.LogWarning("Tentativa de criar timeline para pet inexistente. PetId: {PetId}", request.IdPet);
            activity?.SetStatus(ActivityStatusCode.Error, "Pet não encontrado");
            return ServiceResult<PetTimelineEntry>.NotFound($"O pet de Id {request.IdPet} não foi encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Título inválido");
            return ServiceResult<PetTimelineEntry>.ValidationError("O título do registro é obrigatório.");
        }

        var entry = request.ToEntity();
        await _repository.CreateAsync(entry);

        _logger.LogInformation("Registro de timeline {TimelineId} criado para o pet {PetId}.", entry.Id, entry.IdPet);
        activity?.SetTag("timeline.id", entry.Id);

        return ServiceResult<PetTimelineEntry>.Ok(entry);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(string id, PetTimelineUpdateRequest request)
    {
        using var activity = ActivitySource.StartActivity("UpdatePetTimelineEntry");
        activity?.SetTag("timeline.id", id);

        var entry = await _repository.GetByIdAsync(id);

        if (entry == null)
        {
            _logger.LogWarning("Tentativa de atualizar registro de timeline inexistente. Id: {TimelineId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Registro não encontrado");
            return ServiceResult<bool>.NotFound($"Registro de timeline {id} não encontrado");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Título inválido");
            return ServiceResult<bool>.ValidationError("O título do registro é obrigatório.");
        }

        entry.Update(request.Type, request.Title.Trim(), request.Description,
                     request.Tags ?? new(), request.Attributes ?? new(),
                     (request.OccurredAt ?? entry.OccurredAt).ToUniversalTime());

        await _repository.UpdateAsync(id, entry);

        _logger.LogInformation("Registro de timeline {TimelineId} atualizado.", id);
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(string id)
    {
        using var activity = ActivitySource.StartActivity("DeletePetTimelineEntry");
        activity?.SetTag("timeline.id", id);

        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Tentativa de remover registro de timeline inexistente. Id: {TimelineId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, "Registro não encontrado");
            return ServiceResult<bool>.NotFound($"Registro de timeline {id} não encontrado");
        }

        _logger.LogInformation("Registro de timeline {TimelineId} removido.", id);
        return ServiceResult<bool>.Ok(true);
    }
}