using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using WorkflowCore.Models;

namespace WorkflowCore.Entities;

[Table("WF_JobTypes")]
public class JobTypeEntity
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(500)]
    public string Name { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// represents the last time the record was updated in utc time
    /// the default value in database is the current time in utc
    /// </summary>
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    public static TicketJobType MapToModel(JobTypeEntity jobTypeEntity)
    {
        if (jobTypeEntity == null)
        {
            return null;
        }
        return new TicketJobType
        {
            Id = jobTypeEntity.Id,
            Name = jobTypeEntity.Name
        };
    }

    public static JobTypeEntity MapFromModel(TicketJobType ticketJobType)
    {
        if (ticketJobType == null)
        {
            return null;
        }
        return new JobTypeEntity
        {
            Id = ticketJobType.Id,
            Name = ticketJobType.Name
        };
    }
}


