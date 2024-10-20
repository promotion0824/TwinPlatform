using System.ComponentModel.DataAnnotations;

namespace Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;

/// <summary>
/// Represents a partial schema of the Jobs table
/// </summary>
public class JobsEntryDetail
{
    /// <summary>
    /// JobId unique Id generated if null 
    /// </summary>
    [Key]
    [MaxLength(256)]
    public string? JobId { get; set; }

    /// <summary>
    /// Output of the Job like Deleted Twins
    /// </summary>
    public string? OutputsJson { get; set; }

    /// <summary>
    /// Error results of the job
    /// </summary>
    public string? ErrorsJson { get; set; }

    /// <summary>
    /// Entities involved in the Job 
    /// </summary>
    public string? InputsJson { get; set; }

    /// <summary>
    /// Any custom data related to the Job
    /// </summary>
    public string? CustomData { get; set; }
}
