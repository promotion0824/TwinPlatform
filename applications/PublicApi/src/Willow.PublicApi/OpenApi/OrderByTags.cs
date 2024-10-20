namespace Willow.PublicApi.OpenApi;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class OrderByTags(string documentName) : ConditionalFilter(documentName)
{
    protected override void ApplyFilter(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = [.. swaggerDoc.Paths.SelectMany(path => path.Value.Operations.SelectMany(op => op.Value.Tags)).Distinct().OrderBy(t => t.Name)];
    }
}
