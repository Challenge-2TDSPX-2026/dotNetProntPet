using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProntPet.Data;
using ProntPet.Infrastructure;
using ProntPet.Models;
using ProntPet.Services;

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
///
/// Autenticação: a API exige JWT. A fixture injeta uma configuração "Jwt" própria dos testes
/// (independente do appsettings.json de quem roda) e todo client criado por CreateClient()
/// já sai com um token válido no cabeçalho Authorization. Para testar o comportamento sem
/// login (401), use <see cref="CreateAnonymousClient"/>.
/// </summary>
public class ApiFactoryFixture : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"ProntPetIntegrationTests_{Guid.NewGuid()}";

    /// <summary>Configuração JWT usada somente nos testes.</summary>
    public static readonly JwtSettings TestJwt = new()
    {
        Key = "chave-secreta-somente-para-testes-de-integracao-0123456789",
        Issuer = "ProntPet.Tests",
        Audience = "ProntPet.Tests.Clients",
        ExpirationMinutes = 60
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseSetting entra na configuração do host antes da validação das options na inicialização.
        builder.UseSetting("Jwt:Key", TestJwt.Key);
        builder.UseSetting("Jwt:Issuer", TestJwt.Issuer);
        builder.UseSetting("Jwt:Audience", TestJwt.Audience);
        builder.UseSetting("Jwt:ExpirationMinutes", TestJwt.ExpirationMinutes.ToString());

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

    /// <summary>Todo client criado pela fixture já vai autenticado.</summary>
    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateTestToken());
    }

    /// <summary>Client sem token, para testar rotas públicas e respostas 401.</summary>
    public HttpClient CreateAnonymousClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }

    /// <summary>Gera um JWT válido (mesma chave/issuer/audience da API em teste) para um tutor fictício.</summary>
    public static string CreateTestToken()
    {
        var tokenService = new TokenService(Options.Create(TestJwt));
        var tutor = new Tutor { Id = 1, Name = "Tutor de Testes", Email = "tutor.testes@teste.com" };
        return tokenService.GenerateToken(tutor).Token;
    }
}

// Criando a Collection para compartilhar a Fixture (evita subir um TestServer novo a cada classe de teste)
[CollectionDefinition("ApiCollection")]
public class ApiCollection : ICollectionFixture<ApiFactoryFixture>
{
    // Esta classe não tem código. Serve apenas para aplicar a CollectionFixture
    // a todas as classes de teste marcadas com [Collection("ApiCollection")].
}
