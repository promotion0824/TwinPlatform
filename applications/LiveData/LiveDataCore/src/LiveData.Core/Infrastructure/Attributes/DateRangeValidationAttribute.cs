namespace Willow.LiveData.Core.Infrastructure.Attributes;

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Willow.Infrastructure.Exceptions;

internal class DateRangeValidationAttribute : ActionFilterAttribute
{
    private readonly int totalDaysAllowed = 30;

    /// <inheritdoc/>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        const string fromKey = "start";
        const string toKey = "end";

        //This should have been taken care of in BindRequired attribute
        if (context.ActionArguments.ContainsKey(fromKey) &&
            context.ActionArguments.ContainsKey(toKey))
        {
            if (context.ActionArguments[fromKey] is not DateTime fromDate)
            {
                throw new InvalidCastException();
            }

            if (context.ActionArguments[toKey] is not DateTime toDate)
            {
                throw new InvalidCastException();
            }

            if (fromDate > toDate)
            {
                throw new BadRequestException("Negative time range: The time range entered is invalid");
            }

            if ((toDate - fromDate).TotalDays > totalDaysAllowed)
            {
                throw new BadRequestException($"more than {totalDaysAllowed} days: The time range entered more than {totalDaysAllowed} days.Please provide range within {totalDaysAllowed} days.");
            }
        }

        base.OnActionExecuting(context);
    }
}
