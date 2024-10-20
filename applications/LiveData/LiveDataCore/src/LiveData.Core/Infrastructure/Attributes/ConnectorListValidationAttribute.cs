namespace Willow.LiveData.Core.Infrastructure.Attributes;

using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Features.Connectors.DTOs;
using Willow.LiveData.Core.Infrastructure.Extensions;

internal class ConnectorListValidationAttribute : ActionFilterAttribute
{
    /// <inheritdoc/>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        const string payloadKey = "connectorList";

        //This should have been taken care of in BindRequired attribute
        if (!context.ActionArguments.ContainsKey(payloadKey))
        {
            throw new BadRequestException(string.Empty);
        }

        if (context.ActionArguments[payloadKey] is not ConnectorList connectorList)
        {
            throw new BadRequestException("Invalid payload: Please provide valid payload.");
        }

        if (connectorList.ConnectorIds == null || !connectorList.ConnectorIds.Any())
        {
            throw new BadRequestException($"Please provide at least one ConnectorId");
        }

        var validGuids = connectorList.ConnectorIds.GetValidGuids();
        if (validGuids.Count == 0)
        {
            throw new BadRequestException($"The ConnectorIDs are not valid GUIDs");
        }

        base.OnActionExecuting(context);
    }
}
