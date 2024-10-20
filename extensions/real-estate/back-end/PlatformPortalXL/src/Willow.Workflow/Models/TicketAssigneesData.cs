using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Platform.Users;

namespace Willow.Workflow.Models;

/// <summary>
/// Represents the data for  ticket possible assignees.
/// </summary>
public class TicketAssigneesData
{
    public TicketAssigneesData()
    {
        Workgroups = Enumerable.Empty<Workgroup>().ToList();
        ExternalUserProfiles = Enumerable.Empty<MappedUserProfile>().ToList();
    }
    /// <summary>
    /// Gets the list of workgroups.
    /// </summary>
    public List<Workgroup> Workgroups { get; set; }

    /// <summary>
    /// Gets the list of external user profiles.
    /// </summary>
    public List<MappedUserProfile> ExternalUserProfiles { get; set; }
}
public class MappedUserProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Company { get; set; }

}
