
using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class ConnectorsStats
    {
        public List<ConnectorStats> Data { get; set; }
    }
    
    public class ConnectorStats
    {
        public Guid ConnectorId { get; set; }
        public string CurrentSetState { get; set; }
        public string CurrentStatus { get; set; }
        public int TotalCapabilitiesCount { get; set; }
        public int DisabledCapabilitiesCount { get; set; }
        public int HostingDevicesCount { get; set; }
        public int TotalTelemetryCount { get; set; }
        public List<ConnectorTelemetrySnapshot> Telemetry { get; set; }
        public List<ConnectorStatusSnapshot> Status { get; set; }
    }

    public class ConnectorTelemetrySnapshot
    {
        public DateTime Timestamp { get; set; }
        public int TotalTelemetryCount { get; set; }
        public int UniqueCapabilityCount { get; set; }
        public string SetState { get; set; }
        public string Status { get; set; }
    }

    public class ConnectorStatusSnapshot
    {
        public DateTime TimestampUtc { get; set; }
        public string SetState { get; set; }
        public string Status { get; set; }
    }
}
