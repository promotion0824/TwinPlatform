using FluentValidation;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.MappedIntegration.Dtos.Requests;

public class MappedTicketUpsertRequest
{
    /// <summary>
    /// unique EventId of the Event 
    /// </summary>
    [Required]
    public Guid? EventId { get; set; }

    /// <summary>
    /// Type of the Event create| update
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string EventType { get; set; }

    /// <summary>
    /// Timestamp of the Event
    /// </summary>
    [Required]
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Ticket data of the Event
    /// </summary>
    [Required]
    public TicketData Data { get; set; }
}
public class TicketData
{
    /// <summary>
    /// Willow Ticket Id
    /// </summary>
    public Guid?  TicketId { get; set; }
    /// <summary>
    /// CustomerId of the Ticket
    /// </summary>
    public Guid? CustomerId { get; set; }
    /// <summary>
    /// SiteId of the Ticket
    /// </summary>
    public Guid? SiteId { get; set; }
    /// <summary>
    /// SequenceNumberPrefix = site code
    /// </summary>
    public string SequenceNumberPrefix { get; set; }
    /// <summary>
    /// ticket summary
    /// </summary>
    public string Summary { get; set; }
    /// <summary>
    /// ticket description
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// ticket priority
    /// </summary>
    public string Priority { get; set; }
    /// <summary>
    /// ticket status
    /// </summary>
    public string Status { get; set; }
    /// <summary>
    /// ticket sub status
    /// </summary>
    public string SubStatus { get; set; }
    /// <summary>
    /// Id of the  Twin
    /// </summary>
    public string TwinId { get; set; }
    /// <summary>
    /// Id of the Space Twin
    /// </summary>
    public string SpaceTwinId { get; set; }
    /// <summary>
    /// ticket reporter
    /// </summary>
    public MappedReporter Reporter { get; set; }
    /// <summary>
    /// ticket assignee type
    /// </summary>
    public string AssigneeType { get; set; }
    /// <summary>
    /// ticket assignee
    /// </summary>
    public MappedAssignee Assignee { get; set; }
    /// <summary>
    /// ticket assignee workgroup
    /// </summary>
    public MappedWorkgroup AssigneeWorkgroup { get; set; }
    /// <summary>
    /// ticket due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    /// <summary>
    /// ticket created date in the external system
    /// </summary>
    public DateTime? ExternalCreatedDate { get; set; }
    /// <summary>
    /// ticket updated date in the external system
    /// </summary>
    public DateTime? ExternalUpdatedDate { get; set; }
    /// <summary>
    /// ticket resolved date
    /// </summary>
    public DateTime? ResolvedDate { get; set; }
    /// <summary>
    /// ticket closed date
    /// </summary>
    public DateTime? ClosedDate { get; set; }
    /// <summary>
    /// request type = category
    /// </summary>
    public string RequestType { get; set; }
    /// <summary>
    /// ticket job type
    /// </summary>
    public string JobType { get; set; }
    /// <summary>
    /// ticket cause
    /// </summary>
    public string Cause { get; set; }
    /// <summary>
    /// ticket solution
    /// </summary>
    public string Solution { get; set; }
    /// <summary>
    /// Id of the Ticket in the Mapped
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(128)]
    public string ExternalId { get; set; }
    /// <summary>
    /// ticket source id = app id
    /// </summary>
    public Guid? SourceId { get; set; }
    /// <summary>
    /// ticket source name = app name
    /// </summary>
    public string SourceName { get; set; }
    /// <summary>
    /// ticket creator id
    /// </summary>
    public MappedUserProfile Creator { get; set; }

    public string ServiceNeeded { get; set; }

    /// <summary>
    /// Represents the user who closed the ticket 
    /// </summary>
    public MappedUserProfile ClosedBy { get; set; }

    public static MappedCreateTicketDto MapToCreateTicketDto(TicketData ticketData)
    {
        var mappedTicketDto = new MappedCreateTicketDto();
        mappedTicketDto.CustomerId = ticketData.CustomerId.Value;
        mappedTicketDto.SiteId = ticketData.SiteId.Value;
        mappedTicketDto.Priority = ticketData.Priority;
        mappedTicketDto.Status = ticketData.Status.Replace(" ", ""); ;
        mappedTicketDto.Summary = ticketData.Summary ?? string.Empty; ;
        mappedTicketDto.Description = ticketData.Description ?? string.Empty;
        mappedTicketDto.Cause = ticketData.Cause;
        mappedTicketDto.Solution = ticketData.Solution;
        mappedTicketDto.Reporter = ticketData.Reporter;

        mappedTicketDto.AssigneeType = ticketData.AssigneeType;
        mappedTicketDto.Assignee = ticketData.Assignee;
        mappedTicketDto.AssigneeWorkgroup = ticketData.AssigneeWorkgroup;

        mappedTicketDto.DueDate = ticketData.DueDate;
        mappedTicketDto.ResolvedDate = ticketData.ResolvedDate;
        mappedTicketDto.ClosedDate = ticketData.ClosedDate;
        mappedTicketDto.SourceId = ticketData.SourceId;
        mappedTicketDto.SourceName = ticketData.SourceName;
        mappedTicketDto.ExternalId = ticketData.ExternalId;
        mappedTicketDto.ExternalCreatedDate = ticketData.ExternalCreatedDate;
        mappedTicketDto.ExternalUpdatedDate = ticketData.ExternalUpdatedDate;
        mappedTicketDto.Creator = ticketData.Creator;
        mappedTicketDto.TwinId = ticketData.TwinId;
        mappedTicketDto.SpaceTwinId = ticketData.SpaceTwinId;
        mappedTicketDto.JobType = ticketData.JobType?.Trim();
        mappedTicketDto.SubStatus = ticketData.SubStatus;
        mappedTicketDto.RequestType = ticketData.RequestType?.Trim();
        mappedTicketDto.ServiceNeeded = ticketData.ServiceNeeded?.Trim();
        mappedTicketDto.ClosedBy = ticketData.ClosedBy;

        return mappedTicketDto;
    }

    public static MappedUpdateTicketDto MapToUpdateTicketDto(TicketData ticketData)
    {
        var mappedTicketDto = new MappedUpdateTicketDto();
        
        mappedTicketDto.TicketId = ticketData.TicketId;
        mappedTicketDto.ExternalId = ticketData.ExternalId;
        mappedTicketDto.Priority = ticketData.Priority;
        mappedTicketDto.Status = ticketData.Status.Replace(" ", "");
        mappedTicketDto.Summary = ticketData.Summary ?? string.Empty;
        mappedTicketDto.Description = ticketData.Description ?? string.Empty; ;
        mappedTicketDto.Cause = ticketData.Cause;
        mappedTicketDto.Solution = ticketData.Solution;
        mappedTicketDto.Reporter = ticketData.Reporter;
        mappedTicketDto.AssigneeType = ticketData.AssigneeType;
        mappedTicketDto.Assignee = ticketData.Assignee;
        mappedTicketDto.AssigneeWorkgroup = ticketData.AssigneeWorkgroup;
        mappedTicketDto.DueDate = ticketData.DueDate;
        mappedTicketDto.ResolvedDate = ticketData.ResolvedDate;
        mappedTicketDto.ClosedDate = ticketData.ClosedDate;
        mappedTicketDto.ExternalUpdatedDate = ticketData.ExternalUpdatedDate;
        mappedTicketDto.JobType = ticketData.JobType?.Trim();
        mappedTicketDto.SubStatus = ticketData.SubStatus?.Trim();
        mappedTicketDto.RequestType = ticketData.RequestType?.Trim();
        mappedTicketDto.ServiceNeeded = ticketData.ServiceNeeded?.Trim();
        mappedTicketDto.ClosedBy = ticketData.ClosedBy;

        return mappedTicketDto;
    }
}


