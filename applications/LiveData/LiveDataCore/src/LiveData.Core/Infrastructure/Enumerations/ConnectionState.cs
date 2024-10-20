namespace Willow.LiveData.Core.Infrastructure.Enumerations;

/// <summary>
/// Connection State.
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Online.
    /// </summary>
    ONLINE = 1,

    /// <summary>
    /// Disrupted.
    /// </summary>
    DISRUPTED = 2,

    /// <summary>
    /// Ready.
    /// </summary>
    READY = 3,

    /// <summary>
    /// Offline.
    /// </summary>
    OFFLINE = 4,

    /// <summary>
    /// Disabled.
    /// </summary>
    DISABLED = 5,

    /// <summary>
    /// Archived.
    /// </summary>
    ARCHIVED = 6,

    /// <summary>
    /// Unknown.
    /// </summary>
    UNKNOWN = 7,
}

/// <summary>
/// Connection Set Status.
/// </summary>
public enum ConnectionSetStatus
{
    /// <summary>
    /// Enabled.
    /// </summary>
    ENABLED = 1,

    /// <summary>
    /// Disabled.
    /// </summary>
    DISABLED = 2,

    /// <summary>
    /// Archived.
    /// </summary>
    ARCHIVED = 3,

    /// <summary>
    /// Unknown.
    /// </summary>
    UNKNOWN = 4,
}
