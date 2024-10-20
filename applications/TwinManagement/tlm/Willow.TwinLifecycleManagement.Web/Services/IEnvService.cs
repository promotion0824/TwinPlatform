using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services;

public interface IEnvService
{
	Task<AppVersion> GetAdtApiVersion();
}
