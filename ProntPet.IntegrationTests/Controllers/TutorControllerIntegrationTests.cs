using System.Net;
using System.Net.Http.Json;
using ProntPet.IntegrationTests.Fixtures;
using ProntPet.Models;

namespace ProntPet.IntegrationTests.Controllers;

[Collection("ApiCollection")]
public class TutorControllerIntegrationTests
{
    private readonly HttpClient _client;

    public TutorControllerIntegrationTests(ApiFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    private static object NovoTutorValido()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new
        {
            Name = "Tutor Teste",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "senha123",
            Address = "Rua de Teste, 123"
        };
    }

    [Fact]
    public async Task CriarTutor_DadosValidos_RetornaCreated()
    {
        // Arrange
        var novoTutor = NovoTutorValido();

        // Act
        var response = await _client.PostAsJsonAsync("/api/tutor", novoTutor);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_RetornaOkComListaDeTutores()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/tutor", NovoTutorValido());

        // Act
        var response = await _client.GetAsync("/api/tutor");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_TutorInexistente_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/tutor/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarTutor_TutorExistente_RetornaNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tutor", NovoTutorValido());
        var tutorCriado = await createResponse.Content.ReadFromJsonAsync<Tutor>();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var dadosAtualizados = new
        {
            Name = "Nome Atualizado",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "novaSenha",
            Address = "Rua Nova, 456"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tutor/{tutorCriado!.Id}", dadosAtualizados);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletarTutor_TutorExistente_RetornaNoContentERemoveOTutor()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tutor", NovoTutorValido());
        var tutorCriado = await createResponse.Content.ReadFromJsonAsync<Tutor>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tutor/{tutorCriado!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/tutor/{tutorCriado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
