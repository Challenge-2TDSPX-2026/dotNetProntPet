using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class PetService : IPetService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PetService> _logger;

    public PetService(AppDbContext context, ILogger<PetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Pet>> GetByTutorAsync(int idTutor)
    {
        return await _context.Pets.Where(r => r.IdTutor == idTutor).ToListAsync();
    }

    public async Task<ServiceResult<Pet>> GetByIdAsync(int id)
    {
        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
        {
            _logger.LogWarning("Pet de id {PetId} não encontrado.", id);
            return ServiceResult<Pet>.NotFound($"Pet de id {id} não encontrado!");
        }

        return ServiceResult<Pet>.Ok(pet);
    }

    public async Task<ServiceResult<Pet>> CreateAsync(PetRequest request)
    {
        var tutorExists = await _context.Tutors.AnyAsync(t => t.Id == request.IdTutor);

        if (!tutorExists)
        {
            _logger.LogWarning("Tentativa de criar pet para tutor inexistente. TutorId: {TutorId}", request.IdTutor);
            return ServiceResult<Pet>.NotFound($"O tutor de Id {request.IdTutor} não foi encontrado.");
        }

        if (request.Weight != null && request.Weight < 0)
        {
            _logger.LogWarning("Tentativa de criar pet com peso inválido. Weight: {Weight}", request.Weight);
            return ServiceResult<Pet>.ValidationError("O peso do pet não pode ser menor que 0");
        }

        var pet = request.ToEntity();
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet criado com sucesso: {@Pet}", pet);

        return ServiceResult<Pet>.Ok(pet);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, PetUpdateRequest request)
    {
        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
        {
            _logger.LogWarning("Tentativa de atualizar pet inexistente. PetId: {PetId}", id);
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
        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
        {
            _logger.LogWarning("Tentativa de remover pet inexistente. PetId: {PetId}", id);
            return ServiceResult<bool>.NotFound($"Pet de id {id} não encontrado");
        }

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet {PetId} removido com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
