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

public class TutorServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<TutorService>> _loggerMock;
    private readonly TutorService _sut;

    public TutorServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<TutorService>>();
        _sut = new TutorService(_context, _loggerMock.Object, FakeMeterFactory.Create());
    }

    [Fact]
    public async Task GetAllAsync_NenhumTutorCadastrado_RetornaListaVazia()
    {
        // Arrange & Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_TutorNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 999;

        // Act
        var result = await _sut.GetByIdAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_CriaTutorComSucesso()
    {
        // Arrange
        var request = new TutorRequest("Carlos", "123.456.789-00", "11999999999", "carlos@teste.com", "senha123", "Rua C");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal("Carlos", result.Value!.Name);
        Assert.Equal(1, await _context.Tutors.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_TutorNaoExiste_RetornaNotFound()
    {
        // Arrange
        var request = new TutorRequest("Carlos", "123", "111", "c@teste.com", "123", "Rua C");

        // Act
        var result = await _sut.UpdateAsync(999, request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_TutorExiste_AtualizaDadosComSucesso()
    {
        // Arrange
        var tutor = new Tutor { Name = "Antigo", Cpf = "1", Phone = "1", Email = "a@a.com", Password = "1", Address = "Rua A" };
        _context.Tutors.Add(tutor);
        await _context.SaveChangesAsync();

        var request = new TutorRequest("Novo Nome", "1", "1", "a@a.com", "1", "Rua A");

        // Act
        var result = await _sut.UpdateAsync(tutor.Id, request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        var tutorAtualizado = await _context.Tutors.FindAsync(tutor.Id);
        Assert.Equal("Novo Nome", tutorAtualizado!.Name);
    }

    [Fact]
    public async Task DeleteAsync_TutorNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 321;

        // Act
        var result = await _sut.DeleteAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_TutorExiste_RemoveTutorComSucesso()
    {
        // Arrange
        var tutor = new Tutor { Name = "Remover", Cpf = "2", Phone = "2", Email = "b@b.com", Password = "2", Address = "Rua B" };
        _context.Tutors.Add(tutor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteAsync(tutor.Id);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(0, await _context.Tutors.CountAsync());
    }
}
