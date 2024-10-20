using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using WorkflowCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace WorkflowCore.Entities;

[Table("WF_TicketSubStatus")]
public class TicketSubStatusEntity
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(500)]
    public string Name { get; set; }

    public static TicketSubStatus MapToModel(TicketSubStatusEntity ticketSubStatusEntity)
    {
        if (ticketSubStatusEntity == null)
        {
            return null;
        }
        return new TicketSubStatus
        {
            Id = ticketSubStatusEntity.Id,
            Name = ticketSubStatusEntity.Name
        };
    }

    public static TicketSubStatusEntity MapFromModel(TicketSubStatus ticketSubStatus)
    {
        if (ticketSubStatus == null)
        {
            return null;
        }
        return new TicketSubStatusEntity
        {
            Id = ticketSubStatus.Id,
            Name = ticketSubStatus.Name
        };
    }

    public static List<TicketSubStatus> MapTo(List<TicketSubStatusEntity> subStatus)
    {
        if(subStatus is null)
        {
            return new List<TicketSubStatus>();
        }
        return subStatus.Select(x=>MapToModel(x)).ToList();
    }
}

