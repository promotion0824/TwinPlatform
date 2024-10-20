namespace Willow.Model.Jobs;

/// <summary>
/// 
/// </summary>
public record JobBaseOption
{
    /// <summary>
    /// Name of the Job. Should be same as the class processing the job.
    /// </summary>
    public string JobName { get; set; } = default!;

    /// <summary>
    /// Type name of the IJobProcessor Implementation to invoke.
    /// </summary>
    public string Use { get; set; } = default!;
}
