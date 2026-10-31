using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Infrastructure;
using ProntPet.Models;
using ProntPet.Services;
using ProntPet.UnitTests.Helpers;

namespace ProntPet.UnitTests.Services;

public class TutorPasswordHasherTests
{
    private static Tutor NovoTutor(string senhaArmazenada = "") => new()
    {
        Id = 1, Name = "Maria", Cpf = "1", Phone = "1", Email = "maria@teste.com",
        Password = senhaArmazenada, Address = "Rua A"
    };

    [Fact]
    public void Hash_NaoGuardaASenhaEmTextoPuro()
    {
        var tutor = NovoTutor();

        var hash = TutorPasswordHasher.Hash(tutor, "senha123");

        Assert.NotEqual("senha123", hash);
        Assert.DoesNotContain("senha123", hash);
        Assert.True(hash.Length <= 300); // cabe na coluna PASSWORD (MaxLength 300)
    }

    [Fact]
    public void Hash_MesmaSenhaGeraHashesDiferentes_PorCausaDoSal()
    {
        var tutor = NovoTutor();

        Assert.NotEqual(TutorPasswordHasher.Hash(tutor, "senha123"), TutorPasswordHasher.Hash(tutor, "senha123"));
    }

    [Fact]
    public void Verify_SenhaCorreta_RetornaTrue()
    {
        var tutor = NovoTutor();
        tutor.Password = TutorPasswordHasher.Hash(tutor, "senha123");

        Assert.True(TutorPasswordHasher.Verify(tutor, "senha123"));
    }

    [Fact]
    public void Verify_SenhaIncorreta_RetornaFalse()
    {
        var tutor = NovoTutor();
        tutor.Password = TutorPasswordHasher.Hash(tutor, "senha123");

        Assert.False(TutorPasswordHasher.Verify(tutor, "outra"));
    }

    [Theory]
    [InlineData("senha antiga!")]   // texto puro que não é Base64 válido
    [InlineData("123")]             // texto puro que "parece" Base64
    [InlineData("")]
    public void Verify_SenhaLegadaEmTextoPuro_NuncaAutentica_ENaoLancaExcecao(string senhaLegada)
    {
        var tutor = NovoTutor(senhaLegada);

        Assert.False(TutorPasswordHasher.Verify(tutor, senhaLegada));
    }
}

public class TokenServiceTests
{
    private static JwtSettings Settings(int minutos = 60) => new()
    {
        Key = new string('k', 40),
        Issuer = "issuer-teste",
        Audience = "audience-teste",
        ExpirationMinutes = minutos
    };

    private static TokenService NovoTokenService(int minutos = 60) => new(Options.Create(Settings(minutos)));

    [Fact]
    public void GenerateToken_IncluiClaimsEIssuerEsperados()
    {
        var tutor = new Tutor { Id = 7, Name = "Maria", Email = "maria@teste.com" };

        var result = NovoTokenService().GenerateToken(tutor);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal("7", jwt.Subject);
        Assert.Equal("issuer-teste", jwt.Issuer);
        Assert.Contains("audience-teste", jwt.Audiences);
        Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == "maria@teste.com");
        Assert.Contains(jwt.Claims, c => c.Type == "name" && c.Value == "Maria");
        Assert.Contains(jwt.Claims, c => c.Type == "jti");
    }

    [Fact]
    public void GenerateToken_ExpiraConformeConfiguracao()
    {
        var tutor = new Tutor { Id = 1, Name = "Ana", Email = "ana@teste.com" };

        var result = NovoTokenService(minutos: 30).GenerateToken(tutor);

        var esperado = DateTime.UtcNow.AddMinutes(30);
        Assert.InRange(result.ExpiresAt, esperado.AddMinutes(-1), esperado.AddMinutes(1));
    }

    [Fact]
    public void GenerateToken_DoisTokensDoMesmoTutor_SaoDiferentes()
    {
        var tutor = new Tutor { Id = 1, Name = "Ana", Email = "ana@teste.com" };
        var service = NovoTokenService();

        Assert.NotEqual(service.GenerateToken(tutor).Token, service.GenerateToken(tutor).Token);
    }
}

