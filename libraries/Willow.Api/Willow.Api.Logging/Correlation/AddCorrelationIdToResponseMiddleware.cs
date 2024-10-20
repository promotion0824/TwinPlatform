namespace Willow.Api.Logging.Correlation;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// The add correlation id to response middleware.
/// </summary>
public class AddCorrelationIdToResponseMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<AddCorrelationIdToResponseMiddleware> logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>
    /// Initializes a new instance of the <see cref="AddCorrelationIdToResponseMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate in the stack.</param>
    /// <param name="logger">An instance of an ILogger object.</param>
    public AddCorrelationIdToResponseMiddleware(RequestDelegate next, ILogger<AddCorrelationIdToResponseMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The input HttpContext.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task Invoke(HttpContext context)
    {
        //Get Correlation Id from the Current Activity
        //AppInsights SDK sets the Activity.Current.RootId with OperationId of the scope
        string correlationId = Activity.Current?.RootId ?? Guid.NewGuid().ToString();

        context.Response.OnStarting(() =>
        {
            // Correlation Id will be added only if it is not present
            if (context.Response.Headers.TryAdd(CorrelationIdHeader, new[] { correlationId }))
            {
                logger.LogTrace("Request correlation Id set to {CorrelationId}", correlationId);
            }

            return Task.CompletedTask;
        });

        await next(context);
    }
}
