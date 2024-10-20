using System;
using System.Collections.Generic;
using Willow.IoTService.Monitoring.Enums;

namespace Willow.IoTService.Monitoring.Contracts;

public class MonitorEvent
{    
    public Guid ConnectorId { get; init; }
    public string? ConnectorName { get; init; }
    public string? SiteId { get; init; }
    public string? DeviceId { get; set; }
    public string? IoTHub { get; set; }
    public string? CustomerName { get; set; }
    public string? SiteName { get; set; }
    public string? CustomerId { get; set; }
    public string? ConnectorConnectionType { get; set; }
    public string? ConnectorType { get; set; }
    public IDictionary<string, string>? CustomProperties { get; set; }
    public MonitorSource MonitorSource { get; init; }
    public IDictionary<string, double>? Metrics { get; set; }
}