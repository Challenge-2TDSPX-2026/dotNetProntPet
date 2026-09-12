using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProntPet.Data;

namespace ProntPet.IntegrationTests.Fixtures;

/// <summary>
/// Sobe a aplicação ProntPet real (Program.cs, pipeline de middlewares, Controllers, Services)
/// dentro de um TestServer em memória, substituindo apenas o <see cref="AppDbContext"/> — que
/// na aplicação real aponta para o Oracle — por um provider EF Core InMemory. Assim os testes
/// de integração exercitam o fluxo HTTP completo (roteamento, model binding, Controller,
/// Service, EF Core) sem depender de uma instância Oracle disponível.
///
/// O nome do banco é fixado uma única vez por instância da fixture (Guid), e como ela é
/// compartilhada via CollectionFixture entre todas as classes de teste da "ApiCollection",
/// o banco também é compartilhado entre elas — por isso cada teste usa valores únicos
/// (Guid) em campos com índice único (Cpf, Email, Cnpj etc.) para não colidir uns com os outros.
/// </summary>
public class ApiFactoryFixture : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"ProntPetIntegrationTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove o registro de AppDbContext (Oracle) feito em Infrastructure/DependencyInjection.cs
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}

// Criando a Collection para compartilhar a Fixture (evita subir um TestServer novo a cada classe de teste)
[CollectionDefinition("ApiCollection")]
public class ApiCollection : ICollectionFixture<ApiFactoryFixture>
{
    // Esta classe não tem código. Serve apenas para aplicar a CollectionFixture
    // a todas as classes de teste marcadas com [Collection("ApiCollection")].
}
