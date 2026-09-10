using Microsoft.EntityFrameworkCore;
using ProntPet.Data;

namespace ProntPet.Infrastructure;

/// <summary>
/// Configuração centralizada da infraestrutura da aplicação:
/// persistência (Oracle/EF Core) e Health Checks.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OracleConnection");

        // 1. Configuração do DbContext (Oracle)
        services.AddDbContext<AppDbContext>(options =>
            options.UseOracle(connectionString,
                b => b.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

        // 2. Configuração dos Health Checks
        services.AddHealthChecks()
            // Liveness: apenas confirma que o processo da API está de pé, sem depender de nada externo.
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
                tags: new[] { "live" })
            // Readiness: confirma que a API consegue de fato conversar com o banco Oracle.
            .AddDbContextCheck<AppDbContext>(
                name: "oracle-database",
                tags: new[] { "ready" });

        return services;
    }
}
