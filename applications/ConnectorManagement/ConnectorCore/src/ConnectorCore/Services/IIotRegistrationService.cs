namespace ConnectorCore.Services
{
    using System;
    using System.Threading.Tasks;

    internal interface IIotRegistrationService
    {
        Task<string> RegisterDevice(string deviceId, Guid siteId, string connectorId, string connectionString);

        Task<string> GetConnectionString(Guid clientId);

        Task DeleteDevice(string deviceId, string connectionString);
    }
}
