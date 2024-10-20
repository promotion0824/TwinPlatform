namespace Willow.Api.Authorization.Policies;

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Willow.Api.Authorization.Requirements;

/// <summary>
/// A policy that requires the user to be a platform admin.
/// </summary>
public class PlatformAdminUserPolicy : AuthorizationPolicy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAdminUserPolicy"/> class.
    /// </summary>
    public PlatformAdminUserPolicy()
        : base(PolicyRequirements, Schemes)
    {
    }

    private static IEnumerable<string> Schemes => new[] { Authentication.AuthenticationSchemes.AzureAdB2C };

    private static IEnumerable<IAuthorizationRequirement> PolicyRequirements => new[]
    {
        new PlatformRoleRequirement(PlatformRoles.Admin),
    };
}
