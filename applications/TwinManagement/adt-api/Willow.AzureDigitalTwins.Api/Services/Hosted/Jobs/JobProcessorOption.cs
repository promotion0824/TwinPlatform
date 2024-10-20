using System;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;
public record JobProcessorOption : JobBaseOption
{
    /// <summary>
    /// Delay in timespan after which the job is ready for the first run. If not specified, run delay will be used.
    /// </summary>
    public TimeSpan? InitialDelay { get; set; }

    /// <summary>
    /// Timespan dictates on what frequency each jobs should be triggered for every run
    /// </summary>
    public TimeSpan RunDelay { get; set; }

    /// <summary>
    /// States whether the job is enabled or not
    /// </summary>
    public bool Enabled { get; set; }
}
