using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.dtos;
using ProntPet.Models;
using ProntPet.Services;
using ProntPet.UnitTests.Helpers;

namespace ProntPet.UnitTests.Services;

public class PetServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<PetService>> _loggerMock;
    private readonly PetService _sut; // "sut" = System Under Test

    public PetServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<PetService>>();
        _sut = new PetService(_context, _loggerMock.Object, FakeMeterFactory.Create());
    }

    [Fact]
    public async Task CreateAsync_TutorNaoExiste_RetornaNotFound()
    {
        // Arrange
        var request = new PetRequest(IdTutor: 999, Name: "Rex", Species: "Cachorro",
            Breed: "Vira-lata", BirthDate: null, Weight: 10, Sex: "M");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_PesoNegativo_RetornaValidationError()
    {
        // Arrange
        var tutor = new Tutor { Name = "João", Cpf = "111", Phone = "111", Email = "joao@teste.com", Password = "123", Address = "Rua A" };
        _context.Tutors.Add(tutor);
        await _context.SaveChangesAsync();

        var request = new PetRequest(IdTutor: tutor.Id, Name: "Rex", Species: "Cachorro",
            Breed: "Vira-lata", BirthDate: null, Weight: -5, Sex: "M");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_CriaPetComSucesso()
    {
        // Arrange
        var tutor = new Tutor { Name = "Maria", Cpf = "222", Phone = "222", Email = "maria@teste.com", Password = "123", Address = "Rua B" };
        _context.Tutors.Add(tutor);
        await _context.SaveChangesAsync();

        var request = new PetRequest(IdTutor: tutor.Id, Name: "Bidu", Species: "Cachorro",
            Breed: "Poodle", BirthDate: null, Weight: 8.5m, Sex: "M");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("Bidu", result.Value!.Name);
        Assert.Equal(1, await _context.Pets.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_PetNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 123;

        // Act
        var result = await _sut.GetByIdAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_PetExiste_RetornaPetComSucesso()
    {
        // Arrange
        var pet = new Pet { IdTutor = 1, Name = "Thor", Species = "Cachorro", Sex = "M" };
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(pet.Id);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal("Thor", result.Value!.Name);
    }

    [Fact]
    public async Task UpdateAsync_PetNaoExiste_RetornaNotFound()
    {
        // Arrange
        var request = new PetUpdateRequest("Rex", "Cachorro", "Vira-lata", null, 10, "M");

        // Act
        var result = await _sut.UpdateAsync(999, request);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_PetExiste_AtualizaDadosComSucesso()
    {
        // Arrange
        var pet = new Pet { IdTutor = 1, Name = "Nome Antigo", Species = "Gato", Sex = "F" };
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var request = new PetUpdateRequest("Nome Novo", "Gato", "Siamês", null, 4.2m, "F");

        // Act
        var result = await _sut.UpdateAsync(pet.Id, request);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        var petAtualizado = await _context.Pets.FindAsync(pet.Id);
        Assert.Equal("Nome Novo", petAtualizado!.Name);
    }

    [Fact]
    public async Task DeleteAsync_PetNaoExiste_RetornaNotFound()
    {
        // Arrange
        var idInexistente = 456;

        // Act
        var result = await _sut.DeleteAsync(idInexistente);

        // Assert
        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_PetExiste_RemovePetComSucesso()
    {
        // Arrange
        var pet = new Pet { IdTutor = 1, Name = "Fido", Species = "Cachorro", Sex = "M" };
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteAsync(pet.Id);

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(0, await _context.Pets.CountAsync());
    }
}
