namespace Willow.LiveData.Core.Infrastructure.Filters;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Infrastructure.Configuration;

internal class PageSizeValidationFilter(IOptions<RequestConfiguration> requestOptions) : IEndpointFilter
{
    private readonly int maxPageSize = requestOptions.Value.MaxPageSize;
    private const string PageSizeKey = "pageSize";

    /// <inheritdoc/>
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.Request.Query.ContainsKey(PageSizeKey))
        {
            var stringValues = context.HttpContext.Request.Query[PageSizeKey];
            if (!int.TryParse(stringValues.FirstOrDefault(), out var pageSize))
            {
                throw new BadRequestException("Incorrect format: PageSize should be integer.");
            }

            if (pageSize < 1 || pageSize > maxPageSize)
            {
                throw new BadRequestException($"Invalid PageSize: PageSize should be within 1 to {maxPageSize}.");
            }
        }
        else
        {
            context.HttpContext.Request.QueryString.Add(PageSizeKey, maxPageSize.ToString());
        }

        var result = await next(context);
        return result;
    }
}
