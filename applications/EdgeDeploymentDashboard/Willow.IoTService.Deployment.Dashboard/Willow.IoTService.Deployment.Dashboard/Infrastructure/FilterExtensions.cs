namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

using Microsoft.AspNetCore.Mvc.Filters;

public static class FilterExtensions
{
    public static void AddErrorHandlingFilters(this FilterCollection filters)
    {
        filters.Add(typeof(ModelStateValidationFilter));
        filters.Add(typeof(RequestInvalidFilter));
    }
}
