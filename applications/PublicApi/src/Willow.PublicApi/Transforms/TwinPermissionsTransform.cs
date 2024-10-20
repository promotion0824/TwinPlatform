namespace Willow.PublicApi.Transforms;

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Willow.PublicApi.Authorization;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

internal class TwinPermissionsTransform(ILogger<TwinPermissionsTransform> logger, IAuthorizationService authorizationService, IResourceChecker resourceChecker) : ITransformFactory
{
    private const string TwinIdRoute = nameof(TwinIdRoute);
    private const string TwinIdQuery = nameof(TwinIdQuery);
    private const string TwinIdBody = nameof(TwinIdBody);
    private const string ExternalIdBody = nameof(ExternalIdBody);

    private readonly string[] knownValues = [TwinIdRoute, TwinIdQuery, TwinIdBody, ExternalIdBody];

    public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        if (transformValues.TryGetValue(TwinIdRoute, out string? name))
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                // You must provide a name for the route value.
                return false;
            }

            // A single twin ID is expected in the route.
            context.AddRequestTransform(transformContext => TwinIdRouteTransform(transformContext, name));

            return true;
        }
        else if (transformValues.TryGetValue(TwinIdQuery, out name))
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                // You must provide a name for the query parameter.
                return false;
            }

            // One or more twin IDs are expected in the query.
            context.AddRequestTransform(transformContext => TwinIdQueryTransform(transformContext, name));

            return true;
        }
        else if (transformValues.TryGetValue(TwinIdBody, out name) && transformValues.TryGetValue(ExternalIdBody, out string? externalIdName))
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(externalIdName))
            {
                // JSON property names are expected in the body.
                return false;
            }

            // One or more twin IDs and external IDs are expected in the body.
            // "name" is the name of JSON property that holds the twin IDs.
            // "externalIdName" is the name of JSON property that holds the external IDs.
            context.AddRequestTransform(transformContext => ExternalIdBodyTransform(transformContext, name, externalIdName));

            return true;
        }
        else if (name is not null)
        {
            // One or more twin IDs are expected in the body.
            // "name" is the name of JSON property that holds the twin IDs.
            context.AddRequestTransform(transformContext => TwinIdBodyTransform(transformContext, name));

            return true;
        }

        return false;
    }

    public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        bool hasValidKeys = transformValues.Keys.All(knownValues.Contains);
        bool hasValidRoute = transformValues.TryGetValue(TwinIdRoute, out string? route);
        bool hasValidQuery = transformValues.TryGetValue(TwinIdQuery, out string? query);
        bool hasValidBody = transformValues.TryGetValue(TwinIdBody, out string? body);
        bool hasValidExternalId = transformValues.TryGetValue(ExternalIdBody, out string? externalId);

        bool hasValidCombination = hasValidRoute ^ hasValidQuery ^ (hasValidBody || hasValidExternalId);
        bool hasNonEmptyValues = !string.IsNullOrWhiteSpace(route) || !string.IsNullOrWhiteSpace(query) || !string.IsNullOrWhiteSpace(body) || !string.IsNullOrWhiteSpace(externalId);

        if (transformValues.ContainsKey(TwinIdBody) && transformValues.ContainsKey(ExternalIdBody))
        {
            hasNonEmptyValues = !string.IsNullOrWhiteSpace(body) && !string.IsNullOrWhiteSpace(externalId);
        }
        else if (transformValues.ContainsKey(TwinIdBody))
        {
            // TwinIdBody is allowed to be empty.
            hasNonEmptyValues = true;
        }

        return hasValidKeys && hasValidCombination && hasNonEmptyValues;
    }

    private async ValueTask TwinIdRouteTransform(RequestTransformContext transformContext, string name)
    {
        var httpContext = transformContext.HttpContext;

        httpContext.Request.RouteValues.TryGetValue(name, out var twinId);

        var result = await authorizationService.AuthorizeAsync(httpContext.User, httpContext, new SingleTwinExpressionRequirement(twinId as string));

        if (!result.Succeeded)
        {
            transformContext.HttpContext.Response.StatusCode = 403;
        }
    }

    private async ValueTask TwinIdQueryTransform(RequestTransformContext transformContext, string name)
    {
        var httpContext = transformContext.HttpContext;

        httpContext.Request.Query.TryGetValue(name, out var twinId);

        var twinIds = twinId.ToArray();

        if (twinIds is null)
        {
            transformContext.HttpContext.Response.StatusCode = 400;
            return;
        }

        if (twinIds.Length == 1)
        {
            var result = await authorizationService.AuthorizeAsync(httpContext.User, httpContext, new SingleTwinExpressionRequirement(twinId));

            if (!result.Succeeded)
            {
                transformContext.HttpContext.Response.StatusCode = 403;
            }

            return;
        }

        var filteredTwinIds = await resourceChecker.FilterTwinPermission(twinIds);

        if (!filteredTwinIds.Any())
        {
            // The client does not have permission to any requested IDs.
            transformContext.HttpContext.Response.StatusCode = 403;
            return;
        }
        else if (filteredTwinIds.Count() == twinIds.Length)
        {
            // Nothing to filter.
            return;
        }

        transformContext.Query.Collection.Remove(name);
        transformContext.Query.Collection.Add(name, new(filteredTwinIds.ToArray()));
    }

    /// <summary>
    /// Transform a body that has twin IDs.
    /// </summary>
    /// <remarks>
    /// Supports
    /// * A root array of strings (if <paramref name="name"/> is empty
    /// * An object or array of objects with a named property that can be either
    /// an array of strings, or a single string.
    /// </remarks>
    /// <param name="transformContext">Thr request transform context.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>A task.</returns>
    private async ValueTask TwinIdBodyTransform(RequestTransformContext transformContext, string name)
    {
        if (resourceChecker.HasFullPermissions())
        {
            return;
        }

        if (transformContext.ProxyRequest.Content is null)
        {
            logger.LogWarning("Proxy request content is null");
            return;
        }

        JsonNode? doc;

        try
        {
            // There seems to be a bug where ReadAsStreamAsync throws a NotImplementedException if it is called before another read method.
            // Instead, we'll read the content as a byte array and then create a MemoryStream from that.
            MemoryStream memoryStream = new(await transformContext.ProxyRequest.Content.ReadAsByteArrayAsync());
            doc = await JsonNode.ParseAsync(memoryStream);

            if (doc is null)
            {
                logger.LogWarning("Unable to parse body as JSON when trying to get twin IDs");
                return;
            }
        }
        catch (JsonException jex)
        {
            logger.LogWarning(jex, "Unable to parse body as JSON when trying to get twin IDs");
            return;
        }

        var allowedIds = await resourceChecker.GetAllowedTwins();
        var newRoot = GetTwinNodes(doc.Root, name, allowedIds);

        if (newRoot is null || (newRoot.GetValueKind() == JsonValueKind.Array && newRoot.AsArray().Count == 0))
        {
            transformContext.HttpContext.Response.StatusCode = 403;
            return;
        }
        else if (newRoot.GetValueKind() != JsonValueKind.Array && (newRoot[name] is null || (newRoot[name]!.GetValueKind() == JsonValueKind.Array && newRoot[name]!.AsArray().Count == 0)))
        {
            transformContext.HttpContext.Response.StatusCode = 403;
            return;
        }

        var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

        newRoot.WriteTo(writer);

        await writer.FlushAsync();
        stream.Position = 0;

        transformContext.ProxyRequest.Content = new StreamContent(stream);

        transformContext.ProxyRequest.Content.Headers.ContentType = new("application/json")
        {
            CharSet = "utf-8",
        };
    }

    /// <summary>
    /// Transform a body that has a combination of twin IDs and external IDs.
    /// </summary>
    /// <remarks>
    /// Supports objects with both properties or an array of such objects.
    /// This does not support arrays of both twin IDs and external IDs.
    /// External ID is only checked if twin ID is not present.
    /// </remarks>
    /// <param name="transformContext">The request transform context.</param>
    /// <param name="twinIdName">The name of the twin ID property.</param>
    /// <param name="externalIdName">The name of the external ID property.</param>
    /// <returns>A task.</returns>
    private async ValueTask ExternalIdBodyTransform(RequestTransformContext transformContext, string twinIdName, string externalIdName)
    {
        if (resourceChecker.HasFullPermissions())
        {
            return;
        }

        if (transformContext.ProxyRequest.Content is null)
        {
            logger.LogWarning("Proxy request content is null");
            return;
        }

        JsonNode? doc;

        try
        {
            // There seems to be a bug where ReadAsStreamAsync throws a NotImplementedException if it is called before another read method.
            // Instead, we'll read the content as a byte array and then create a MemoryStream from that.
            MemoryStream memoryStream = new(await transformContext.ProxyRequest.Content.ReadAsByteArrayAsync());
            doc = await JsonNode.ParseAsync(memoryStream);

            if (doc is null)
            {
                logger.LogWarning("Unable to parse body as JSON when trying to get twin IDs");
                return;
            }
        }
        catch (JsonException jex)
        {
            logger.LogWarning(jex, "Unable to parse body as JSON when trying to get twin IDs");
            return;
        }

        //var ids = GetIds(doc.Root, name, externalIdName);
        var allowedIds = await resourceChecker.GetAllowedTwins();
        var newRoot = GetTwinNodes(doc.Root, twinIdName, externalIdName, allowedIds);

        if (newRoot is null)
        {
            transformContext.HttpContext.Response.StatusCode = 403;
            return;
        }

        var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

        newRoot.WriteTo(writer);

        await writer.FlushAsync();
        stream.Position = 0;

        transformContext.ProxyRequest.Content = new StreamContent(stream);

        transformContext.ProxyRequest.Content.Headers.ContentType = new("application/json")
        {
            CharSet = "utf-8",
        };
    }

    private static JsonNode? GetTwinNodes(JsonNode? node, string twinIdName, IEnumerable<TwinIds> allowedIds)
    {
        if (node is null)
        {
            return null;
        }

        if (node.GetValueKind() == JsonValueKind.Array)
        {
            var nodes = node.AsArray().Select(e => GetTwinNodes(e, twinIdName, allowedIds)).Where(t => t is not null).ToArray();

            return nodes.Length == 0 ? null : new JsonArray(nodes);
        }
        else if (string.IsNullOrWhiteSpace(twinIdName) && node.GetValueKind() == JsonValueKind.String && node.AsValue().TryGetValue(out string? twinId) && allowedIds.Any(id => id.TwinId == twinId))
        {
            return node.DeepClone();
        }
        else if (!string.IsNullOrWhiteSpace(twinIdName))
        {
            // This currently supports only one level of depth.
            JsonNode? twinIdProperty = node[twinIdName];

            if (twinIdProperty is null)
            {
                return null;
            }
            else if (twinIdProperty.GetValueKind() == JsonValueKind.Array)
            {
                var nodes = twinIdProperty.AsArray().Select(e => GetTwinNodes(e, string.Empty, allowedIds)).Where(t => t is not null).ToArray();

                node[twinIdName] = nodes.Length == 0 ? null : new JsonArray(nodes);

                return node.DeepClone();
            }
            else if (twinIdProperty.AsValue().TryGetValue(out twinId) && !string.IsNullOrEmpty(twinId) && allowedIds.Any(id => id.TwinId == twinId))
            {
                // Twin ID matches, we're good.
                return node.DeepClone();
            }
        }

        return null;
    }

    private static JsonNode? GetTwinNodes(JsonNode? node, string twinIdName, string externalIdName, IEnumerable<TwinIds> allowedIds)
    {
        if ((string.IsNullOrEmpty(twinIdName) && !string.IsNullOrEmpty(externalIdName)) ||
            (!string.IsNullOrEmpty(twinIdName) && string.IsNullOrEmpty(externalIdName)))
        {
            throw new ArgumentException("Both Twin ID and External ID must be provided, or neither", nameof(twinIdName));
        }

        if (node is null)
        {
            return null;
        }

        switch (node.GetValueKind())
        {
            case JsonValueKind.Array:
                var nodes = node.AsArray().Select(e => GetTwinNodes(e, twinIdName, externalIdName, allowedIds)).Where(t => t is not null).ToArray();

                return nodes.Length == 0 ? null : new JsonArray(nodes);

            case JsonValueKind.Object:

                // This currently supports only one level of depth.
                JsonNode? twinIdProperty = node[twinIdName];
                JsonNode? externalIdProperty = node[externalIdName];

                if (twinIdProperty is null && externalIdProperty is null)
                {
                    return null;
                }

                if ((twinIdProperty is not null && twinIdProperty.GetValueKind() != JsonValueKind.String) || (externalIdProperty is not null && externalIdProperty.GetValueKind() != JsonValueKind.String))
                {
                    // We do not allow arrays of twins and external IDs, as we can't link one twin ID to an external ID.
                    throw new InvalidOperationException("Twin ID and External ID must be strings");
                }

                bool twinIdPropertyIsEmpty = twinIdProperty is null || (twinIdProperty.AsValue().TryGetValue(out string? twinId) && string.IsNullOrEmpty(twinId));

                node[twinIdName] = twinIdProperty is not null && twinIdProperty.AsValue().TryGetValue(out twinId) && (string.IsNullOrEmpty(twinId) || allowedIds.Any(id => id.TwinId == twinId)) ? twinIdProperty : null;

                // We only examine the external ID if there is no twin ID specified.
                node[externalIdName] = !twinIdPropertyIsEmpty ||
                                        (externalIdProperty is not null &&
                                        externalIdProperty.AsValue().TryGetValue(out string? externalId) &&
                                        allowedIds.Any(id => id.ExternalId == externalId)) ? externalIdProperty : null;

                return (!twinIdPropertyIsEmpty && node[twinIdName] is null) || node[externalIdName] is null ? null : node.DeepClone();

            default:
                throw new InvalidOperationException("Unsupported JSON schema");
        }
    }
}
