namespace ConnectorCore.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the status of a connector.
/// </summary>
public enum ConnectorStatus
{
    /// <summary>
    /// None.
    /// </summary>
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Online.
    /// </summary>
    [Display(Name = "Online")]
    Online = 1,

    /// <summary>
    /// Online with errors.
    /// </summary>
    [Display(Name = "Online with errors")]
    OnlineWithErrors = 2,

    /// <summary>
    /// Offline.
    /// </summary>
    [Display(Name = "Offline")]
    Offline = 3,
}
