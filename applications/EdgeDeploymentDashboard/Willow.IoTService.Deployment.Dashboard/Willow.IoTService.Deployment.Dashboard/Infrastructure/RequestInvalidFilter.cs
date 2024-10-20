namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class RequestInvalidFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // no action needed here
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.ExceptionHandled || context.Exception is not RequestBaseException ex)
        {
            return;
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        context.ExceptionHandled = true;
        var problem = factory.CreateProblemDetails(context.HttpContext);
        problem.Status = ex.StatusCode;
        problem.Title = ex.Message;
        context.Result = new ObjectResult(problem) { StatusCode = ex.StatusCode };
    }
}
