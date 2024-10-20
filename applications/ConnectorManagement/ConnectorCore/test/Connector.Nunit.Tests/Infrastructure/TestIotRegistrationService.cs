namespace Connector.Nunit.Tests.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using ConnectorCore.Services;

    public class TestIotRegistrationService : IIotRegistrationService
    {
        public async Task<string> RegisterDevice(string deviceId, Guid siteId, string connectorId, string connectionString)
        {
            return await Task.FromResult(null as string);
        }

        public async Task<string> GetConnectionString(Guid clientId)
        {
            return await Task.FromResult(null as string);
        }

        public async Task DeleteDevice(string deviceId, string connectionString)
        {
            await Task.FromResult(null as string);
        }
    }
}
