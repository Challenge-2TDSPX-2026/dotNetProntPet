using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Models;
using ProntPet.Services;
using ProntPet.UnitTests.Helpers;
using ProntPet;

namespace ProntPet.UnitTests.Services;

public class ConsultationServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<ConsultationService>> _loggerMock;
    private readonly ConsultationService _sut;

    public ConsultationServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<ConsultationService>>();
        _sut = new ConsultationService(_context, _loggerMock.Object, FakeMeterFactory.Create());
    }

    [Fact]
    public async Task CreateAsync_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Arrange
        var request = new ConsultationRequest(999, 1, new DateOnly(2026, 1, 1), "Febre", "Infecção", null);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ClinicaNaoExiste_RetornaNotFound()
    {
        // Arrange
        var record = new MedicalRecord { IdPet = 1, BloodType = "A+", IsCastrated = false, LastUpdate = DateTime.UtcNow };
        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        var request = new ConsultationRequest(record.Id, 999, new DateOnly(2026, 1, 1), "Febre", "Infecção", null);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_RegistraConsultaComSucesso()
    {
        // Arrange
        var record = new MedicalRecord { IdPet = 1, BloodType = "A+", IsCastrated = false, LastUpdate = DateTime.UtcNow };
        var clinic = new Clinic { Name = "Clínica Central", Cnpj = "1", Address = "Rua A" };
        _context.MedicalRecords.Add(record);
        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();

        var request = new ConsultationRequest(record.Id, clinic.Id, new DateOnly(2026, 1, 1), "Febre", "Infecção", "Retorno em 7 dias");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal("Infecção", result.Value!.Diagnosis);
    }

    [Fact]
    public async Task GetByIdAsync_ConsultaNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999;

        // Act
        var result = await _sut.GetByIdAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_ConsultaExiste_RemoveConsultaComSucesso()
    {
        // Arrange
        var consultation = new Consultation
        {
            IdMedicalRecord = 1,
            IdClinic = 1,
            ConsultationDate = DateTime.UtcNow,
            Symptoms = "Tosse"
        };
        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteAsync(consultation.Id);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(0, await _context.Consultations.CountAsync());
    }
}
