using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PlatformPortalXL.Infrastructure;

/// <summary>
/// Middleware that includes the email of the current user in all log messages written during the request.
/// </summary>
public class UserEmailLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserEmailLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<UserEmailLoggingMiddleware>>();
        var userEmail = context.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        using var scope = logger.BeginScope("{UserEmail}", userEmail);

        await _next(context);
    }
}
