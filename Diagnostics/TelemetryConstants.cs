namespace ProntPet.Diagnostics;

/// <summary>
/// Nomes usados para identificar as fontes de tracing (ActivitySource) e de métricas (Meter)
/// customizadas da aplicação junto ao OpenTelemetry.
/// </summary>
public static class TelemetryConstants
{
    public const string ServiceName = "ProntPet.Api";
    public const string MeterName = "ProntPet.Metrics";
}
