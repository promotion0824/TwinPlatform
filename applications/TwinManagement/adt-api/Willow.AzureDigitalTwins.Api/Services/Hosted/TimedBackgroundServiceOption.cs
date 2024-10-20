using System;
using System.Collections.Generic;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted;
public record TimedBackgroundServiceOption
{
    public const string Name = "TimedBackgroundService";

    /// <summary>
    /// Determines how often the hosted the hosted service should start a cycle (each cycle triggers individual jobs if ready).
    /// </summary>
    public TimeSpan ScanDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// List of Jobs to process.
    /// </summary>
    public List<JobProcessorOption> Jobs { get; set; }    

    /// <summary>
    /// Determines if the hosted service is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }
}
