namespace Willow.PublicApi.OpenApi;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class PathDocumentFilter(string documentName, string prepend) : ConditionalFilter(documentName)
{
    protected override void ApplyFilter(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.Select(path => new KeyValuePair<string, OpenApiPathItem>($"{prepend}{path.Key}".ToLowerInvariant(), path.Value)).ToDictionary();
        swaggerDoc.Paths.Clear();

        foreach (var path in paths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}
