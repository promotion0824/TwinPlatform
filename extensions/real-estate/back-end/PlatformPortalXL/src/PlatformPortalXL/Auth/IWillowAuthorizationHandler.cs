using System;
using Microsoft.AspNetCore.Authorization;

namespace PlatformPortalXL.Auth;

public interface IWillowAuthorizationHandler : IAuthorizationHandler
{
    Type RequirementType { get; }
}

/// <summary>
/// Marker interface for defining a class as a global (unscoped) permission evaluator.
/// </summary>
public interface IGlobalPermissionEvaluator;

public static class WillowAuthorizationHandlerExtensions
{
    public static bool IsGlobalPermissionEvaluator(this IWillowAuthorizationHandler handler)
    {
        return handler is IGlobalPermissionEvaluator;
    }
}
