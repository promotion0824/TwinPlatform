namespace Authorization.Common.Models;

/// <summary>
/// DTO Class for Application Entity
/// </summary>
public class ApplicationModel
{
    /// <summary>
    /// Unique Identifier for the Application Model.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the Application.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Application Description.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Defines whether the application should support Client Credential Auth (Service-to-Service Authentication).
    /// </summary>
    public bool SupportClientAuthentication { get; set; }
}
