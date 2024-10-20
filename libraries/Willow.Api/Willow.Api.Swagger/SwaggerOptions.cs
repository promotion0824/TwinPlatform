namespace Willow.Api.Swagger;

/// <summary>
/// The swagger options for the application.
/// </summary>
public class SwaggerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether swagger is enabled.
    /// </summary>
    public string SwaggerEnabled { get; set; } = default!;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string DocumentTitle { get; set; } = default!;

    /// <summary>
    /// Gets or sets the document version.
    /// </summary>
    public string DocumentVersion { get; set; } = default!;

    /// <summary>
    /// Gets or sets the route prefix.
    /// </summary>
    public string RoutePrefix { get; set; } = default!;

    /// <summary>
    /// Determines if Swagger is enabled.
    /// </summary>
    /// <returns>The value of SwaggerEnabled as a boolean.</returns>
    public bool IsSwaggerEnabled() => bool.Parse(SwaggerEnabled);
}
