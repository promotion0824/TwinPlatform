namespace Willow.LiveData.Core.Infrastructure.Filters;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Willow.Infrastructure.Exceptions;

internal class RequiredTwinIdValidationFilter : IEndpointFilter
{
    private const string TwinIdKey = "twinId";

    /// <inheritdoc/>
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Query.ContainsKey(TwinIdKey))
        {
            throw new BadRequestException("Missing twinId");
        }

        if (context.HttpContext.Request.Query.TryGetValue(TwinIdKey, out var value) && value.All(string.IsNullOrWhiteSpace))
        {
            throw new BadRequestException("Missing twinId");
        }

        var result = await next(context);
        return result;
    }
}
