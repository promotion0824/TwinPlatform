namespace Willow.LiveData.Core.Infrastructure.Filters;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Willow.Infrastructure.Exceptions;

internal class DateRangeValidationFilter : IEndpointFilter
{
    private const int TotalDaysAllowed = 30;

    /// <inheritdoc/>
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        const string fromKey = "start";
        const string toKey = "end";

        //This should have been taken care of in BindRequired attribute
        if (context.HttpContext.Request.Query.ContainsKey(fromKey) &&
            context.HttpContext.Request.Query.ContainsKey(toKey))
        {
            if (!DateTimeOffset.TryParse(context.HttpContext.Request.Query[fromKey], out var fromDate))
            {
                throw new InvalidCastException();
            }

            if (!DateTimeOffset.TryParse(context.HttpContext.Request.Query[toKey], out var toDate))
            {
                throw new InvalidCastException();
            }

            if (fromDate > toDate)
            {
                throw new BadRequestException("Negative time range: The time range entered is invalid");
            }

            if ((toDate - fromDate).TotalDays > TotalDaysAllowed)
            {
                throw new BadRequestException($"more than {TotalDaysAllowed} days: The time range entered more than {TotalDaysAllowed} days.Please provide range within {TotalDaysAllowed} days.");
            }
        }

        var result = await next(context);
        return result;
    }
}
