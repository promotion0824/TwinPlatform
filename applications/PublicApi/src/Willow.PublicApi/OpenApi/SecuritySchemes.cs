namespace Willow.PublicApi.OpenApi;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class SecuritySchemes(string document, Uri tokenEndpoint) : ConditionalFilter(document)
{
    protected override void ApplyFilter(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Components.SecuritySchemes.Clear();

        swaggerDoc.Components.SecuritySchemes.Add("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                ClientCredentials = new OpenApiOAuthFlow
                {
                    TokenUrl = tokenEndpoint,
                },
            },
        });

        // Add the security requirement to all operations except the token endpoint.
        // Normally this would be done with an OperationFilter, but it was not executing.
        foreach (var path in swaggerDoc.Paths)
        {
            if (path.Key == tokenEndpoint.ToString())
            {
                continue;
            }

            foreach (var op in path.Value.Operations)
            {
                op.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2",
                            },
                        },
                        new List<string>()
                    },
                });
            }
        }
    }
}
