using System.Net;
using System.Net.Http.Json;
using ProntPet.IntegrationTests.Fixtures;
using ProntPet.Models;

namespace ProntPet.IntegrationTests.Controllers;

[Collection("ApiCollection")]
public class VaccinationControllerIntegrationTests
{
    private readonly HttpClient _client;

    public VaccinationControllerIntegrationTests(ApiFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CriarPetAuxiliarAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var tutorResponse = await _client.PostAsJsonAsync("/api/tutor", new
        {
            Name = "Tutor Vacinação",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "123",
            Address = "Rua V"
        });
        var tutor = await tutorResponse.Content.ReadFromJsonAsync<Tutor>();

        var petResponse = await _client.PostAsJsonAsync("/api/pet", new
        {
            IdTutor = tutor!.Id,
            Name = "Pet Vacinação",
            Species = "Cachorro",
            Breed = (string?)null,
            BirthDate = (DateOnly?)null,
            Weight = 10.0,
            Sex = "M"
        });
        var pet = await petResponse.Content.ReadFromJsonAsync<Pet>();
        return pet!.Id;
    }

    [Fact]
    public async Task CriarVacinacao_PetInexistente_RetornaNotFound()
    {
        // Arrange
        var request = new
        {
            IdPet = 999999,
            VaccineName = "V10",
            ApplicationDate = new DateOnly(2026, 1, 1),
            ExpirationDate = new DateOnly(2027, 1, 1),
            Lot = "LOTE1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vaccination", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CriarVacinacao_DataExpiracaoInvalida_RetornaBadRequest()
    {
        // Arrange
        var idPet = await CriarPetAuxiliarAsync();
        var request = new
        {
            IdPet = idPet,
            VaccineName = "V10",
            ApplicationDate = new DateOnly(2026, 1, 10),
            ExpirationDate = new DateOnly(2026, 1, 1),
            Lot = "LOTE1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vaccination", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CriarVacinacao_DadosValidos_RetornaCreated()
    {
        // Arrange
        var idPet = await CriarPetAuxiliarAsync();
        var request = new
        {
            IdPet = idPet,
            VaccineName = "Antirrábica",
            ApplicationDate = new DateOnly(2026, 1, 1),
            ExpirationDate = new DateOnly(2027, 1, 1),
            Lot = "LOTE1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vaccination", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetById_VacinacaoInexistente_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/vaccination/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletarVacinacao_VacinacaoExistente_RetornaNoContent()
    {
        // Arrange
        var idPet = await CriarPetAuxiliarAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/vaccination", new
        {
            IdPet = idPet,
            VaccineName = "V10",
            ApplicationDate = new DateOnly(2026, 1, 1),
            ExpirationDate = new DateOnly(2027, 1, 1),
            Lot = "LOTE1"
        });
        var vaccination = await createResponse.Content.ReadFromJsonAsync<Vaccination>();

        // Act
        var response = await _client.DeleteAsync($"/api/vaccination/{vaccination!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
