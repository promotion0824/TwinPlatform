using System;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Options;

namespace Willow.IoTService.Monitoring.Models
{
    public class ConnectorConfigInfo
    {
        public Guid ConnectorId { get; set; }

        public string? ConnectorName { get; set; }

        public ConnectorConnectionType ConnectionType { get; set; }

        public Guid SiteId { get; set; }

        public string? SiteName { get; set; }

        public Guid CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public bool IsEnabled { get; set; }
        
        public DateTime LastUpdatedAt { get; set; }

        public bool IsADXEnabled { get; set; }

        public string? AzureResourceId { get; set; }

        public int TimeInterval { get; set; }

        public AppInsightsOptions? AzureAppInsights { get; set; }
        
        public string? ConnectorType { get; set; }
    }
}