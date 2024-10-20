namespace Willow.IoTService.Deployment.Dashboard.Controllers;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("api")]
public class ErrorsController : ControllerBase
{
    [Route("error-development")]
    public IActionResult HandleErrorDevelopment([FromServices] IHostEnvironment hostEnvironment)
    {
        if (!hostEnvironment.IsDevelopment())
        {
            return this.NotFound();
        }

        var exceptionHandlerFeature = this.HttpContext.Features.Get<IExceptionHandlerFeature>()!;
        return this.Problem(exceptionHandlerFeature.Error.StackTrace, title: exceptionHandlerFeature.Error.Message);
    }

    [Route("error")]
    public IActionResult HandleError()
    {
        return this.Problem();
    }
}
