namespace Willow.LiveData.TelemetryStreaming.Services;

using Newtonsoft.Json;
using Willow.LiveData.TelemetryStreaming.Models;

/// <summary>
/// Gets subscriptions from a JSON file.
/// </summary>
internal class ConfigSubscriptionService : ISubscriptionService
{
    private readonly List<Subscription> subscriptions = [];

    public ConfigSubscriptionService()
    {
        string subscriptions = File.ReadAllText("testsubscriptions.json");
        var subs = JsonConvert.DeserializeObject<List<Subscription>>(subscriptions) ?? throw new InvalidOperationException("Unable to read subscriptions configuration");

        this.subscriptions.AddRange(subs);
    }

    public ValueTask<Subscription[]> GetSubscriptions(string connectorId, string externalId) =>
        ValueTask.FromResult(subscriptions.Where(s => s.ConnectorId == connectorId && s.ExternalId == externalId).ToArray());
}
