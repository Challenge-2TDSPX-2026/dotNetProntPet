using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Diagnostics;
using ProntPet.Dtos;

namespace ProntPet.Services;

public class AuthService : IAuthService
{
    private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);

    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        using var activity = ActivitySource.StartActivity("Login");

        var email = request.Email.Trim().ToLower();
        var tutor = await _context.Tutors.AsNoTracking().FirstOrDefaultAsync(t => t.Email.ToLower() == email);

        // Mesma resposta para e-mail inexistente e senha incorreta: não revela quais e-mails existem.
        if (tutor == null || !TutorPasswordHasher.Verify(tutor, request.Password))
        {
            _logger.LogWarning("Falha de autenticação: credenciais inválidas.");
            activity?.SetStatus(ActivityStatusCode.Error, "Credenciais inválidas");
            return null;
        }

        var token = _tokenService.GenerateToken(tutor);

        _logger.LogInformation("Login realizado com sucesso. TutorId: {TutorId}", tutor.Id);
        activity?.SetTag("tutor.id", tutor.Id);

        return new LoginResponse(token.Token, "Bearer", token.ExpiresAt, tutor.Id, tutor.Name, tutor.Email);
    }
}
