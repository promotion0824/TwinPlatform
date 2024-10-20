namespace Willow.CommandAndControl.Application.Helpers;

/// <summary>
/// Set point command name.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "I'm not sure of the consequences of changing this")]
public enum SetPointCommandName
{
    /// <summary>
    /// At most.
    /// </summary>
    atMost,

    /// <summary>
    /// At least.
    /// </summary>
    atLeast,

    /// <summary>
    /// Set exactly.
    /// </summary>
    set,
}

/// <summary>
/// Actions for requested commands.
/// </summary>
public enum RequestedCommandAction
{
    /// <summary>
    /// Approve.
    /// </summary>
    Approve,

    /// <summary>
    /// Reject.
    /// </summary>
    Reject,

    /// <summary>
    /// Pending.
    /// </summary>
    Pending,
}

/// <summary>
/// Actions for resolved commands.
/// </summary>
public enum ResolvedCommandAction
{
    /// <summary>
    /// Cancel.
    /// </summary>
    Cancel,

    /// <summary>
    /// Execute.
    /// </summary>
    Execute,

    /// <summary>
    /// Suspended.
    /// </summary>
    Suspend,

    /// <summary>
    /// Unsuspend.
    /// </summary>
    Unsuspend,

    /// <summary>
    /// Retry.
    /// </summary>
    Retry,
}
