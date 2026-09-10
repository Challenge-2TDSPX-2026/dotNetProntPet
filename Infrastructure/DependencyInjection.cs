using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProntPet.Data;
using ProntPet.Diagnostics;

namespace ProntPet.Infrastructure;

/// <summary>
/// Configuração centralizada da infraestrutura da aplicação:
/// persistência (Oracle/EF Core), Health Checks e Observabilidade (OpenTelemetry).
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

        // 3. Configuração do OpenTelemetry (Tracing + Métricas)
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(TelemetryConstants.ServiceName);

        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation() // Captura requisições HTTP de entrada (spans automáticos por endpoint)
                    .AddHttpClientInstrumentation() // Captura chamadas HTTP de saída (ex: futuras integrações com serviços externos)
                    .AddSource(TelemetryConstants.ServiceName) // Assina os spans customizados criados nos Services
                    .AddConsoleExporter(); // Exporta para o console (debug local); trocar por AddOtlpExporter() para enviar a um coletor real
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    // Métricas padrão do ASP.NET Core: http.server.request.duration (tempo de resposta)
                    // e contagem de requisições por status code (base para taxa de erros).
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(TelemetryConstants.MeterName) // Assina as métricas de negócio customizadas
                    .AddConsoleExporter();
            });

        return services;
    }
}
