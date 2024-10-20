namespace Willow.Api.Common.Runtime;

/// <summary>
/// The application context.
/// </summary>
public class ApplicationContext
{
    /// <summary>
    /// Gets or sets the public host name for the application.
    /// </summary>
    public string PublicHostName { get; set; } = default!;
}

/// <summary>
/// The platform application context.
/// </summary>
public class PlatformApplicationContext : ApplicationContext
{
    /// <summary>
    /// Gets or sets the region name.
    /// </summary>
    public string RegionName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the region code.
    /// </summary>
    public string RegionCode { get; set; } = default!;
}
