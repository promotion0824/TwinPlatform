using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using System.Net;
using Willow.AzureDigitalTwins.Services.Configuration;

namespace Willow.AzureDigitalTwins.Services.Infra;

public class EventRouteFilterType
{
    private EventRouteFilterType(string value) { Value = value; }

    public string Value { get; private set; }

    public static EventRouteFilterType TwinCreate { get { return new EventRouteFilterType("Microsoft.DigitalTwins.Twin.Create"); } }
    public static EventRouteFilterType TwinUpdate { get { return new EventRouteFilterType("Microsoft.DigitalTwins.Twin.Update"); } }
    public static EventRouteFilterType TwinDelete { get { return new EventRouteFilterType("Microsoft.DigitalTwins.Twin.Delete"); } }
    public static EventRouteFilterType RelationshipCreate { get { return new EventRouteFilterType("Microsoft.DigitalTwins.Relationship.Create"); } }
    public static EventRouteFilterType RelationshipUpdate { get { return new EventRouteFilterType("Microsoft.DigitalTwins.Relationship.Update"); } }
    public static EventRouteFilterType RelationshipDelete { get { return new EventRouteFilterType("Microsoft.DigitalTwins.Relationship.Delete"); } }
    public static EventRouteFilterType Telemetry { get { return new EventRouteFilterType("microsoft.iot.telemetry"); } }
}

public interface IAzureDigitalTwinEventRoute
{
    Task CreateOrReplaceEventRouteAsync(string routeId, string endpointName, IEnumerable<EventRouteFilterType> filters);
    Task<DigitalTwinsEventRoute> GetEventRouteAsync(string routeId);
    string GetEventRouteFilterString(IEnumerable<EventRouteFilterType> filters);
}

public class AzureDigitalTwinEventRoute : IAzureDigitalTwinEventRoute
{
    private readonly DigitalTwinsClient _digitalTwinsClient;

    public AzureDigitalTwinEventRoute(AzureDigitalTwinsSettings settings, TokenCredential tokenCredential)
    {
        _digitalTwinsClient = new DigitalTwinsClient(settings.Instance.InstanceUri, tokenCredential);
    }

    public async Task CreateOrReplaceEventRouteAsync(string routeId, string endpointName, IEnumerable<EventRouteFilterType> filters)
    {
        var eventRoute = new DigitalTwinsEventRoute(endpointName, GetEventRouteFilterString(filters));
        await _digitalTwinsClient.CreateOrReplaceEventRouteAsync(routeId, eventRoute);
    }

    public async Task<DigitalTwinsEventRoute> GetEventRouteAsync(string routeId)
    {
        try
        {
            return await _digitalTwinsClient.GetEventRouteAsync(routeId);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public string GetEventRouteFilterString(IEnumerable<EventRouteFilterType> filters)
    {
        return string.Join(" OR ", filters.Select(x => $"type = '{x.Value}'").ToArray());
    }
}
