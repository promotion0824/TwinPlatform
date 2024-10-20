using PlatformPortalXL.Dto;
using System.Collections.Generic;
using System;
using Willow.Platform.Users;
using PlatformPortalXL.Models;
using System.Linq;

namespace PlatformPortalXL.ServicesApi.DirectoryApi.Responses;

public class GetUserDetailsResponse
{
    public GetUserDetailsResponse()
    {
        UserAssignments = Enumerable.Empty<UserAssignment>().ToList();
    }
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Initials { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public UserStatus Status { get; set; }
    public string Auth0UserId { get; set; }

    public string Name => ((FirstName ?? "") + " " + (LastName ?? "")).Trim();

    public string Company { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }

    public List<UserAssignment> UserAssignments { get; set; }
}

public record UserAssignment(string permissionId, Guid resourceId);

