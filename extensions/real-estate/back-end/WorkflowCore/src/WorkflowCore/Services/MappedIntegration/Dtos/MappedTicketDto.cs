using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.MappedIntegration.Dtos;

public class MappedTicketDto : MappedTicketBaseDto
{
    public MappedTicketDto()
    {
        Creator = new MappedUserProfile();
        ClosedBy = new MappedUserProfile();
    }

    /// <summary>
    /// Created date of the ticket
    /// </summary>
    public DateTime? CreatedDate { get; set; }
    /// <summary>
    /// Updated date of the ticket
    /// </summary>
    public DateTime? UpdatedDate { get; set; }
    
    /// <summary>
    /// ticket url in Willow
    /// </summary>
    public string WillowUrl { get; set; }

    public static MappedTicketDto MapFrom(TicketEntity ticketEntity)
    {
        if (ticketEntity is null)
        {
            return null;
        }
        var mappedTicketDto = new MappedTicketDto();
        mappedTicketDto.Id = ticketEntity.Id;
        mappedTicketDto.SiteId = ticketEntity.SiteId;
        mappedTicketDto.Summary = ticketEntity.Summary;
        mappedTicketDto.Description = ticketEntity.Description;
        mappedTicketDto.Priority = Enum.GetName(typeof(Priority), ticketEntity.Priority);
        mappedTicketDto.Status = Enum.GetName(typeof(TicketStatusEnum), ticketEntity.Status);
        mappedTicketDto.SubStatus = ticketEntity.SubStatus?.Name;
        mappedTicketDto.TwinId = ticketEntity.TwinId;
        mappedTicketDto.SpaceTwinId = ticketEntity.SpaceTwinId;
        mappedTicketDto.DueDate = ticketEntity.DueDate;
        mappedTicketDto.CreatedDate = ticketEntity.CreatedDate;
        mappedTicketDto.UpdatedDate = ticketEntity.UpdatedDate;
        mappedTicketDto.ResolvedDate = ticketEntity.ResolvedDate;
        mappedTicketDto.ClosedDate = ticketEntity.ClosedDate;
        mappedTicketDto.RequestType = ticketEntity.Category?.Name;
        mappedTicketDto.JobType = ticketEntity.JobType?.Name;
        mappedTicketDto.Cause = ticketEntity.Cause;
        mappedTicketDto.Solution = ticketEntity.Solution;
        mappedTicketDto.ExternalId = ticketEntity.ExternalId;
        mappedTicketDto.Creator.Id = ticketEntity.CreatorId;
        mappedTicketDto.AssigneeType = ticketEntity.AssigneeType.ToString();
        mappedTicketDto.WillowUrl = "";
        mappedTicketDto.ServiceNeeded = ticketEntity.ServiceNeeded?.Name;
        mappedTicketDto.ExternalCreatedDate = ticketEntity.ExternalCreatedDate;
        mappedTicketDto.ExternalUpdatedDate = ticketEntity.ExternalUpdatedDate;

        mappedTicketDto.Reporter = new MappedReporter
        {
            ReporterId = ticketEntity.ReporterId,
            ReporterName = ticketEntity.ReporterName,
            ReporterEmail = ticketEntity.ReporterEmail,
            ReporterPhone = ticketEntity.ReporterPhone
        };

        if (ticketEntity.AssigneeType == WorkflowCore.Models.AssigneeType.CustomerUser
            && ticketEntity.AssigneeId.HasValue)
        {
            mappedTicketDto.Assignee = new MappedAssignee
            {
                Id = ticketEntity.AssigneeId.Value,
                Name = ticketEntity.AssigneeName,
            };
        }
        else if (ticketEntity.AssigneeType == WorkflowCore.Models.AssigneeType.WorkGroup
            && ticketEntity.AssigneeId.HasValue)
        {
            mappedTicketDto.AssigneeWorkgroup = new MappedWorkgroup
            {
                Id = ticketEntity.AssigneeId,
                Name = ticketEntity.AssigneeName,
                Assignees = new List<MappedAssignee>()

            };
        }
        return mappedTicketDto;

    }

    public static List<MappedTicketDto> MapFromList(List<TicketEntity> ticketEntities)
    {
        return ticketEntities.Select(MapFrom).ToList();
    } 
}

