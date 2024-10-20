using Microsoft.PowerBI.Api;
using Microsoft.Rest;

namespace PlatformPortalXL.Services.PowerBI
{
    public interface IPowerBIClientFactory
    {
        IPowerBIClient Create(string token);
    }

    public class PowerBIClientFactory : IPowerBIClientFactory
    {
        public IPowerBIClient Create(string token)
        {
            return new PowerBIClient(new TokenCredentials(token, "Bearer"));
        }
    }
}
