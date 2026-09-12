using System.Net;
using System.Net.Http.Json;
using ProntPet.IntegrationTests.Fixtures;
using ProntPet.Models;

namespace ProntPet.IntegrationTests.Controllers;

[Collection("ApiCollection")]
public class MedicalRecordControllerIntegrationTests
{
    private readonly HttpClient _client;

    public MedicalRecordControllerIntegrationTests(ApiFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CriarPetAuxiliarAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var tutorResponse = await _client.PostAsJsonAsync("/api/tutor", new
        {
            Name = "Tutor Prontuário",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "123",
            Address = "Rua P"
        });
        var tutor = await tutorResponse.Content.ReadFromJsonAsync<Tutor>();

        var petResponse = await _client.PostAsJsonAsync("/api/pet", new
        {
            IdTutor = tutor!.Id,
            Name = "Pet Prontuário",
            Species = "Gato",
            Breed = (string?)null,
            BirthDate = (DateOnly?)null,
            Weight = 4.0,
            Sex = "F"
        });
        var pet = await petResponse.Content.ReadFromJsonAsync<Pet>();
        return pet!.Id;
    }

    [Fact]
    public async Task CriarProntuario_PetInexistente_RetornaNotFound()
    {
        // Arrange
        var request = new
        {
            IdPet = 999999,
            BloodType = "A+",
            Allergies = (string?)null,
            ChronicDiseases = (string?)null,
            IsCastrated = false,
            MicrochipCode = (string?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/medicalrecord", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CriarProntuario_DadosValidos_RetornaCreated()
    {
        // Arrange
        var idPet = await CriarPetAuxiliarAsync();
        var request = new
        {
            IdPet = idPet,
            BloodType = "O-",
            Allergies = "Nenhuma",
            ChronicDiseases = (string?)null,
            IsCastrated = true,
            MicrochipCode = Guid.NewGuid().ToString("N")[..8]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/medicalrecord", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var record = await response.Content.ReadFromJsonAsync<MedicalRecord>();
        Assert.NotNull(record);
        Assert.Equal("O-", record!.BloodType);
    }

    [Fact]
    public async Task GetById_ProntuarioInexistente_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/medicalrecord/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarProntuario_ProntuarioExistente_RetornaNoContent()
    {
        // Arrange
        var idPet = await CriarPetAuxiliarAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/medicalrecord", new
        {
            IdPet = idPet,
            BloodType = "A+",
            Allergies = (string?)null,
            ChronicDiseases = (string?)null,
            IsCastrated = false,
            MicrochipCode = (string?)null
        });
        var record = await createResponse.Content.ReadFromJsonAsync<MedicalRecord>();

        var dadosAtualizados = new
        {
            IdPet = idPet,
            BloodType = "B+",
            Allergies = "Poeira",
            ChronicDiseases = (string?)null,
            IsCastrated = true,
            MicrochipCode = Guid.NewGuid().ToString("N")[..8]
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/medicalrecord/{record!.Id}", dadosAtualizados);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletarProntuario_ProntuarioExistente_RetornaNoContent()
    {
        // Arrange
        var idPet = await CriarPetAuxiliarAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/medicalrecord", new
        {
            IdPet = idPet,
            BloodType = "A+",
            Allergies = (string?)null,
            ChronicDiseases = (string?)null,
            IsCastrated = false,
            MicrochipCode = (string?)null
        });
        var record = await createResponse.Content.ReadFromJsonAsync<MedicalRecord>();

        // Act
        var response = await _client.DeleteAsync($"/api/medicalrecord/{record!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
