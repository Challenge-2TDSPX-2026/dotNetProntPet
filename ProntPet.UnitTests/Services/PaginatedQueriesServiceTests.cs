using Microsoft.Extensions.Logging;
using Moq;
using ProntPet.Common;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;
using ProntPet.Services;
using ProntPet.UnitTests.Helpers;

namespace ProntPet.UnitTests.Services;

/// <summary>
/// Testes de paginação, ordenação e filtros das listagens (Tutor, Clinic, Pet, Vaccination e Consultation).
/// </summary>
public class PaginatedQueriesServiceTests
{
    private readonly AppDbContext _context = InMemoryDbContextFactory.Create();

    private TutorService NovoTutorService() =>
        new(_context, new Mock<ILogger<TutorService>>().Object, FakeMeterFactory.Create());

    private ClinicService NovoClinicService() =>
        new(_context, new Mock<ILogger<ClinicService>>().Object, FakeMeterFactory.Create());

    private PetService NovoPetService() =>
        new(_context, new Mock<ILogger<PetService>>().Object, FakeMeterFactory.Create());

    private VaccinationService NovoVaccinationService() =>
        new(_context, new Mock<ILogger<VaccinationService>>().Object, FakeMeterFactory.Create());

    private ConsultationService NovoConsultationService() =>
        new(_context, new Mock<ILogger<ConsultationService>>().Object, FakeMeterFactory.Create());

    private async Task SeedTutorsAsync(params string[] nomes)
    {
        foreach (var nome in nomes)
        {
            var sufixo = Guid.NewGuid().ToString("N")[..8];
            _context.Tutors.Add(new Tutor
            {
                Name = nome,
                Cpf = sufixo,
                Phone = sufixo,
                Email = $"{sufixo}@teste.com",
                Password = "senha123",
                Address = "Rua de Teste, 123"
            });
        }
        await _context.SaveChangesAsync();
    }

    // ---------------------------------------------------------------- Tutor

    [Fact]
    public async Task Tutor_Paginacao_RetornaSomenteItensDaPagina()
    {
        // Arrange
        await SeedTutorsAsync(Enumerable.Range(1, 25).Select(i => $"Tutor {i:00}").ToArray());

        // Act
        var result = await NovoTutorService().GetAllAsync(new TutorQueryParameters { PageNumber = 2, PageSize = 10 });

        // Assert
        Assert.Equal(ServiceStatus.Ok, result.Status);
        var page = result.Value!;
        Assert.Equal(10, page.Items.Count);
        Assert.Equal(25, page.TotalRecords);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal("Tutor 11", page.Items[0].Name);
    }

    [Fact]
    public async Task Tutor_FiltroPorNome_IgnoraMaiusculasEMinusculas()
    {
        // Arrange
        await SeedTutorsAsync("Maria Silva", "João Souza", "MARIANA Lima");

        // Act
        var result = await NovoTutorService().GetAllAsync(new TutorQueryParameters { Name = "maria" });

        // Assert
        Assert.Equal(2, result.Value!.TotalRecords);
        Assert.All(result.Value.Items, t => Assert.Contains("maria", t.Name.ToLower()));
    }

    [Fact]
    public async Task Tutor_OrdenacaoDescendentePorNome_InverteAOrdem()
    {
        // Arrange
        await SeedTutorsAsync("Ana", "Carlos", "Bruno");

        // Act
        var result = await NovoTutorService().GetAllAsync(
            new TutorQueryParameters { SortBy = "name", Ascending = false });

        // Assert
        Assert.Equal(new[] { "Carlos", "Bruno", "Ana" }, result.Value!.Items.Select(t => t.Name));
    }

