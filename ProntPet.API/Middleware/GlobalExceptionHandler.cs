using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProntPet.Middleware;

/// <summary>
/// Handler global de exceções não tratadas (última linha de defesa).
/// Responsável por:
///   1. Logar o erro em nível <c>Error</c>, com stack trace e o CorrelationId da requisição.
///   2. Devolver ao cliente uma resposta padronizada (RFC 7807 - ProblemDetails),
///      sem vazar detalhes internos (stack trace, mensagens de exceção do EF/Oracle, etc.).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items["CorrelationId"] as string ?? "N/A";

        _logger.LogError(
            exception,
            "Erro não tratado ao processar {Method} {Path}. CorrelationId: {CorrelationId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            correlationId);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Ocorreu um erro inesperado ao processar a requisição.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["correlationId"] = correlationId;

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
