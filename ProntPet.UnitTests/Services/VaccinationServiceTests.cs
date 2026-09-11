using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;
using ProntPet.Services;
using ProntPet.UnitTests.Helpers;

namespace ProntPet.UnitTests.Services;

public class VaccinationServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<VaccinationService>> _loggerMock;
    private readonly VaccinationService _sut;

    public VaccinationServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<VaccinationService>>();
        _sut = new VaccinationService(_context, _loggerMock.Object, FakeMeterFactory.Create());
    }

    [Fact]
    public async Task CreateAsync_PetNaoExiste_RetornaNotFound()
    {
        // Arrange
        var request = new VaccinationRequest(999, "V10", new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), "LOTE1");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_DataExpiracaoAnteriorAplicacao_RetornaValidationError()
    {
        // Arrange
        var pet = new Pet { IdTutor = 1, Name = "Rex", Species = "Cachorro", Sex = "M" };
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var request = new VaccinationRequest(pet.Id, "V10", new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 1), "LOTE1");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_RegistraVacinacaoComSucesso()
    {
        // Arrange
        var pet = new Pet { IdTutor = 1, Name = "Rex", Species = "Cachorro", Sex = "M" };
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var request = new VaccinationRequest(pet.Id, "Antirrábica", new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), "LOTE1");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal("Antirrábica", result.Value!.VaccineName);
    }

    [Fact]
    public async Task GetByIdAsync_VacinacaoNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999;

        // Act
        var result = await _sut.GetByIdAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_VacinacaoNaoExiste_RetornaNotFound()
    {
        // Arrange
        var request = new VaccinationRequest(1, "V10", new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), "LOTE1");

        // Act
        var result = await _sut.UpdateAsync(999, request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_VacinacaoExiste_RemoveVacinacaoComSucesso()
    {
        // Arrange
        var vaccination = new Vaccination
        {
            IdPet = 1,
            VaccineName = "V10",
            ApplicationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            Lot = "LOTE1"
        };
        _context.Vaccinations.Add(vaccination);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteAsync(vaccination.Id);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(0, await _context.Vaccinations.CountAsync());
    }
}
