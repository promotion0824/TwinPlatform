using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services;

public interface IGraphService
{
	Task<TwinGraph> GetTwinGraph(string[] twinIds);
}
