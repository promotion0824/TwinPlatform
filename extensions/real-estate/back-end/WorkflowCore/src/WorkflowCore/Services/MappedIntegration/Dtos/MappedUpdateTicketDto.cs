using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Common;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.MappedIntegration.Dtos;

public class MappedUpdateTicketDto : MappedTicketBaseDto
{
    public Guid? TicketId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? JobTypeId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ServiceNeededId { get; set; }
    public Guid? SubStatusId { get; set; }
    public void MapToTicketEntity(TicketEntity ticketEntity)
    {
        ticketEntity.ExternalId = ExternalId;
        ticketEntity.Summary = Summary;
        ticketEntity.Description = Description;
        ticketEntity.Priority = (int)Enum.Parse<Priority>(Priority, true);
        ticketEntity.Status = (int)Enum.Parse<TicketStatusEnum>(Status, true);
        ticketEntity.Solution = Solution ?? string.Empty;
        ticketEntity.Cause = Cause ?? string.Empty;       
        ticketEntity.AssigneeType = Enum.Parse<AssigneeType>(AssigneeType, true);
        // only update assignee data if the assignee name changed
        ticketEntity.AssigneeId = ticketEntity.AssigneeName != GetAssigneeData().Name ? GetAssigneeData().Id : ticketEntity.AssigneeId;
        ticketEntity.AssigneeName = ticketEntity.AssigneeName != GetAssigneeData().Name? GetAssigneeData().Name : ticketEntity.AssigneeName;

        ticketEntity.ReporterId = Reporter?.ReporterId;
        ticketEntity.ReporterName = Reporter?.ReporterName ?? string.Empty;
        ticketEntity.ReporterPhone = Reporter?.ReporterPhone ?? string.Empty;
        ticketEntity.ReporterEmail = Reporter?.ReporterEmail ?? string.Empty;
        ticketEntity.ReporterCompany = Reporter?.ReporterCompany ?? string.Empty;

        // only update the dates if the dates are different
        if (IsDateTimeDifferent(ticketEntity.ExternalUpdatedDate, ExternalUpdatedDate))
        {
            ticketEntity.ExternalUpdatedDate = ExternalUpdatedDate;
        }
        if (IsDateTimeDifferent(ticketEntity.DueDate, DueDate))
        {
            ticketEntity.DueDate = DueDate;
        }
        if (IsDateTimeDifferent(ticketEntity.ResolvedDate, ResolvedDate))
        {
            ticketEntity.ResolvedDate = ResolvedDate;
        }
        if (IsDateTimeDifferent(ticketEntity.ClosedDate, ClosedDate))
        {
            ticketEntity.ClosedDate = ClosedDate;
        }
        // JobTypeId, CategoryId, ServiceNeededId, SubStatusId values are mapped from existing values
        // if value doesn't exist , ignore the update instead of setting value to null
        if (JobTypeId is not null && ticketEntity.JobTypeId != JobTypeId)
        {
            ticketEntity.JobTypeId = JobTypeId;
        }
        if (CategoryId is not null && ticketEntity.CategoryId != CategoryId)
        {
            ticketEntity.CategoryId = CategoryId;
        }
        if (ServiceNeededId is not null && ticketEntity.ServiceNeededId != ServiceNeededId)
        {
            ticketEntity.ServiceNeededId = ServiceNeededId;
        }
        if(SubStatusId is not null && ticketEntity.SubStatusId != SubStatusId)
        {
            ticketEntity.SubStatusId = SubStatusId;
        }
    }

    /// <summary>
    /// Checks if two DateTime values are different within a certain tolerance.
    /// Same date retrieved from the database can have different ticks compared to the date from the API request.
    /// </summary>
    /// <param name="date1">The first DateTime value.</param>
    /// <param name="date2">The second DateTime value.</param>
    /// <returns>True if the DateTime values are equal within the tolerance, otherwise false.</returns>
    private bool IsDateTimeDifferent(DateTime? date1, DateTime? date2)
    {
        if (date1.HasValue && date2.HasValue)
        {
            var dateTicksDifference = Math.Abs((date1.Value.Ticks - date2.Value.Ticks));
            // ignore the update if the difference is less than 10 milliseconds
            var toleranceTicks = TimeSpan.TicksPerMillisecond * 10;
           
            if (dateTicksDifference <= toleranceTicks)
            {
                return false;
            }
        }

        return true;
    }
}

