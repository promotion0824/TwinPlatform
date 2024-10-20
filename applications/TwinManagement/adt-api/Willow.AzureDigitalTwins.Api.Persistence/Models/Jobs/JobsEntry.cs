using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Willow.Model.Async;

namespace Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;

/// <summary>
/// Represents the schema of the Jobs table 
/// </summary>
[Index(nameof(JobType), nameof(Status), nameof(IsDeleted))]
public class JobsEntry
{
    /// <summary>
    /// JobId unique Id generated if null 
    /// </summary>
    [Key]
    [MaxLength(256)]
    public string? JobId { get; set; }

    /// <summary>
    /// Refers to Parent Job Id when child jobs are created
    /// Example: Indexing all files would generate child job for each file
    /// </summary>
    [MaxLength(256)]
    public string? ParentJobId { get; set; }

    /// <summary>
    /// Twins, Relationships, Models,TimeSeries, DataValidation, TwinsSync, MTI, Copilot
    /// </summary>
    [MaxLength(32)]
    public required string JobType { get; set; }

    /// <summary>
    /// User Id of the API caller
    /// </summary>
    [MaxLength(256)]
    public required string UserId { get; set; }

    /// <summary>
    /// NotStarted, Inprogress, Error, Completed, Canceled
    /// </summary>
    [Required]
    public required AsyncJobStatus Status { get; set; } = AsyncJobStatus.Queued;

    /// <summary>
    /// Entities processed by the Job
    /// </summary>
    public int? ProgressCurrentCount { get; set; }

    /// <summary>
    /// Total entities to be processed by the Job
    /// </summary>
    public int? ProgressTotalCount { get; set; }

    /// <summary>
    /// Setting to support soft delete
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Example: "Import by File"
    /// </summary>
    [MaxLength(256)]
    public string? UserMessage { get; set; }

    /// <summary>
    /// Entities process status
    /// </summary>
    public string? ProgressStatusMessage { get; set; }

    /// <summary>
    /// URI for Source blob
    /// </summary>
    [MaxLength(2048)]
    public string? SourceResourceUri { get; set; }

    /// <summary>
    /// URI for Target blob
    /// </summary>
    [MaxLength(2048)]
    public string? TargetResourceUri { get; set; }

    /// <summary>
    /// External job
    /// </summary>
    public bool IsExternal { get; set; } = false;

    /// <summary>
    /// Job sub type for MTI jobs
    /// </summary>
    [MaxLength(32)]
    public string? JobSubtype { get; set; }

    /// <summary>
    /// Job creation time
    /// </summary>
    public required DateTimeOffset TimeCreated { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Job updated time
    /// </summary>
    public required DateTimeOffset TimeLastUpdated { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Job Processing start time
    /// </summary>
    public DateTimeOffset? ProcessingStartTime { get; set; }

    /// <summary>
    /// Job Processing end time
    /// </summary>
    public DateTimeOffset? ProcessingEndTime { get; set; }

    public JobsEntryDetail? JobsEntryDetail { get; set; }
}
