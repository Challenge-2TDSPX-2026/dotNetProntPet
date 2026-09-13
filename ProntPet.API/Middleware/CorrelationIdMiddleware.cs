using Serilog.Context;

namespace ProntPet.Middleware;

/// <summary>
/// Garante que toda requisição tenha um Correlation Id (recebido do cliente via header
/// ou gerado automaticamente), disponibiliza esse Id no header de resposta e o injeta
/// no contexto de log do Serilog, para que todas as linhas de log emitidas durante o
/// processamento da requisição carreguem a mesma propriedade "CorrelationId".
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items["CorrelationId"] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var existingId) &&
            !string.IsNullOrWhiteSpace(existingId))
        {
            return existingId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
