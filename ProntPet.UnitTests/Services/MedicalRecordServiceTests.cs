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

public class MedicalRecordServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<MedicalRecordService>> _loggerMock;
    private readonly MedicalRecordService _sut;

    public MedicalRecordServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<MedicalRecordService>>();
        _sut = new MedicalRecordService(_context, _loggerMock.Object, FakeMeterFactory.Create());
    }

    [Fact]
    public async Task CreateAsync_PetNaoExiste_RetornaNotFound()
    {
        // Arrange
        var request = new MedicalRecordRequest(999, "A+", null, null, false, null);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_CriaProntuarioComDataDeAtualizacaoPreenchida()
    {
        // Arrange
        var pet = new Pet { IdTutor = 1, Name = "Rex", Species = "Cachorro", Sex = "M" };
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var request = new MedicalRecordRequest(pet.Id, "O-", "Nenhuma", null, true, "MC123");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.NotEqual(default, result.Value!.LastUpdate);
    }

    [Fact]
    public async Task GetByIdAsync_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999;

        // Act
        var result = await _sut.GetByIdAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_ProntuarioExiste_AtualizaDadosComSucesso()
    {
        // Arrange
        var record = new MedicalRecord { IdPet = 1, BloodType = "A+", IsCastrated = false, LastUpdate = DateTime.UtcNow.AddDays(-10) };
        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        var request = new MedicalRecordRequest(1, "B+", "Alergia a poeira", null, true, "MC999");

        // Act
        var result = await _sut.UpdateAsync(record.Id, request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        var recordAtualizado = await _context.MedicalRecords.FindAsync(record.Id);
        Assert.Equal("B+", recordAtualizado!.BloodType);
        Assert.True(recordAtualizado.IsCastrated);
    }

    [Fact]
    public async Task DeleteAsync_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 555;

        // Act
        var result = await _sut.DeleteAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }
}
