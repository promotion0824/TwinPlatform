namespace Willow.Infrastructure.Swagger;

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Willow.LiveData.Core.Infrastructure.Configuration;

internal class SecurityRequirementsOperationFilter(IOptions<Auth0Configuration> auth0Options) : IOperationFilter
{
    private bool authEnabled = !string.IsNullOrEmpty(auth0Options.Value.Domain) && !string.IsNullOrEmpty(auth0Options.Value.Audience);

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Policy names map to scopes
        var requiredScopes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .Distinct()
            .ToList();

        var allowAnonymous = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if ((requiredScopes.Any() || authEnabled) && !allowAnonymous)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            operation.Security = new List<OpenApiSecurityRequirement>();
            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" },
                    },
                    requiredScopes
                },
            };
            operation.Security.Add(securityRequirement);
        }
    }
}
