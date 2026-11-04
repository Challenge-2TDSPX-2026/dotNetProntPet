using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProntPet.Infrastructure;

/// <summary>
/// Mostra o cadeado (Bearer) e a resposta 401 no Swagger somente nos endpoints que exigem autenticação.
/// As rotas públicas ([AllowAnonymous]: login e cadastro de tutor) ficam sem cadeado.
/// </summary>
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        var isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

        if (isAnonymous || !requiresAuthorization)
            return;

        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Não autenticado: token JWT ausente, inválido ou expirado."
        });

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }] = Array.Empty<string>()
            }
        };
    }
}
