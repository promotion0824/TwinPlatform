namespace Willow.IoTService.Monitoring.Options
{
    public class HttpClientFactoryOptions
    {
        public AzureManagementApiOptions? AzureManagementApi { get; set; }
    }

    public class AzureManagementApiOptions
    {
        public string? BaseAddress { get; set; }
    }
}