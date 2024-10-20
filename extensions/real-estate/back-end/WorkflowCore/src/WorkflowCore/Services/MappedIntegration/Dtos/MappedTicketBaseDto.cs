using System;
using System.Linq;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.MappedIntegration.Dtos;

/// <summary>
/// Base class for MappedTicketDto
/// this class includes all the properties that are common between MappedCreateTicketDto and MappedUpdateTicketDto
/// </summary>
public class MappedTicketBaseDto
{
    /// <summary>
    /// Id of the Ticket in the Willow
    /// </summary>
    public Guid? Id { get; set; }

   
    /// <summary>
    /// The site Id of the ticket
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// summary of the ticket
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// description of the ticket
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// priority of the ticket
    /// </summary>
    public string Priority { get; set; }

    /// <summary>
    /// status of the ticket
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// sub status of the ticket
    /// </summary>
    public string SubStatus { get; set; }

    /// <summary>
    /// TwinId of the ticket
    /// </summary>
    public string TwinId { get; set; }

    /// <summary>
    /// SpaceTwinId of the Space
    /// </summary>
    public string SpaceTwinId { get; set; }

    /// <summary>
    /// reporter of the ticket
    /// </summary>
    public MappedReporter Reporter { get; set; }


    /// <summary>
    /// Assignee type of the ticket
    /// </summary>
    public string AssigneeType { get; set; }

    /// <summary>
    /// Assignee of the ticket
    /// </summary>
    public MappedAssignee Assignee { get; set; }

    /// <summary>
    /// Assignee workgroup of the ticket
    /// </summary>
    public MappedWorkgroup AssigneeWorkgroup { get; set; }

    /// <summary>
    /// Due date of the ticket
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Created date of the ticket in the external system
    /// </summary>
    public DateTime? ExternalCreatedDate { get; set; }

    /// <summary>
    /// Updated date of the ticket in the external system
    /// </summary>
    public DateTime? ExternalUpdatedDate { get; set; }

    /// <summary>
    /// Resolved date of the ticket
    /// </summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// Closed date of the ticket
    /// </summary>
    public DateTime? ClosedDate { get; set; }

    /// <summary>
    /// Request Type is the same as Ticket Category
    /// </summary>
    public string RequestType { get; set; }

    /// <summary>
    /// Job Type of the ticket
    /// </summary>
    public string JobType { get; set; }

    /// <summary>
    /// Cause of the issue
    /// </summary>
    public string Cause { get; set; }

    /// <summary>
    /// Solution of the issue
    /// </summary>
    public string Solution { get; set; }

    /// <summary>
    /// Id of the Ticket in the Mapped
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Creator  of the Ticket 
    /// </summary>
    public MappedUserProfile Creator { get; set; }
    /// <summary>
    /// Service needed for the ticket
    /// </summary>
    public string ServiceNeeded { get; set; }

    /// <summary>
    /// Represents the user who closed the ticket
    /// </summary>
    /// <remarks>
    /// This property is optional and  included when retrieving a single ticket 
    /// but is not present when retrieving a list of tickets.
    /// </remarks>
    public MappedUserProfile ClosedBy { get; set; }

    protected (Guid? Id, string Name) GetAssigneeData()
    {
        var assigneeType = Enum.Parse<AssigneeType>(AssigneeType, true);
        if (assigneeType == WorkflowCore.Models.AssigneeType.CustomerUser)
        {
            return (Id: Assignee.Id, Name: Assignee.Name);
        }
        else if (assigneeType == WorkflowCore.Models.AssigneeType.WorkGroup)
        {
            return (Id: AssigneeWorkgroup.Id, Name: AssigneeWorkgroup.Name);
        }
        else
        {
            return (Id: null, Name: string.Empty);
        }

    }

    public bool IsUserAssignee() => WorkflowCore.Models.AssigneeType.CustomerUser.ToString().ToLower() == AssigneeType.ToLower();

    public bool IsWorkgroupAssignee() => WorkflowCore.Models.AssigneeType.WorkGroup.ToString().ToLower() == AssigneeType.ToLower();
}

