using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Willow.Infrastructure.Swagger
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Policy names map to scopes
            var requiredScopes = context
                .MethodInfo.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy)
                .Distinct()
                .ToList();

            if (requiredScopes.Any())
            {
                operation.Responses.Add(
                    "401",
                    new OpenApiResponse { Description = "Unauthorized" }
                );
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                operation.Security = new List<OpenApiSecurityRequirement>();
                var securityRequirement = new OpenApiSecurityRequirement();
                securityRequirement.Add(
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    requiredScopes
                );
                operation.Security.Add(securityRequirement);
            }
        }
    }
}
