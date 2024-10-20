namespace Willow.IoTService.Deployment.Dashboard.Controllers;

using Microsoft.AspNetCore.Mvc;

public static class ControllerExtensions
{
    public static string GetApiPath(this ControllerBase controller)
    {
        var version = controller.ControllerContext.RouteData.Values["apiVersion"]
                               ?.ToString() ??
                      "1";
        var controllerFragment = controller.GetType()
                                           .Name.Replace("Controller", string.Empty);

        return $"v{version}/{controllerFragment}";
    }
}
