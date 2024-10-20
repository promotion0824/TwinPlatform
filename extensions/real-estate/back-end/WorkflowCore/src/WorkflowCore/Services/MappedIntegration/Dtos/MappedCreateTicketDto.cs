using System;
using Willow.Common;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.MappedIntegration.Dtos;
public class MappedCreateTicketDto : MappedTicketBaseDto
{
    public Guid CustomerId { get; set; }
    public Guid? SourceId { get; set; }
    public string SequenceNumber { get; set; }

    public string SourceName { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? IssueId { get; set; }
    public string IssueName { get; set; }
    public Guid? JobTypeId { get; set; }
    public Guid? ServiceNeededId { get; set; }
    public Guid? SubStatusId { get; set; }
    public TicketEntity MapTo(MappedCreateTicketDto mappedTicketDto, IDateTimeService dateTimeService)
    {
        var utcNow = dateTimeService.UtcNow;
        var assigneeType = Enum.Parse<AssigneeType>(mappedTicketDto.AssigneeType, true);
        var ticketEntity = new TicketEntity();
        ticketEntity.Id = Guid.NewGuid();
        ticketEntity.CustomerId = mappedTicketDto.CustomerId;
        ticketEntity.SiteId = mappedTicketDto.SiteId;
        ticketEntity.SequenceNumber = mappedTicketDto.SequenceNumber;
        ticketEntity.Priority = (int)Enum.Parse<Priority>(mappedTicketDto.Priority, true);
        ticketEntity.Status = (int)Enum.Parse<TicketStatusEnum>(mappedTicketDto.Status, true);
        // this field will be removed eventually and the twin id will be used instead to get the twin type
        ticketEntity.IssueType = IssueType.Asset;
        ticketEntity.IssueId = mappedTicketDto.IssueId;
        ticketEntity.IssueName = mappedTicketDto.IssueName ?? string.Empty;
        ticketEntity.Summary = mappedTicketDto.Summary;
        ticketEntity.Description = mappedTicketDto.Description;
        ticketEntity.Cause = mappedTicketDto.Cause ?? string.Empty;
        ticketEntity.Solution = mappedTicketDto.Solution ?? string.Empty;
        ticketEntity.ReporterId = mappedTicketDto.Reporter?.ReporterId;
        ticketEntity.ReporterName = mappedTicketDto.Reporter?.ReporterName ?? string.Empty;
        ticketEntity.ReporterPhone = mappedTicketDto.Reporter?.ReporterPhone ?? string.Empty;
        ticketEntity.ReporterEmail = mappedTicketDto.Reporter?.ReporterEmail ?? string.Empty;
        ticketEntity.ReporterCompany = mappedTicketDto.Reporter?.ReporterCompany ?? string.Empty;
        ticketEntity.AssigneeType = assigneeType;
        ticketEntity.AssigneeId = GetAssigneeData().Id;
        ticketEntity.AssigneeName = GetAssigneeData().Name;
        ticketEntity.DueDate = mappedTicketDto.DueDate;
        ticketEntity.CreatedDate = utcNow;
        ticketEntity.UpdatedDate = utcNow;
        ticketEntity.ResolvedDate = mappedTicketDto.ResolvedDate;
        ticketEntity.ClosedDate = mappedTicketDto.ClosedDate;
        ticketEntity.SourceType = SourceType.Mapped;
        ticketEntity.SourceId = mappedTicketDto.SourceId;
        ticketEntity.ExternalId = mappedTicketDto.ExternalId;
        ticketEntity.ExternalCreatedDate = mappedTicketDto.ExternalCreatedDate;
        ticketEntity.ExternalUpdatedDate = mappedTicketDto.ExternalUpdatedDate;
        ticketEntity.CreatorId = mappedTicketDto.Creator.Id;
        ticketEntity.CategoryId = mappedTicketDto.CategoryId;
        ticketEntity.SourceName = mappedTicketDto.SourceName;
        ticketEntity.TwinId = mappedTicketDto.TwinId;
        ticketEntity.SpaceTwinId = mappedTicketDto.SpaceTwinId;
        ticketEntity.JobTypeId = mappedTicketDto.JobTypeId;
        ticketEntity.ServiceNeededId = mappedTicketDto.ServiceNeededId;
        ticketEntity.SubStatusId = mappedTicketDto.SubStatusId;


        ticketEntity.ExternalMetadata = "";
        ticketEntity.InsightName = "";
        ticketEntity.ExternalStatus = "";
        ticketEntity.FloorCode = "";
        ticketEntity.Notes = "";

        return ticketEntity;
    }
}

