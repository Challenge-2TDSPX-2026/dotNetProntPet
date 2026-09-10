using ProntPet.Infrastructure;
using ProntPet.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Serilog;
using Serilog.Events;

using System;

// Configuração do Serilog (Log Estruturado)
// Console: acompanhamento em tempo real (dev/produção).
// File: histórico persistido em disco, com rotação diária, para investigação posterior.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/prontpet-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando a ProntPet API...");

    var builder = WebApplication.CreateBuilder(args);

    // Substitui o logger padrão do ASP.NET Core pelo Serilog
    builder.Host.UseSerilog();

    // Infraestrutura: DbContext (Oracle) + Health Checks
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add services to the container.

    builder.Services.AddScoped<IPetService, PetService>();
    builder.Services.AddScoped<ITutorService, TutorService>();
    builder.Services.AddScoped<IVaccinationService, VaccinationService>();
    builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
    builder.Services.AddScoped<IClinicService, ClinicService>();
    builder.Services.AddScoped<IConsultationService, ConsultationService>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {

        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "ProntPet API",
            Version = "v1",
            Description = "API de gerenciamento veterinário"
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Loga uma linha estruturada por requisição HTTP concluída (método, path, status, duração).
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    // Liveness: a aplicação está de pé? (não checa dependências externas)
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    // Readiness: a aplicação está pronta pra receber tráfego? (checa o Oracle)
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação ProntPet falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