public class AuthServiceTests
{
    private readonly AppDbContext _context = InMemoryDbContextFactory.Create();
    private readonly AuthService _sut;
    private readonly TutorService _tutorService;

    public AuthServiceTests()
    {
        var tokenService = new TokenService(Options.Create(new JwtSettings
        {
            Key = new string('k', 40),
            Issuer = "issuer-teste",
            Audience = "audience-teste",
            ExpirationMinutes = 60
        }));

        _sut = new AuthService(_context, tokenService, new Mock<ILogger<AuthService>>().Object);
        _tutorService = new TutorService(_context, new Mock<ILogger<TutorService>>().Object, FakeMeterFactory.Create());
    }

    private Task<ServiceResult<Tutor>> CadastrarAsync(string email, string senha) =>
        _tutorService.CreateAsync(new TutorRequest("Maria", Guid.NewGuid().ToString("N")[..8],
            Guid.NewGuid().ToString("N")[..8], email, senha, "Rua A"));

    [Fact]
    public async Task Login_CredenciaisValidas_RetornaToken()
    {
        await CadastrarAsync("maria@teste.com", "senha123");

        var response = await _sut.LoginAsync(new LoginRequest("maria@teste.com", "senha123"));

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response!.Token));
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal("maria@teste.com", response.Email);
    }

    [Fact]
    public async Task Login_EmailComCaixaDiferente_Autentica()
    {
        await CadastrarAsync("maria@teste.com", "senha123");

        var response = await _sut.LoginAsync(new LoginRequest("  MARIA@Teste.com ", "senha123"));

        Assert.NotNull(response);
    }

    [Fact]
    public async Task Login_SenhaIncorreta_RetornaNull()
    {
        await CadastrarAsync("maria@teste.com", "senha123");

        Assert.Null(await _sut.LoginAsync(new LoginRequest("maria@teste.com", "errada")));
    }

    [Fact]
    public async Task Login_EmailInexistente_RetornaNull()
    {
        Assert.Null(await _sut.LoginAsync(new LoginRequest("ninguem@teste.com", "senha123")));
    }

    [Fact]
    public async Task Login_TutorLegadoComSenhaEmTextoPuro_NaoAutentica()
    {
        // Tutor cadastrado antes do hash: a senha está em texto puro no banco.
        _context.Tutors.Add(new Tutor
        {
            Name = "Legado", Cpf = "9", Phone = "9", Email = "legado@teste.com",
            Password = "senha antiga!", Address = "Rua Z"
        });
        await _context.SaveChangesAsync();

        Assert.Null(await _sut.LoginAsync(new LoginRequest("legado@teste.com", "senha antiga!")));
    }

    [Fact]
    public async Task CadastroDeTutor_ArmazenaHash_ENaoASenhaEmTextoPuro()
    {
        await CadastrarAsync("maria@teste.com", "senha123");

        var armazenada = _context.Tutors.Single().Password;

        Assert.NotEqual("senha123", armazenada);
    }

    [Fact]
    public async Task AtualizacaoDeTutor_ReHasheiaASenha_ELoginPassaAUsarANova()
    {
        var criado = (await CadastrarAsync("maria@teste.com", "senha123")).Value!;

        await _tutorService.UpdateAsync(criado.Id,
            new TutorRequest("Maria", criado.Cpf, criado.Phone, "maria@teste.com", "novaSenha", "Rua A"));

        Assert.Null(await _sut.LoginAsync(new LoginRequest("maria@teste.com", "senha123")));
        Assert.NotNull(await _sut.LoginAsync(new LoginRequest("maria@teste.com", "novaSenha")));
    }
}
