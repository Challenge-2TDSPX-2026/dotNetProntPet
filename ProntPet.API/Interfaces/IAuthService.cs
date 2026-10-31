using ProntPet.Dtos;

namespace ProntPet.Services;

/// <summary>Autenticação de tutores por e-mail e senha.</summary>
public interface IAuthService
{
    /// <summary>Devolve o token se as credenciais forem válidas; null caso contrário.</summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
