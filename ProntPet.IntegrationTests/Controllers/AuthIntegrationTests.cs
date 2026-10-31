using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ProntPet.IntegrationTests.Fixtures;

namespace ProntPet.IntegrationTests.Controllers;

/// <summary>
/// Testes de ponta a ponta de autenticação e autorização (cadastro público, login, token e 401).
/// </summary>
[Collection("ApiCollection")]
public class AuthIntegrationTests
{
    private readonly HttpClient _authenticated;
    private readonly HttpClient _anonymous;

    public AuthIntegrationTests(ApiFactoryFixture factory)
    {
        _authenticated = factory.CreateClient();
        _anonymous = factory.CreateAnonymousClient();
    }

    private static object NovoTutor(string email, string senha)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new
        {
            Name = "Tutor Auth",
            Cpf = suffix,
            Phone = suffix,
            Email = email,
            Password = senha,
            Address = "Rua de Teste, 123"
        };
    }

    private static string EmailUnico() => $"{Guid.NewGuid().ToString("N")[..8]}@teste.com";

    [Fact]
    public async Task RotaProtegida_SemToken_RetornaUnauthorized()
    {
        var response = await _anonymous.GetAsync("/api/clinic");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RotaProtegida_TokenInvalido_RetornaUnauthorized()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/clinic");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token.invalido.qualquer");

        var response = await _anonymous.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CadastroDeTutor_SemToken_EhPublico_RetornaCreated()
    {
        var response = await _anonymous.PostAsJsonAsync("/api/tutor", NovoTutor(EmailUnico(), "senha123"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ListagemDeTutores_SemToken_RetornaUnauthorized()
    {
        var response = await _anonymous.GetAsync("/api/tutor");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_CredenciaisValidas_RetornaTokenQueDaAcessoAsRotasProtegidas()
    {
        // Arrange: cadastro público + login
        var email = EmailUnico();
        (await _anonymous.PostAsJsonAsync("/api/tutor", NovoTutor(email, "senha123"))).EnsureSuccessStatusCode();

        // Act
        var loginResponse = await _anonymous.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "senha123" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = login.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal("Bearer", login.GetProperty("tokenType").GetString());

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _anonymous.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(email, me.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Login_SenhaIncorreta_RetornaUnauthorized()
    {
        var email = EmailUnico();
        (await _anonymous.PostAsJsonAsync("/api/tutor", NovoTutor(email, "senha123"))).EnsureSuccessStatusCode();

        var response = await _anonymous.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "errada" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmailInexistente_RetornaUnauthorized()
    {
        var response = await _anonymous.PostAsJsonAsync("/api/auth/login",
            new { Email = EmailUnico(), Password = "qualquer" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmailMalFormatado_RetornaBadRequest()
    {
        var response = await _anonymous.PostAsJsonAsync("/api/auth/login",
            new { Email = "isto-nao-e-um-email", Password = "senha123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ClientAutenticadoDaFixture_AcessaRotaProtegida()
    {
        var response = await _authenticated.GetAsync("/api/clinic");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CriarTutor_SenhaNaoEhExpostaNaResposta()
    {
        var response = await _anonymous.PostAsJsonAsync("/api/tutor", NovoTutor(EmailUnico(), "senha123"));

        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("senha123", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
    }
}
