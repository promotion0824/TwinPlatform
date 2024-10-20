using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using PlatformPortalXL.Auth.Permissions;

namespace PlatformPortalXL.Auth.Extensions;

public static class AuthorizationOptionsExtensions
{
    /// <summary>
    /// Extension method to add all the application registrations for policies that can be used on controllers.
    /// </summary>
    /// <remarks>
    /// Maps requirements by name to requirement class instances.
    /// </remarks>
    public static void AddAuthorizationRequirements(this AuthorizationOptions options)
    {
        var typeT = typeof(WillowAuthorizationRequirement);
        var types = typeT.Assembly.GetTypes().Where(p => typeT.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

        foreach (var type in types)
        {
            var requirement = (WillowAuthorizationRequirement)Activator.CreateInstance(type)!;

            options.AddPolicy(type.Name, policy => policy.Requirements.Add(requirement));
        }
    }
}
