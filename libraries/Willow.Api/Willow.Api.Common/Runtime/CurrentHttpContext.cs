namespace Willow.Api.Common.Runtime;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Willow.Api.Common.Extensions;
using Willow.Api.Common.Middlewares;

/// <summary>
/// Provides access to the current http context.
/// </summary>
public class CurrentHttpContext : ICurrentHttpContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentHttpContext"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">An instance of an IHttpContextAccessor.</param>
    public CurrentHttpContext(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext is null)
        {
            return;
        }

        var requestHeaders = httpContextAccessor.HttpContext.Request.Headers;

        if (requestHeaders.TryGetValue(RequestIdMiddleware.HeaderKey, out var headerKey))
        {
            RequestId = Guid.Parse(headerKey.ToString());
        }

        IsAuthenticatedRequest = httpContextAccessor.HttpContext.User.Identity is { IsAuthenticated: true };

        if (!IsAuthenticatedRequest)
        {
            return;
        }

        UserEmail = httpContextAccessor.HttpContext.GetClaimValue(ClaimTypes.Email) ??
                    httpContextAccessor.HttpContext.GetClaimValue(WillowClaimTypes.Emails);
        BearerToken = httpContextAccessor.HttpContext.GetBearerToken();
    }

    /// <summary>
    /// Gets the request id for the current http context.
    /// </summary>
    public Guid RequestId { get; }

    /// <summary>
    /// Gets a value indicating whether the current http context is authenticated.
    /// </summary>
    public bool IsAuthenticatedRequest { get; }

    /// <summary>
    /// Gets the user email for the current http context.
    /// </summary>
    public string? UserEmail { get; }

    /// <summary>
    /// Gets the bearer token for the current http context.
    /// </summary>
    public string? BearerToken { get; }
}
