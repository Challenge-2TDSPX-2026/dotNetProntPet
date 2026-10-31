using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Autenticação de tutores (login com e-mail e senha) e emissão de tokens JWT.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Autentica um tutor e devolve um token JWT.
        /// </summary>
        /// <remarks>
        /// Use o token no cabeçalho <c>Authorization: Bearer {token}</c> nas demais rotas.
        /// No Swagger, clique em <b>Authorize</b> e cole apenas o token.
        /// O cadastro do tutor é feito em <c>POST /api/Tutor</c> (rota pública).
        /// </remarks>
        /// <param name="request">E-mail e senha do tutor.</param>
        /// <response code="200">Autenticado. Retorna o token, o tipo e a data de expiração (UTC).</response>
        /// <response code="400">Dados inválidos (e-mail mal formatado ou campos ausentes).</response>
        /// <response code="401">E-mail ou senha incorretos.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            return response is null
                ? Unauthorized("E-mail ou senha inválidos.")
                : Ok(response);
        }

        /// <summary>
        /// Retorna os dados do tutor autenticado, lidos do token.
        /// </summary>
        /// <response code="200">Token válido.</response>
        /// <response code="401">Token ausente, inválido ou expirado.</response>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                tutorId = User.GetTutorId(),
                name = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value,
                email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            });
        }
    }
}
