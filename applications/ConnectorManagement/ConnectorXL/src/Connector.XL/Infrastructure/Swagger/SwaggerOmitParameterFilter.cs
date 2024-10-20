namespace Connector.XL.Infrastructure.Swagger;

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class SwaggerOmitParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor cad))
        {
            return;
        }

        var attributes = cad.MethodInfo.GetCustomAttributes<SwaggerOmitParameterAttribute>();
        foreach (var attribute in attributes)
        {
            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == attribute.ParameterName);
            if (parameter != null)
            {
                operation.Parameters.Remove(parameter);
            }
        }
    }
}
