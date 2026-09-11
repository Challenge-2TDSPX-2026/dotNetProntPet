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

public class ClinicServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<ClinicService>> _loggerMock;
    private readonly ClinicService _sut;

    public ClinicServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<ClinicService>>();
        _sut = new ClinicService(_context, _loggerMock.Object, FakeMeterFactory.Create());
    }

    [Fact]
    public async Task GetAllAsync_NenhumaClinicaCadastrada_RetornaListaVazia()
    {
        // Arrange & Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_CriaClinicaComSucesso()
    {
        // Arrange
        var request = new ClinicRequest("Clínica Pet Feliz", "12.345.678/0001-99", "Av. Central, 100");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal("Clínica Pet Feliz", result.Value!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ClinicaNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999;

        // Act
        var result = await _sut.GetByIdAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_ClinicaExiste_AtualizaDadosComSucesso()
    {
        // Arrange
        var clinic = new Clinic { Name = "Antiga", Cnpj = "1", Address = "Rua X" };
        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();

        var request = new ClinicRequest("Nova Clínica", "1", "Rua Y");

        // Act
        var result = await _sut.UpdateAsync(clinic.Id, request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        var clinicAtualizada = await _context.Clinics.FindAsync(clinic.Id);
        Assert.Equal("Nova Clínica", clinicAtualizada!.Name);
    }

    [Fact]
    public async Task DeleteAsync_ClinicaNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 777;

        // Act
        var result = await _sut.DeleteAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }
}
