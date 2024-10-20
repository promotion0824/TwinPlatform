namespace Willow.LiveData.Core.Infrastructure.Attributes;

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Infrastructure.Extensions;

internal class TelemetryInputValidationAttribute : ActionFilterAttribute
{
    /// <inheritdoc/>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        const string payloadKey = "payload";

        //This should have been taken care of in BindRequired attribute
        if (!context.ActionArguments.ContainsKey(payloadKey))
        {
            throw new BadRequestException(string.Empty);
        }

        if (context.ActionArguments[payloadKey] is not TelemetryRequestBody telemetryRequestBody)
        {
            throw new BadRequestException("Invalid payload: Please provide valid payload.");
        }

        if (telemetryRequestBody.ConnectorId == Guid.Empty
            &
            !(telemetryRequestBody.TrendIds != null && telemetryRequestBody.TrendIds.Any())
            &
            !(telemetryRequestBody.DtIds != null && telemetryRequestBody.DtIds.Any()))
        {
            throw new BadRequestException($"Bad request. Please provide any one of these: {nameof(telemetryRequestBody.ConnectorId)}, {nameof(telemetryRequestBody.TrendIds)} or {nameof(telemetryRequestBody.DtIds)}.");
        }

        var validGuids = telemetryRequestBody.TrendIds.GetValidGuids();
        if (validGuids.Count == 0 && telemetryRequestBody.TrendIds != null)
        {
            throw new BadRequestException($"The TrendIDs are not valid GUIDs");
        }

        base.OnActionExecuting(context);
    }
}
