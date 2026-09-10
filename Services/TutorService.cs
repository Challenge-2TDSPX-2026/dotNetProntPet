using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Services;

public class TutorService : ITutorService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TutorService> _logger;

    public TutorService(AppDbContext context, ILogger<TutorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Tutor>> GetAllAsync()
    {
        return await _context.Tutors.ToListAsync();
    }

    public async Task<ServiceResult<Tutor>> GetByIdAsync(int id)
    {
        var tutor = await _context.Tutors.FindAsync(id);

        if (tutor == null)
        {
            _logger.LogWarning("Tutor de id {TutorId} não encontrado.", id);
            return ServiceResult<Tutor>.NotFound($"Tutor de id {id} não encontrado!");
        }

        return ServiceResult<Tutor>.Ok(tutor);
    }

    public async Task<ServiceResult<Tutor>> CreateAsync(TutorRequest request)
    {
        var tutor = request.ToEntity();
        _context.Tutors.Add(tutor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor criado com sucesso: {TutorId} - {TutorName}", tutor.Id, tutor.Name);

        return ServiceResult<Tutor>.Ok(tutor);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, TutorRequest request)
    {
        var tutor = await _context.Tutors.FindAsync(id);

        if (tutor == null)
        {
            _logger.LogWarning("Tentativa de atualizar tutor inexistente. TutorId: {TutorId}", id);
            return ServiceResult<bool>.NotFound($"Tutor de id {id} não encontrado!");
        }

        tutor.Update(request.Name, request.Cpf, request.Phone, request.Email, request.Password, request.Address);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor {TutorId} atualizado com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var tutor = await _context.Tutors.FindAsync(id);

        if (tutor == null)
        {
            _logger.LogWarning("Tentativa de remover tutor inexistente. TutorId: {TutorId}", id);
            return ServiceResult<bool>.NotFound($"Tutor de id {id} não encontrado!");
        }

        _context.Tutors.Remove(tutor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor {TutorId} removido com sucesso.", id);

        return ServiceResult<bool>.Ok(true);
    }
}