public class MappedTicketUpsertValidator : AbstractValidator<MappedTicketUpsertRequest>
{
    public MappedTicketUpsertValidator(IConfiguration configuration)
    {
        var appSettings = configuration.Get<AppSettings>();

        // when ticket synced two ways, the read only value is false
        var isFullSynced = !appSettings.MappedIntegrationConfiguration.IsReadOnly;

        // validation for all tickets full synced and readonly
        When(x => string.Equals(x.EventType, TicketEventType.Create.ToString(),
        StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Data.SequenceNumberPrefix).NotEmpty();
            RuleFor(x => x.Data.Creator).NotEmpty();
            When(x => x.Data.Creator is not null, () =>
            {
                RuleFor(x => x.Data.Creator.Name).NotEmpty().MaximumLength(250);
                RuleFor(x => x.Data.Creator.Email).NotEmpty().EmailAddress().MaximumLength(100);
                RuleFor(x => x.Data.Creator.Phone).MaximumLength(32);
                RuleFor(x => x.Data.Creator.Company).MaximumLength(250);
            });
        });
        RuleFor(x => x.Data.CustomerId).NotEmpty();
        RuleFor(x => x.Data.SiteId).NotNull();
        RuleFor(x => x.Data.SourceId).NotEmpty();
        RuleFor(x => x.Data.SourceName).NotEmpty();
        RuleFor(x => x.Data.SpaceTwinId).NotEmpty().MaximumLength(250);

