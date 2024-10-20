namespace Willow.Api.Authorization.Policies;

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Willow.Api.Authorization.Requirements;
using static Willow.Api.Authentication.AuthenticationSchemes;

/// <summary>
/// A policy that requires the user to be a platform admin user or a platform application.
/// </summary>
public class PlatformAdminUserOrPlatformApplicationPolicy : AuthorizationPolicy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAdminUserOrPlatformApplicationPolicy"/> class.
    /// </summary>
    public PlatformAdminUserOrPlatformApplicationPolicy()
        : base(PolicyRequirements, Schemes)
    {
    }

    private static IEnumerable<string> Schemes => new[] { AzureAdB2C, AzureAd };

    private static IEnumerable<IAuthorizationRequirement> PolicyRequirements => new[]
    {
        new PlatformAdminUserOrPlatformApplicationRequirement(),
    };
}
