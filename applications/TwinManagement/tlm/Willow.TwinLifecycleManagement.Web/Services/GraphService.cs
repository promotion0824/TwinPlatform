using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services;

/// <summary>
/// A service to get twin graph information.
/// </summary>
public class GraphService(IGraphClient graphClient) : IGraphService
{
    /// <summary>
    /// Get twin graph via twin input collection.
    /// </summary>
    /// <param name="twinIds">Twin Ids.</param>
    /// <returns>TwinGraph.</returns>
    public async Task<TwinGraph> GetTwinGraph(string[] twinIds)
    {
        return await graphClient.GetTwinGraphAsync(twinIds);
    }
}
