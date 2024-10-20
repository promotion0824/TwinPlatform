using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkflowCore.Models;

namespace WorkflowCore.Entities;

[Table("WF_ServiceNeeded")]
public class ServiceNeededEntity
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

    // this is same as request id in Mapped
    public Guid? CategoryId { get; set; }

    public static TicketServiceNeeded MapToModel(ServiceNeededEntity serviceNeededEntity)
    {
        if (serviceNeededEntity == null)
        {
            return null;
        }
        return new TicketServiceNeeded
        {
            Id = serviceNeededEntity.Id,
            Name = serviceNeededEntity.Name,
            CategoryId = serviceNeededEntity.CategoryId
        };
    }
}

