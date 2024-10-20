namespace Willow.Model.Jobs;

/// <summary>
/// Job Execution Context.
/// </summary>
public record JobExecutionContext
{
    /// <summary>
    /// Is Job called on demand.
    /// </summary>
    public bool IsOnDemand = false;

    /// <summary>
    /// Is Job called on the startup.
    /// </summary>
    public bool IsStartup = false;
}
