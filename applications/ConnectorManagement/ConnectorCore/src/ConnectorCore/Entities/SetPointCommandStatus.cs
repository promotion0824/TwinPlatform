namespace ConnectorCore.Entities;

/// <summary>
/// Set point command status.
/// </summary>
public enum SetPointCommandStatus
{
    /// <summary>
    /// Submitted.
    /// </summary>
    Submitted = 0,

    /// <summary>
    /// Activation failed.
    /// </summary>
    ActivationFailed = 1,

    /// <summary>
    /// Active.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Reset failed.
    /// </summary>
    ResetFailed = 3,

    /// <summary>
    /// Completed.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Deleted.
    /// </summary>
    Deleted = 5,
}
