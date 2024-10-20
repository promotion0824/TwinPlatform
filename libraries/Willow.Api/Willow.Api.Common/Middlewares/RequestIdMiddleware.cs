namespace Willow.Api.Common.Middlewares;

using Microsoft.AspNetCore.Http;

/// <summary>
/// A middleware that adds a unique request id to the request header if it does not exist.
/// </summary>
public class RequestIdMiddleware
{
    private readonly RequestDelegate next;

    /// <summary>
    /// The header key for the request id.
    /// </summary>
    public const string HeaderKey = "RequestId";

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestIdMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate to call in the middleware pipeline.</param>
    public RequestIdMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The http context for the request.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(HeaderKey))
        {
            context.Request.Headers.Append(HeaderKey, Guid.NewGuid().ToString());
        }

        await next(context);
    }
}
