namespace Willow.PublicApi.OpenApi;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal abstract class ConditionalFilter(string documentName) : IDocumentFilter
{
    protected string DocumentName { get; } = documentName;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Info.Version != DocumentName)
        {
            return;
        }

        ApplyFilter(swaggerDoc, context);
    }

    protected abstract void ApplyFilter(OpenApiDocument swaggerDoc, DocumentFilterContext context);
}
