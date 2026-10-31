using Microsoft.AspNetCore.Identity;
using ProntPet.Models;

namespace ProntPet.Common;

/// <summary>
/// Hash e verificação de senha dos tutores, usando o <see cref="PasswordHasher{TUser}"/> do ASP.NET Core Identity
/// (PBKDF2 com sal aleatório e formato versionado). O hash cabe folgado nos 300 caracteres da coluna
/// PASSWORD, então não há migração de schema.
/// </summary>
public static class TutorPasswordHasher
{
    private static readonly PasswordHasher<Tutor> Hasher = new();

    public static string Hash(Tutor tutor, string password) => Hasher.HashPassword(tutor, password);

    public static bool Verify(Tutor tutor, string password)
    {
        if (string.IsNullOrEmpty(tutor.Password) || string.IsNullOrEmpty(password))
            return false;

        try
        {
            var result = Hasher.VerifyHashedPassword(tutor, tutor.Password, password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            // Senha legada em texto puro (não é um hash válido): nunca autentica.
            return false;
        }
    }
}
