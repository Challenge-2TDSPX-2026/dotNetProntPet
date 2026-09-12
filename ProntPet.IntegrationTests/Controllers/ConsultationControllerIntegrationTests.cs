using System.Net;
using System.Net.Http.Json;
using ProntPet.IntegrationTests.Fixtures;
using ProntPet.Models;

namespace ProntPet.IntegrationTests.Controllers;

[Collection("ApiCollection")]
public class ConsultationControllerIntegrationTests
{
    private readonly HttpClient _client;

    public ConsultationControllerIntegrationTests(ApiFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(int idMedicalRecord, int idClinic)> CriarProntuarioEClinicaAuxiliaresAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var tutorResponse = await _client.PostAsJsonAsync("/api/tutor", new
        {
            Name = "Tutor Consulta",
            Cpf = suffix,
            Phone = suffix,
            Email = $"{suffix}@teste.com",
            Password = "123",
            Address = "Rua C"
        });
        var tutor = await tutorResponse.Content.ReadFromJsonAsync<Tutor>();

        var petResponse = await _client.PostAsJsonAsync("/api/pet", new
        {
            IdTutor = tutor!.Id,
            Name = "Pet Consulta",
            Species = "Cachorro",
            Breed = (string?)null,
            BirthDate = (DateOnly?)null,
            Weight = 9.0,
            Sex = "M"
        });
        var pet = await petResponse.Content.ReadFromJsonAsync<Pet>();

        var recordResponse = await _client.PostAsJsonAsync("/api/medicalrecord", new
        {
            IdPet = pet!.Id,
            BloodType = "A+",
            Allergies = (string?)null,
            ChronicDiseases = (string?)null,
            IsCastrated = false,
            MicrochipCode = (string?)null
        });
        var record = await recordResponse.Content.ReadFromJsonAsync<MedicalRecord>();

        var clinicResponse = await _client.PostAsJsonAsync("/api/clinic", new
        {
            Name = "Clínica Consulta",
            Cnpj = suffix,
            Address = "Rua D"
        });
        var clinic = await clinicResponse.Content.ReadFromJsonAsync<Clinic>();

        return (record!.Id, clinic!.Id);
    }

    [Fact]
    public async Task CriarConsulta_ProntuarioInexistente_RetornaNotFound()
    {
        // Arrange
        var request = new
        {
            IdMedicalRecord = 999999,
            IdClinic = 1,
            ConsultationDate = new DateOnly(2026, 1, 1),
            Symptoms = "Febre",
            Diagnosis = (string?)null,
            Observations = (string?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/consultation", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CriarConsulta_DadosValidos_RetornaCreated()
    {
        // Arrange
        var (idMedicalRecord, idClinic) = await CriarProntuarioEClinicaAuxiliaresAsync();
        var request = new
        {
            IdMedicalRecord = idMedicalRecord,
            IdClinic = idClinic,
            ConsultationDate = new DateOnly(2026, 1, 1),
            Symptoms = "Febre",
            Diagnosis = "Infecção",
            Observations = "Retorno em 7 dias"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/consultation", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var consultation = await response.Content.ReadFromJsonAsync<Consultation>();
        Assert.Equal("Infecção", consultation!.Diagnosis);
    }

    [Fact]
    public async Task GetById_ConsultaInexistente_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/consultation/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletarConsulta_ConsultaExistente_RetornaNoContent()
    {
        // Arrange
        var (idMedicalRecord, idClinic) = await CriarProntuarioEClinicaAuxiliaresAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/consultation", new
        {
            IdMedicalRecord = idMedicalRecord,
            IdClinic = idClinic,
            ConsultationDate = new DateOnly(2026, 1, 1),
            Symptoms = "Tosse",
            Diagnosis = (string?)null,
            Observations = (string?)null
        });
        var consultation = await createResponse.Content.ReadFromJsonAsync<Consultation>();

        // Act
        var response = await _client.DeleteAsync($"/api/consultation/{consultation!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
