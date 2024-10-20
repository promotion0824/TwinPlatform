using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Authorization.TwinPlatform.Web.Middleware;

/// <summary>
/// UM BFF Response Handler Middleware
/// </summary>
public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateException dbUpEx)
        {
            logger.LogError(dbUpEx, "Duplicate record error.");
            await HandleDbUpdateExceptionAsync(context, dbUpEx);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error occurred processing the request.");
            await HandleGeneralExceptionAsync(context, ex);
        }

    }

    private static Task HandleDbUpdateExceptionAsync(HttpContext context, DbUpdateException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        context.Response.ContentType = "application/json";

        var result = new
        {
            error = "A conflict occurred while processing your request.",
            details = ex.Message
        };

        return context.Response.WriteAsJsonAsync(result);
    }

    private static Task HandleGeneralExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var result = new
        {
            error = "An error occurred while processing your request.",
            details = ex.Message
        };

        return context.Response.WriteAsJsonAsync(result);
    }
}
