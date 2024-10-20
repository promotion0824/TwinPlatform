namespace Willow.IoTService.Monitoring.Contracts;

public interface IAlertResolverMessage
{
    public string ConnectorId { get; set; }
    public string ConnectorType { get; set; }
    public string ConnectorName { get; set; }
    public string CustomerId { get; set; }
}