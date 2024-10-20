namespace Willow.PublicApi.OpenApi;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class OAuthTokenEndpointDocumentFilter(string documentName) : ConditionalFilter(documentName)
{
    protected override void ApplyFilter(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathItem = new OpenApiPathItem();

        pathItem.AddOperation(OperationType.Post, new OpenApiOperation
        {
            Summary = "Get OAuth2 Token",
            Description = "Generates an OAuth2 token.",
            OperationId = "GetOAuth2Token",
            Tags = new List<OpenApiTag> { new OpenApiTag { Name = "OAuth2" } },
            Responses = new OpenApiResponses
            {
                {
                    "200", new OpenApiResponse
                    {
                        Description = "Token response",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json", new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            { "access_token", new OpenApiSchema { Type = "string" } },
                                            { "token_type", new OpenApiSchema { Type = "string" } },
                                            { "expires_in", new OpenApiSchema { Type = "integer" } },
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/x-www-form-urlencoded", new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    { "grant_type", new OpenApiSchema { Type = "string" } },
                                    { "client_id", new OpenApiSchema { Type = "string" } },
                                    { "client_secret", new OpenApiSchema { Type = "string" } },
                                },
                                Required = new HashSet<string> { "grant_type", "client_id", "client_secret" },
                            },
                        }
                    },
                },
            },
        });

        swaggerDoc.Paths.Add($"/{DocumentName}/oauth2/token", pathItem);
    }
}
