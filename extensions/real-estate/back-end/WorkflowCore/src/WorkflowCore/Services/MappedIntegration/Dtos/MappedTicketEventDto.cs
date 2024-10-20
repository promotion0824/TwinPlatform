using System;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.MappedIntegration.Dtos;
/// <summary>
/// Request DTO to send ticket events to Mapped API
/// </summary>
public class MappedTicketEventDto
{
    public Guid EventId { get; set; }
    public string EventType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public MappedTicketData Data { get; set; }


    public class MappedTicketData : MappedTicketBaseDto
    {
        public MappedTicketData()
        {
            ClosedBy = new MappedUserProfile();
        }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public static MappedTicketEventDto MapFrom(TicketEventDto ticketEventDto)
    {
        if (ticketEventDto is null || ticketEventDto.Data is null)
        {
            return null;
        }
        var ticketData = ticketEventDto.Data;
        var mappedTicketData = new MappedTicketData();
        mappedTicketData.Id = ticketData.Id;
        mappedTicketData.SiteId = ticketData.SiteId;
        mappedTicketData.Priority = ticketData.Priority;
        mappedTicketData.Status = ticketEventDto.EventType.Equals("Create", StringComparison.OrdinalIgnoreCase) ? TicketStatusEnum.New.ToString() : ticketData.Status;
        mappedTicketData.TwinId = ticketData.TwinId;
        mappedTicketData.Summary = ticketData.Summary;
        mappedTicketData.Description = ticketData.Description;
        mappedTicketData.Reporter = ticketData.ReporterId.HasValue ? new MappedReporter
        {
            ReporterId = ticketData.ReporterId.Value,
            ReporterName = ticketData.ReporterName,
            ReporterPhone = ticketData.ReporterPhone,
            ReporterEmail = ticketData.ReporterEmail,
            ReporterCompany = ticketData.ReporterCompany
        } : new MappedReporter();

        mappedTicketData.AssigneeType = ticketData.AssigneeType.ToString().ToLower();
        mappedTicketData.Assignee = ticketData.AssigneeType == AssigneeType.CustomerUser.ToString().ToLower() && ticketData.AssigneeId.HasValue ?
            new MappedAssignee
            {
                Id = ticketData.AssigneeId.Value,
                Name = ticketData.AssigneeName,
            } : new MappedAssignee();
        mappedTicketData.AssigneeWorkgroup = ticketData.AssigneeType == AssigneeType.WorkGroup.ToString() ?
            new MappedWorkgroup
            {
                Id = ticketData.AssigneeId,
                Name = ticketData.AssigneeName,
            } : new MappedWorkgroup();
        mappedTicketData.DueDate = ticketData.DueDate;
        mappedTicketData.CreatedDate = ticketData.CreatedDate;
        mappedTicketData.UpdatedDate = ticketData.UpdatedDate;
        mappedTicketData.ResolvedDate = ticketData.ResolvedDate;
        mappedTicketData.ClosedDate = ticketData.ClosedDate;
        mappedTicketData.SpaceTwinId = ticketData.SpaceTwinId;
        mappedTicketData.Cause = ticketData.Cause;
        mappedTicketData.Solution = ticketData.Solution;
        mappedTicketData.Creator = new MappedUserProfile
        {
            Id = ticketData.CreatorId,
        };
        return new MappedTicketEventDto
        {

            EventId = Guid.NewGuid(),
            EventType = ticketEventDto.EventType,
            Timestamp = DateTimeOffset.UtcNow,
            Data = mappedTicketData
        };
    }
   
}

