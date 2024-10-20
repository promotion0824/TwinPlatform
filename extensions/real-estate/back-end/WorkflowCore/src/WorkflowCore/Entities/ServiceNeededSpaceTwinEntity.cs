using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities;

[Table("WF_ServiceNeededSpaceTwin")]
public class ServiceNeededSpaceTwinEntity
{
    public Guid Id { get; set; }
    public Guid ServiceNeededId { get; set; }
    [ForeignKey(nameof(ServiceNeededId))]
    public ServiceNeededEntity ServiceNeeded { get; set; }
    [Required]
    [MaxLength(250)]
    public string SpaceTwinId { get; set; }

    /// <summary>
    /// represents the last time the record was updated in utc time
    /// the default value in database is the current time in utc
    /// </summary>
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}

