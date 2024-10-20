namespace Willow.CommandAndControl.Application.Requests.Sites;

/// <summary>
/// Site Information.
/// </summary>
public class SiteDto
{
    /// <summary>
    /// Gets or sets the site ID.
    /// </summary>
    public required string SiteId { get; set; }

    /// <summary>
    /// Gets or sets the site name or address of the site.
    /// </summary>
    public required string SiteName { get; set; }
}
