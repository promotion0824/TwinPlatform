using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mime;

namespace Willow.Common.Middlewares;

public static class ExceptionHandlerMiddleware
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(AppError =>
        {
            AppError.Run(async ctx =>
            {
                var contextFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var problemFactory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var logger = ctx.RequestServices.GetRequiredService<ILogger<IApplicationBuilder>>();
                    var problemDetails = problemFactory.CreateProblemDetails(ctx, StatusCodes.Status500InternalServerError, title: "Internal Server Error");
                    logger.LogError(contextFeature.Error, contextFeature.Error.Message);
                    ctx.Response.ContentType = MediaTypeNames.Application.Json;
                    await ctx.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails));
                }
            });
        });
    }
}