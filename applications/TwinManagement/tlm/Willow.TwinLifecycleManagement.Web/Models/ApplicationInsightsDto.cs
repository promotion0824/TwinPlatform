namespace Willow.TwinLifecycleManagement.Web.Models;

/// <summary>
/// Record for App Insights Configuration.
/// </summary>
public record ApplicationInsightsDto
{
    /// <summary>
    /// Gets or sets the Connection String.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets the App Cloud Role Name.
    /// </summary>
    public string CloudRoleName { get; set; } = null!;
}
