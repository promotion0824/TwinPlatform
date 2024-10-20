namespace Willow.Api.Authorization;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Willow.Api.Client.Sdk.Directory.Services;

/// <summary>
/// A middleware that adds platform role claims to the current user.
/// </summary>
public class AuthorizationClaimsMiddleware
{
    private readonly RequestDelegate next;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationClaimsMiddleware"/> class.
    /// </summary>
    /// <param name="next">A delegate for the next middleware to execute in the pipeline.</param>
    public AuthorizationClaimsMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Invoke the middleware.
    /// </summary>
    /// <param name="context">The http context of the request.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var userLookupService = context.RequestServices.GetRequiredService<IUserLookupService>();
        var currentUser = await userLookupService.GetCurrentUser();

        if (currentUser?.User?.PlatformRoles.Any() == true)
        {
            context.AddPlatformRoleClaims(currentUser.User.PlatformRoles);
        }

        await next(context);
    }
}
