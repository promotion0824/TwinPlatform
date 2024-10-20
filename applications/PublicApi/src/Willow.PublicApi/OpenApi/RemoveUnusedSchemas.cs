namespace Willow.PublicApi.OpenApi;

using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class RemoveUnusedSchemas(string documentName) : ConditionalFilter(documentName)
{
    protected override void ApplyFilter(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var usedSchemas = new HashSet<string>();

        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations.Values)
            {
                foreach (var response in operation.Responses.Values)
                {
                    AddReferencedSchemas(response.Content, usedSchemas, swaggerDoc);
                }

                if (operation.RequestBody != null)
                {
                    AddReferencedSchemas(operation.RequestBody.Content, usedSchemas, swaggerDoc);
                }

                foreach (var parameter in operation.Parameters)
                {
                    AddReferencedSchemas(parameter.Content, usedSchemas, swaggerDoc);
                }
            }
        }

        usedSchemas.Add("SourceType");

        var schemasToRemove = swaggerDoc.Components.Schemas.Keys.Except(usedSchemas).ToList();

        foreach (var schemaName in schemasToRemove)
        {
            swaggerDoc.Components.Schemas.Remove(schemaName);
        }
    }

    private void AddReferencedSchemas(IDictionary<string, OpenApiMediaType> content, HashSet<string> usedSchemas, OpenApiDocument swaggerDoc)
    {
        foreach (var mediaType in content.Values)
        {
            ResolveNestedSchemas(mediaType.Schema, usedSchemas, swaggerDoc);
        }
    }

    private void ResolveNestedSchemas(OpenApiSchema schema, HashSet<string> usedSchemas, OpenApiDocument swaggerDoc)
    {
        if (schema == null)
        {
            return;
        }

        // Handle direct references
        if (schema.Reference != null)
        {
            var schemaId = schema.Reference.Id;

            if (!usedSchemas.Contains(schemaId))
            {
                usedSchemas.Add(schemaId);

                // Recursively resolve referenced schema if it exists
                if (swaggerDoc.Components.Schemas.TryGetValue(schemaId, out var referenceSchema))
                {
                    ResolveNestedSchemas(referenceSchema, usedSchemas, swaggerDoc);
                }
                else
                {
                    // Log missing schema warning
                    Console.WriteLine($"Warning: Schema {schemaId} not found in document.");
                }
            }
        }

        // Handle properties (objects)
        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties.Values)
            {
                ResolveNestedSchemas(property, usedSchemas, swaggerDoc);
            }
        }

        // Handle array items
        if (schema.Items != null)
        {
            ResolveNestedSchemas(schema.Items, usedSchemas, swaggerDoc);
        }

        // Handle additional properties (dictionaries)
        if (schema.AdditionalProperties != null && schema.AdditionalProperties is OpenApiSchema additionalSchema)
        {
            ResolveNestedSchemas(additionalSchema, usedSchemas, swaggerDoc);
        }

        // Handle AllOf (inheritance)
        if (schema.AllOf != null)
        {
            foreach (var subschema in schema.AllOf)
            {
                ResolveNestedSchemas(subschema, usedSchemas, swaggerDoc);
            }
        }

        // Handle OneOf (polymorphism)
        if (schema.OneOf != null)
        {
            foreach (var subschema in schema.OneOf)
            {
                ResolveNestedSchemas(subschema, usedSchemas, swaggerDoc);
            }
        }

        // Handle AnyOf (polymorphism)
        if (schema.AnyOf != null)
        {
            foreach (var subschema in schema.AnyOf)
            {
                ResolveNestedSchemas(subschema, usedSchemas, swaggerDoc);
            }
        }
    }
}
