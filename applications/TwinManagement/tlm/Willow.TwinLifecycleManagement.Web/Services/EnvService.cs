using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services;

public class EnvService(IEnvClient envClient) : IEnvService
{
	public async Task<AppVersion> GetAdtApiVersion()
	{
		return await envClient.VersionAsync();
	}
}
