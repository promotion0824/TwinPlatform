namespace Willow.PublicApi.Transforms;

using Yarp.ReverseProxy.Transforms.Builder;

/// <summary>
/// Adds a key/value pair to the form data of the request body.
/// </summary>
internal class BodyFormTransform : ITransformFactory
{
    private const string BodyFormParameter = nameof(BodyFormParameter);

    private const string Append = nameof(Append);

    public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        if (!transformValues.ContainsKey(BodyFormParameter) || !transformValues.ContainsKey(Append))
        {
            return false;
        }

        context.AddRequestTransform(async transformContext =>
        {
            var httpContext = transformContext.HttpContext;

            if (!httpContext.Request.HasFormContentType)
            {
                return;
            }

            // Read the existing form data
            httpContext.Request.EnableBuffering();
            var form = await httpContext.Request.ReadFormAsync();
            var formData = form.ToDictionary(x => x.Key, x => x.Value.ToString());
            httpContext.Request.Body.Position = 0;

            // Add the new key/value pair
            formData[transformValues[BodyFormParameter]] = transformValues[Append];

            // Update the request body with the modified form data
            transformContext.ProxyRequest.Content = new FormUrlEncodedContent(formData);
        });

        return true;
    }

    public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues) =>
        transformValues.ContainsKey(BodyFormParameter) && transformValues.ContainsKey(Append);
}
