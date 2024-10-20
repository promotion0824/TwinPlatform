using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services
{
	public class DQRuleService : IDQRuleService
	{
		private readonly IDQRuleClient _client;
		public DQRuleService(IDQRuleClient client)
		{
			ArgumentNullException.ThrowIfNull(client);
			_client = client;
		}

		public async Task DeleteAllRules()
		{
			await _client.DeleteAllRulesAsync();

		}
	}
}
