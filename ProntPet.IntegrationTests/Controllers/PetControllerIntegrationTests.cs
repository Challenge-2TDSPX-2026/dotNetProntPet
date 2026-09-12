using System.Net;
using System.Net.Http.Json;
using ProntPet.IntegrationTests.Fixtures;
using ProntPet.Models;

namespace ProntPet.IntegrationTests.Controllers;

[Collection("ApiCollection")] // Compartilha a instância da API (e o banco InMemory) entre classes
public class PetControllerIntegrationTests
{
    private readonly HttpClient _client;

    public PetControllerIntegrationTests(ApiFactoryFixture factory)
    {
        // Cria um client HTTP que já aponta para a API em memória
        _client = factory.CreateClient();
    }

    private async Task<int> CriarTutorAuxiliarAsync()
    {
        // Vários testes de Pet dependem de um Tutor existente. Em vez de repetir esse
        // setup entre eles, isolamos aqui — cada chamada usa dados únicos (Guid) pra não
        // colidir com os índices únicos de Cpf/Phone/Email, já que o banco é compartilhado
        // entre as classes de teste da coleção.
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var novoTutor = new
        {
            Name = "Tutor de Teste",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "senha123",
            Address = "Rua de Teste, 123"
        };

        var response = await _client.PostAsJsonAsync("/api/tutor", novoTutor);
        response.EnsureSuccessStatusCode();

        var tutorCriado = await response.Content.ReadFromJsonAsync<Tutor>();
        return tutorCriado!.Id;
    }

    [Fact]
    public async Task CriarPet_TutorValido_RetornaCreated()
    {
        // Arrange
        var idTutor = await CriarTutorAuxiliarAsync();
        var novoPet = new
        {
            IdTutor = idTutor,
            Name = "Rex",
            Species = "Cachorro",
            Breed = "Vira-lata",
            BirthDate = (DateOnly?)null,
            Weight = 12.5,
            Sex = "M"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pet", novoPet);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var petCriado = await response.Content.ReadFromJsonAsync<Pet>();
        Assert.NotNull(petCriado);
        Assert.Equal("Rex", petCriado!.Name);
    }

    [Fact]
    public async Task CriarPet_TutorInexistente_RetornaNotFound()
    {
        // Arrange
        var novoPet = new
        {
            IdTutor = 999999,
            Name = "Rex",
            Species = "Cachorro",
            Breed = "Vira-lata",
            BirthDate = (DateOnly?)null,
            Weight = 12.5,
            Sex = "M"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pet", novoPet);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CriarPet_PesoNegativo_RetornaBadRequest()
    {
        // Arrange
        var idTutor = await CriarTutorAuxiliarAsync();
        var petInvalido = new
        {
            IdTutor = idTutor,
            Name = "Rex",
            Species = "Cachorro",
            Breed = "Vira-lata",
            BirthDate = (DateOnly?)null,
            Weight = -5,
            Sex = "M"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pet", petInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_PetInexistente_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/pet/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_PetExistente_RetornaOkComDadosDoPet()
    {
        // Arrange
        var idTutor = await CriarTutorAuxiliarAsync();
        var novoPet = new
        {
            IdTutor = idTutor,
            Name = "Bidu",
            Species = "Cachorro",
            Breed = "Poodle",
            BirthDate = (DateOnly?)null,
            Weight = 6.0,
            Sex = "M"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/pet", novoPet);
        var petCriado = await createResponse.Content.ReadFromJsonAsync<Pet>();

        // Act
        var response = await _client.GetAsync($"/api/pet/{petCriado!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pet = await response.Content.ReadFromJsonAsync<Pet>();
        Assert.Equal("Bidu", pet!.Name);
    }

    [Fact]
    public async Task AtualizarPet_PetExistente_RetornaNoContent()
    {
        // Arrange
        var idTutor = await CriarTutorAuxiliarAsync();
        var novoPet = new
        {
            IdTutor = idTutor,
            Name = "Nome Antigo",
            Species = "Gato",
            Breed = "Siamês",
            BirthDate = (DateOnly?)null,
            Weight = 4.0,
            Sex = "F"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/pet", novoPet);
        var petCriado = await createResponse.Content.ReadFromJsonAsync<Pet>();

        var petAtualizado = new
        {
            Name = "Nome Novo",
            Species = "Gato",
            Breed = "Siamês",
            BirthDate = (DateOnly?)null,
            Weight = 4.5,
            Sex = "F"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/pet/{petCriado!.Id}", petAtualizado);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletarPet_PetExistente_RetornaNoContentERemoveOPet()
    {
        // Arrange
        var idTutor = await CriarTutorAuxiliarAsync();
        var novoPet = new
        {
            IdTutor = idTutor,
            Name = "Fido",
            Species = "Cachorro",
            Breed = "Vira-lata",
            BirthDate = (DateOnly?)null,
            Weight = 10.0,
            Sex = "M"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/pet", novoPet);
        var petCriado = await createResponse.Content.ReadFromJsonAsync<Pet>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/pet/{petCriado!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/pet/{petCriado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
