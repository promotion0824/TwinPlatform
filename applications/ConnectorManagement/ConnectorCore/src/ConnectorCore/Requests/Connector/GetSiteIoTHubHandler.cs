namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Infrastructure.Exceptions;
using ConnectorCore.Models;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class GetSiteIoTHubHandler
{
    public static async Task<Results<Ok<SiteIoTHub>, BadRequest<ProblemDetails>, NotFound<string>>> HandleAsync([FromRoute] Guid customerId, [FromRoute] Guid? siteId, [FromServices] IIotRegistrationService iotRegistration)
    {
        string connectionString;

        try
        {
            connectionString = await iotRegistration.GetConnectionString(customerId);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound($"Customer {customerId} does not exist or its IoTHub has not been configured yet.");
        }

        var hostNameSetting = connectionString.Split(';').FirstOrDefault(x => x.StartsWith("hostname=", StringComparison.InvariantCultureIgnoreCase));
        if (hostNameSetting == null)
        {
            return TypedResults.NotFound($"customer IoTHub is not configured correctly.");
        }

        var hostName = hostNameSetting.Substring("hostname=".Length);
        return TypedResults.Ok(new SiteIoTHub { HostName = hostName });
    }
}
