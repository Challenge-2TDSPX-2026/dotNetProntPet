using System.Net;
using System.Net.Http.Json;
using ProntPet.IntegrationTests.Fixtures;
using ProntPet.Models;

namespace ProntPet.IntegrationTests.Controllers;

[Collection("ApiCollection")]
public class ClinicControllerIntegrationTests
{
    private readonly HttpClient _client;

    public ClinicControllerIntegrationTests(ApiFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    private static object NovaClinicaValida()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new
        {
            Name = "Clínica Teste",
            Cnpj = suffix,
            Address = "Av. Central, 100"
        };
    }

    [Fact]
    public async Task CriarClinica_DadosValidos_RetornaCreated()
    {
        // Arrange
        var novaClinica = NovaClinicaValida();

        // Act
        var response = await _client.PostAsJsonAsync("/api/clinic", novaClinica);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_RetornaOkComListaDeClinicas()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/clinic", NovaClinicaValida());

        // Act
        var response = await _client.GetAsync("/api/clinic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ClinicaInexistente_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/clinic/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarClinica_ClinicaExistente_RetornaNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/clinic", NovaClinicaValida());
        var clinica = await createResponse.Content.ReadFromJsonAsync<Clinic>();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var dadosAtualizados = new { Name = "Clínica Renomeada", Cnpj = suffix, Address = "Rua Nova" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/clinic/{clinica!.Id}", dadosAtualizados);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletarClinica_ClinicaExistente_RetornaNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/clinic", NovaClinicaValida());
        var clinica = await createResponse.Content.ReadFromJsonAsync<Clinic>();

        // Act
        var response = await _client.DeleteAsync($"/api/clinic/{clinica!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