        RuleFor(x => x.Data.Status).NotEmpty()
                                 .Must(BeAValidEnumValue<TicketStatusEnum>)
                                 .WithMessage($"{nameof(MappedTicketUpsertRequest.Data.Status)} {{PropertyValue}} must be a valid Status");

        When(x => !string.IsNullOrWhiteSpace(x.Data.ClosedBy?.Email), () =>
        {
            RuleFor(x => x.Data.ClosedBy.Email).EmailAddress().MaximumLength(100);
            RuleFor(x => x.Data.ClosedBy.Name).NotEmpty().MaximumLength(250);
        });

     
        // validate full synced tickets
        if (isFullSynced)
        {
            // ticket id should be required for update for Fully Synced integration
            When(x => string.Equals(x.EventType, TicketEventType.Update.ToString(),
                StringComparison.OrdinalIgnoreCase), () =>
                {
                    RuleFor(x => x.Data.TicketId).NotEmpty();
           
                });
            RuleFor(x => x.Data.Summary).NotEmpty();
            RuleFor(x => x.Data.Description).NotEmpty();
            RuleFor(x => x.Data.RequestType).NotEmpty();
            RuleFor(x => x.Data.JobType).NotEmpty();
            RuleFor(x => x.Data.ServiceNeeded).NotEmpty();

            RuleFor(x => x.Data.Reporter).NotNull();
            When(x => x.Data.Reporter is not null, () =>
            {
                RuleFor(x => x.Data.Reporter.ReporterName).NotEmpty().MaximumLength(500);
                RuleFor(x => x.Data.Reporter.ReporterEmail).NotEmpty().EmailAddress().MaximumLength(64);
                RuleFor(x => x.Data.Reporter.ReporterPhone).MaximumLength(32);
                RuleFor(x => x.Data.Reporter.ReporterCompany).MaximumLength(64);

            });

            RuleFor(x => x.Data.Priority).NotEmpty()
                                     .Must(BeAValidEnumValue<Priority>)
                                     .WithMessage($"{nameof(MappedTicketUpsertRequest.Data.Priority)} {{PropertyValue}} must be a valid Priority");

            When(x => !string.IsNullOrEmpty(x.Data.Status) &&
               Enum.Parse<TicketStatusEnum>(x.Data.Status, true) == TicketStatusEnum.OnHold, () =>
               {
                   RuleFor(x => x.Data.SubStatus).NotEmpty();
               });

            RuleFor(x => x.Data.AssigneeType).NotEmpty()
                                        .Must(BeAValidEnumValue<AssigneeType>)
                                        .WithMessage($"{{PropertyValue}} is invalid AssigneeType , the available values are: {string.Join(", ", GetAssigneeTypesList())}");

            When(x => BeAValidEnumValue<AssigneeType>(x.Data.AssigneeType) &&
                           Enum.Parse<AssigneeType>(x.Data.AssigneeType, true) == AssigneeType.CustomerUser, () =>
                           {
                               RuleFor(x => x.Data.Assignee).NotNull();
                               When(x => x.Data.Assignee is not null, () =>
                               {
                                   RuleFor(x => x.Data.Assignee.Name).NotEmpty().MaximumLength(250);
                                   RuleFor(x => x.Data.Assignee.Email).NotEmpty().EmailAddress().MaximumLength(100);
                               });

                           });


            When(x => BeAValidEnumValue<AssigneeType>(x.Data.AssigneeType) && Enum.Parse<AssigneeType>(x.Data.AssigneeType, true) == AssigneeType.WorkGroup, () =>
            {
                RuleFor(x => x.Data.AssigneeWorkgroup).NotNull()
                                                      .WithMessage($"Workgroup name is required when the Assignee type is {AssigneeType.WorkGroup}");

                When(x => x.Data.AssigneeWorkgroup is not null, () =>
                {
                    RuleFor(x => x.Data.AssigneeWorkgroup.Name).NotEmpty().MaximumLength(250);
                });

            });



            When(x => string.Equals(x.Data.Status, TicketStatusEnum.ClosedCompleted.ToString(), StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.Data.ClosedDate).NotEmpty();
                RuleFor(x => x.Data.Cause).NotEmpty();
                RuleFor(x => x.Data.Solution).NotEmpty();

            });
        }

        // validate ticket with read only sync
        else
        {

            RuleFor(x => x.Data.Priority).NotEmpty();

            When(x => !string.IsNullOrWhiteSpace(x.Data.AssigneeType), () =>
            {
                RuleFor(x => x.Data.AssigneeType)
                               .Must(BeAValidEnumValue<AssigneeType>)
                               .WithMessage($"{{PropertyValue}} is invalid AssigneeType , the available values are: {string.Join(", ", GetAssigneeTypesList())}");

                When(x => BeAValidEnumValue<AssigneeType>(x.Data.AssigneeType), () =>
                {

                    When(x => Enum.Parse<AssigneeType>(x.Data.AssigneeType, true) == AssigneeType.CustomerUser, () =>
                    {
                        RuleFor(x => x.Data.Assignee)
                                            .NotNull()
                                            .WithMessage($"Assignee details (name and email) are required when the Assignee type is {AssigneeType.CustomerUser}");
                        When(x => x.Data.Assignee is not null, () =>
                        {
                            RuleFor(x => x.Data.Assignee.Name).NotEmpty().MaximumLength(250);
                            RuleFor(x => x.Data.Assignee.Email).NotEmpty().EmailAddress().MaximumLength(100);
                        });

                    });

                    When(x => Enum.Parse<AssigneeType>(x.Data.AssigneeType, true) == AssigneeType.WorkGroup, () =>
                    {
                        RuleFor(x => x.Data.AssigneeWorkgroup)
                                            .NotNull()
                                            .WithMessage($"Workgroup name is required when the Assignee type is {AssigneeType.WorkGroup}");

                        When(x => x.Data.AssigneeWorkgroup is not null, () =>
                        {
                            RuleFor(x => x.Data.AssigneeWorkgroup.Name).NotEmpty().MaximumLength(250);
                        });

                    });
                });

            });
        }
    }

    private bool BeAValidEnumValue<T>(string value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Replace(" ", "");

        return Enum.TryParse<T>(value, true, out _);
    }

    private string[] GetAssigneeTypesList()
    {
        return Enum.GetNames<AssigneeType>().ToArray();
    }
}


