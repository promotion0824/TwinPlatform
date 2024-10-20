namespace Willow.PublicApi.Transforms;

using System.Collections.Generic;
using Yarp.ReverseProxy.Transforms.Builder;

/// <summary>
/// Removes the origin header.
/// </summary>
/// <remarks>
/// A request made from a browser to a B2C application will fail with a CORS error.
/// Removing the origin header fixes this.
/// It is probably better to configure the B2C application to allow this.
/// </remarks>
internal class OriginHeaderTransform : ITransformFactory
{
    private const string OriginHeader = nameof(OriginHeader);

    public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        if (!transformValues.ContainsKey(OriginHeader))
        {
            return false;
        }

        context.AddRequestHeaderRemove("Origin");

        return true;
    }

    public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues) =>
        transformValues.ContainsKey(OriginHeader);
}
