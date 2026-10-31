using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProntPet.Common;

public static class UserClaimsExtensions
{
    /// <summary>Id do tutor autenticado (claim "sub" do token JWT), ou null se ausente/inválido.</summary>
    public static int? GetTutorId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;
}
