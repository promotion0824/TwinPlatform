namespace Willow.LiveData.Core.Infrastructure.Attributes;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Infrastructure.Configuration;

internal class PageSizeValidationAttribute(IOptions<RequestConfiguration> requestOptions) : ActionFilterAttribute
{
    private readonly int maxPageSize = requestOptions.Value.MaxPageSize;

    /// <inheritdoc/>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        const string key = "pageSize";
        if (context.ActionArguments.TryGetValue(key, out object value))
        {
            if (value is not int pageSize)
            {
                throw new BadRequestException("Incorrect format: PageSize should be integer.");
            }

            if (pageSize < 1 || pageSize > maxPageSize)
            {
                throw new BadRequestException($"Invalid PageSize: PageSize should be within 1 to {maxPageSize}.");
            }
        }
        else { context.ActionArguments[key] = maxPageSize; }

        base.OnActionExecuting(context);
    }
}
