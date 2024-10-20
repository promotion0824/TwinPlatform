using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
    public class SiteConnectorStatsDto
    {
        public Guid SiteId { get; set; }
        public List<ConnectorStatsDto> ConnectorStats { get; set; }

        public static List<SiteConnectorStatsDto> MapFromModels(IEnumerable<Guid> siteIds, IEnumerable<ConnectorStatsDto> stats)
        {
            var statsGroupedBySite = stats.GroupBy(x => x.SiteId).ToDictionary(x => x.Key, x => x.ToList());

            return siteIds.Select(siteId => new SiteConnectorStatsDto 
            { 
                SiteId = siteId, 
                ConnectorStats = statsGroupedBySite.ContainsKey(siteId) ? statsGroupedBySite[siteId] : new List<ConnectorStatsDto>() 
            }).ToList();
        }
    }

    public class ConnectorStatsDto
    {
        public Guid SiteId { get; set; }
        public Guid ConnectorId { get; set; }
        public string ConnectorName { get; set; }
        public string CurrentSetState { get; set; }
        public string CurrentStatus { get; set; }
        public int TotalCapabilitiesCount { get; set; }
        public int DisabledCapabilitiesCount { get; set; }
        public int HostingDevicesCount { get; set; }
        public int TotalTelemetryCount { get; set; }
        public List<ConnectorTelemetrySnapshot> Telemetry { get; set; }
        public List<ConnectorStatusSnapshot> Status { get; set; }

        public static ConnectorStatsDto MapFromModel(Guid siteId, string name, ConnectorStats model)
        {
            if (model == null)
            {
                return null;
            }

            return new ConnectorStatsDto
            {
                ConnectorId = model.ConnectorId,
                ConnectorName = name,
                CurrentSetState = model.CurrentSetState,
                CurrentStatus = model.CurrentStatus,
                DisabledCapabilitiesCount = model.DisabledCapabilitiesCount,
                TotalCapabilitiesCount = model.TotalCapabilitiesCount,
                HostingDevicesCount = model.HostingDevicesCount,
                SiteId = siteId,
                Status = model.Status,
                Telemetry = model.Telemetry,
                TotalTelemetryCount = model.TotalTelemetryCount
            };
        }

        public static List<ConnectorStatsDto> MapFromModels(IEnumerable<Connector> connectors, IEnumerable<ConnectorStats> stats)
        {
            var statsConnectorIds = stats?.Select(x => x.ConnectorId).ToList();

            return stats?
                .OrderBy(x => x.ConnectorId)
                .Zip(connectors?.Where(x => statsConnectorIds.Contains(x.Id)).OrderBy(x => x.Id), (s, c) => MapFromModel(c.SiteId, c.Name, s))
                .ToList();
        }
    }
}
