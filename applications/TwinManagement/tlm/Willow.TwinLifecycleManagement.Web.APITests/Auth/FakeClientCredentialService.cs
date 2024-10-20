using System.Threading;
using Azure.Identity;
using Willow.Api.Authentication;

namespace Willow.TwinLifecycleManagement.Web.APITests
{
	public class FakeClientCredentialService : IClientCredentialTokenService
	{
		public string GetClientCredentialToken(DefaultAzureCredentialOptions tokenCredentialOptions = null, CancellationToken cancellationToken = default)
		{
			return "fake_tok";
		}
	}
}
