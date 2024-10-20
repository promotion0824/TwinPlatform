using Microsoft.EntityFrameworkCore;
using System;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Dto;

/// <summary>
/// Ticket event Data sent to Ticket event topic
/// </summary>
public class TicketEventDto
{
    public Guid EventId { get; set; }
    public string EventType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public TicketData Data { get; set; }
    public class TicketData
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string TwinId { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public string AssigneeType { get; set; }
        public string AssigneeName { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string CategoryName { get; set; }
        public string JobType { get; set; }
        public string SpaceTwinId { get; set; }
        public string SubStatus { get; set; }
        public string Cause { get; set; }
        public string Solution { get; set; }
        public Guid CreatorId { get; set; }
        public string ServiceNeeded { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ServiceNeededId { get; set; }
        public Guid? JobTypeId { get; set; }
        public Guid? SubStatusId { get; set; }
    }
    public static TicketEventDto MapFromTicketEntity(TicketEntity ticketEntity)
    {
        var eventType = ticketEntity.EntityLifeCycleState switch
        {
            EntityState.Added => TicketEventType.Create,
            EntityState.Modified => TicketEventType.Update,
            EntityState.Deleted => TicketEventType.Delete,
            _ => throw new NotImplementedException()
        };
        var ticketData = new TicketData();

        ticketData.Id = ticketEntity.Id;
        ticketData.SiteId = ticketEntity.SiteId;
        ticketData.Priority = Enum.GetName(typeof(Priority), ticketEntity.Priority).ToLower();
        ticketData.Status = Enum.GetName(typeof(TicketStatusEnum), ticketEntity.Status).ToLower();
        ticketData.SubStatus = ticketEntity.SubStatus?.Name;
        ticketData.SubStatusId = ticketEntity.SubStatusId;
        ticketData.TwinId = ticketEntity.TwinId;
        ticketData.Summary = ticketEntity.Summary;
        ticketData.Description = ticketEntity.Description;
        ticketData.ReporterId = ticketEntity.ReporterId;
        ticketData.ReporterName = ticketEntity.ReporterName;
        ticketData.ReporterPhone = ticketEntity.ReporterPhone;
        ticketData.ReporterEmail = ticketEntity.ReporterEmail;
        ticketData.ReporterCompany = ticketEntity.ReporterCompany;
        ticketData.AssigneeType = ticketEntity.AssigneeType.ToString().ToLower();
        ticketData.AssigneeId = ticketEntity.AssigneeId;
        ticketData.AssigneeName = ticketEntity.AssigneeName;
        ticketData.DueDate = ticketEntity.DueDate;
        ticketData.CreatedDate = ticketEntity.CreatedDate;
        ticketData.UpdatedDate = ticketEntity.UpdatedDate;
        ticketData.ResolvedDate = ticketEntity.ResolvedDate;
        ticketData.ClosedDate = ticketEntity.ClosedDate;
        ticketData.CategoryName = ticketEntity.Category?.Name ?? "Unspecified";
        ticketData.JobType = ticketEntity.JobType?.Name;
        ticketData.SpaceTwinId = ticketEntity.SpaceTwinId;
        ticketData.Cause = ticketEntity.Cause;
        ticketData.Solution = ticketEntity.Solution;
        ticketData.CreatorId = ticketEntity.CreatorId;
        ticketData.ServiceNeeded = ticketEntity.ServiceNeeded?.Name;
        ticketData.CategoryId = ticketEntity.CategoryId;
        ticketData.ServiceNeededId = ticketEntity.ServiceNeededId;
        ticketData.JobTypeId = ticketEntity.JobTypeId;
        return new TicketEventDto
        {

            EventId = Guid.NewGuid(),
            EventType = eventType.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            Data = ticketData
        };
    }
}