    [Fact]
    public async Task Tutor_NenhumCadastrado_RetornaPaginaVazia()
    {
        var result = await NovoTutorService().GetAllAsync(new TutorQueryParameters());

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalRecords);
        Assert.Equal(0, result.Value.TotalPages);
    }

    // ---------------------------------------------------------------- Clinic

    [Fact]
    public async Task Clinic_FiltroPorNome_RetornaSomenteCorrespondentes()
    {
        // Arrange
        _context.Clinics.AddRange(
            new Clinic { Name = "Clínica Pet Feliz", Cnpj = "11.111.111/0001-11", Address = "Av. A" },
            new Clinic { Name = "VetCenter", Cnpj = "22.222.222/0001-22", Address = "Av. B" });
        await _context.SaveChangesAsync();

        // Act
        var result = await NovoClinicService().GetAllAsync(new ClinicQueryParameters { Name = "FELIZ" });

        // Assert
        Assert.Single(result.Value!.Items);
        Assert.Equal("Clínica Pet Feliz", result.Value.Items[0].Name);
    }

    // ---------------------------------------------------------------- Pet

    [Fact]
    public async Task Pet_PorTutor_FiltraPorTutorEEspecie()
    {
        // Arrange
        _context.Pets.AddRange(
            new Pet { IdTutor = 1, Name = "Rex", Species = "Cachorro", Sex = "M" },
            new Pet { IdTutor = 1, Name = "Mimi", Species = "Gato", Sex = "F" },
            new Pet { IdTutor = 1, Name = "Tom", Species = "Gato", Sex = "M" },
            new Pet { IdTutor = 2, Name = "Outro", Species = "Gato", Sex = "M" });
        await _context.SaveChangesAsync();

        // Act
        var result = await NovoPetService().GetByTutorAsync(1, new PetQueryParameters { Species = "gato" });

        // Assert
        Assert.Equal(2, result.Value!.TotalRecords);
        Assert.All(result.Value.Items, p => Assert.Equal(1, p.IdTutor));
        Assert.Equal(new[] { "Mimi", "Tom" }, result.Value.Items.Select(p => p.Name)); // padrão: nome crescente
    }

    [Fact]
    public async Task Pet_OrdenacaoPorPesoDescendente_RetornaMaisPesadoPrimeiro()
    {
        // Arrange
        _context.Pets.AddRange(
            new Pet { IdTutor = 1, Name = "Leve", Species = "Gato", Sex = "F", Weight = 3m },
            new Pet { IdTutor = 1, Name = "Pesado", Species = "Cachorro", Sex = "M", Weight = 30m });
        await _context.SaveChangesAsync();

        // Act
        var result = await NovoPetService().GetByTutorAsync(
            1, new PetQueryParameters { SortBy = "weight", Ascending = false });

        // Assert
        Assert.Equal("Pesado", result.Value!.Items[0].Name);
    }

    // ---------------------------------------------------------------- Vaccination

    [Fact]
    public async Task Vaccination_IntervaloDeDatasInvalido_RetornaValidationError()
    {
        var query = new VaccinationQueryParameters
        {
            AppliedFrom = new DateOnly(2026, 10, 20),
            AppliedTo = new DateOnly(2026, 10, 10)
        };

        var result = await NovoVaccinationService().GetByPetAsync(1, query);

        Assert.Equal(ServiceStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task Vaccination_FiltroExpired_SeparaVencidasDasValidas()
    {
        // Arrange
        var hoje = DateTime.Today;
        _context.Vaccinations.AddRange(
            new Vaccination { IdPet = 1, VaccineName = "Antiga", Lot = "L1", ApplicationDate = hoje.AddDays(-400), ExpirationDate = hoje.AddDays(-35) },
            new Vaccination { IdPet = 1, VaccineName = "Atual", Lot = "L2", ApplicationDate = hoje.AddDays(-10), ExpirationDate = hoje.AddDays(355) });
        await _context.SaveChangesAsync();

        // Act
        var vencidas = await NovoVaccinationService().GetByPetAsync(1, new VaccinationQueryParameters { Expired = true });
        var validas = await NovoVaccinationService().GetByPetAsync(1, new VaccinationQueryParameters { Expired = false });

        // Assert
        Assert.Equal("Antiga", Assert.Single(vencidas.Value!.Items).VaccineName);
        Assert.Equal("Atual", Assert.Single(validas.Value!.Items).VaccineName);
    }

    [Fact]
    public async Task Vaccination_FiltroPorPeriodo_IncluiODiaFinalInteiro()
    {
        // Arrange
        _context.Vaccinations.AddRange(
            new Vaccination { IdPet = 1, VaccineName = "A", Lot = "L1", ApplicationDate = new DateTime(2026, 10, 10), ExpirationDate = new DateTime(2027, 10, 10) },
            new Vaccination { IdPet = 1, VaccineName = "B", Lot = "L2", ApplicationDate = new DateTime(2026, 10, 20), ExpirationDate = new DateTime(2027, 10, 20) },
            new Vaccination { IdPet = 1, VaccineName = "C", Lot = "L3", ApplicationDate = new DateTime(2026, 10, 25), ExpirationDate = new DateTime(2027, 10, 25) });
        await _context.SaveChangesAsync();

        // Act: de 10/10 até 20/10 (inclusive)
        var result = await NovoVaccinationService().GetByPetAsync(1, new VaccinationQueryParameters
        {
            AppliedFrom = new DateOnly(2026, 10, 10),
            AppliedTo = new DateOnly(2026, 10, 20),
            SortBy = "applicationDate",
            Ascending = true
        });

        // Assert
        Assert.Equal(new[] { "A", "B" }, result.Value!.Items.Select(v => v.VaccineName));
    }

    // ---------------------------------------------------------------- Consultation

    [Fact]
    public async Task Consultation_FiltroPorClinicaEBusca_RetornaSomenteCorrespondentes()
    {
        // Arrange
        _context.Consultations.AddRange(
            new Consultation { IdMedicalRecord = 1, IdClinic = 1, ConsultationDate = new DateTime(2026, 10, 1), Diagnosis = "Gastrite" },
            new Consultation { IdMedicalRecord = 1, IdClinic = 2, ConsultationDate = new DateTime(2026, 10, 2), Diagnosis = "Gastrite" },
            new Consultation { IdMedicalRecord = 1, IdClinic = 1, ConsultationDate = new DateTime(2026, 10, 3), Diagnosis = "Otite" },
            new Consultation { IdMedicalRecord = 9, IdClinic = 1, ConsultationDate = new DateTime(2026, 10, 4), Diagnosis = "Gastrite" });
        await _context.SaveChangesAsync();

        // Act
        var result = await NovoConsultationService().GetByMedicalRecordAsync(
            1, new ConsultationQueryParameters { IdClinic = 1, Search = "gastrite" });

        // Assert
        var consulta = Assert.Single(result.Value!.Items);
        Assert.Equal(new DateTime(2026, 10, 1), consulta.ConsultationDate);
    }

    [Fact]
    public async Task Consultation_PadraoOrdenaPorDataDecrescente()
    {
        // Arrange
        _context.Consultations.AddRange(
            new Consultation { IdMedicalRecord = 1, IdClinic = 1, ConsultationDate = new DateTime(2026, 1, 1) },
            new Consultation { IdMedicalRecord = 1, IdClinic = 1, ConsultationDate = new DateTime(2026, 6, 1) });
        await _context.SaveChangesAsync();

        // Act
        var result = await NovoConsultationService().GetByMedicalRecordAsync(1, new ConsultationQueryParameters());

        // Assert
        Assert.Equal(new DateTime(2026, 6, 1), result.Value!.Items[0].ConsultationDate);
    }

    [Fact]
    public async Task Consultation_IntervaloDeDatasInvalido_RetornaValidationError()
    {
        var query = new ConsultationQueryParameters
        {
            From = new DateOnly(2026, 10, 20),
            To = new DateOnly(2026, 10, 10)
        };

        var result = await NovoConsultationService().GetByMedicalRecordAsync(1, query);

        Assert.Equal(ServiceStatus.ValidationError, result.Status);
    }
}
