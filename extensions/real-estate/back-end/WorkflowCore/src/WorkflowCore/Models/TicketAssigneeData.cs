using System.Collections.Generic;
using WorkflowCore.Dto;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Models;


/// <summary>
/// Represents the data for  ticket possible assignees.
/// </summary>
public class TicketAssigneeData
{
    /// <summary>
    /// Gets the list of workgroups.
    /// </summary>
    public List<WorkgroupDto> Workgroups { get; set; }

    /// <summary>
    /// Gets the list of external user profiles.
    /// </summary>
    public List<MappedUserProfile> ExternalUserProfiles { get; set; }
}

