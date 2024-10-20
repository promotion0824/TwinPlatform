using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Workflow;

namespace PlatformPortalXL.Models
{
    public class PortfolioDashboardSiteStatus
    {
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public ServiceStatus Status { get; set; }
        public int? PointCount { get; set; }
        public PortfolioDashboardSiteInsights Insights { get; set; }
        public PortfolioDashboardSiteTickets Tickets { get; set; }

        public List<PortfolioDashboardGatewayStatus> Gateways { get; set; }

        public bool IsOnline => (Status == ServiceStatus.Online || Status == ServiceStatus.OnlineWithErrors);
        public int OnlineGatewayCount => Gateways?.Count(g => g.IsOnline) ?? 0;
        public int PointErrorCount => Gateways?.Sum(g => g.Connectors?.Sum(c => c.ErrorCount) ?? 0) ?? 0;
        public int GatewayCount => Gateways?.Count() ?? 0;
        public int OfflineGatewayCount => Gateways?.Count(g => g.IsOffline) ?? 0;
        public int ConnectorCount => Gateways?.Select(g => g.ConnectorCount).Sum() ?? 0;
        public int OfflineConnectorCount => Gateways?.Select(g => g.OfflineConnectorCount).Sum() ?? 0;
    }

    public enum ServiceStatus
    {
        NotOperational = 0,
        Offline = 1,
        Online = 2,
        OnlineWithErrors = 3,
        Archived = 4
    }

    public class PortfolioDashboardSiteInsights
    {
        public int UrgentCount { get; set; }
        public int OpenCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }

        public static PortfolioDashboardSiteInsights MapFrom(SiteInsightStatistics stats)
        {
            if (stats == null)
            {
                return null;
            }

            return new PortfolioDashboardSiteInsights
            {
                OpenCount = stats.OpenCount,
                UrgentCount = stats.UrgentCount,
                HighCount = stats.HighCount,
                MediumCount = stats.MediumCount
            };
        }

        public static List<PortfolioDashboardSiteInsights> MapFrom(List<SiteInsightStatistics> stats)
        {
            return stats?.Select(MapFrom).ToList();
        }
    }

    public class PortfolioDashboardSiteTickets
    {
        public int OverdueCount { get; set; }
        public int UnresolvedCount { get; set; }
        public int ResolvedCount { get; set; }
        public int UrgentCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }

        public static PortfolioDashboardSiteTickets MapFrom(SiteTicketStatistics stats)
        {
            if (stats == null)
            {
                return null;
            }

            return new PortfolioDashboardSiteTickets
            {
                OverdueCount = stats.OverdueCount,
                UnresolvedCount = 0,
                ResolvedCount = 0,
                UrgentCount = stats.UrgentCount,
                HighCount = stats.HighCount,
                MediumCount = stats.MediumCount
            };
        }

        public static List<PortfolioDashboardSiteTickets> MapFrom(List<SiteTicketStatistics> stats)
        {
            return stats?.Select(MapFrom).ToList();
        }
    }

    public class PortfolioDashboardGatewayStatus
    {
        public Guid GatewayId { get; set; }
        public string Name { get; set; }
        public ServiceStatus Status { get; set; }
        public DateTime? LastUpdated { get; set; }

        public List<PortfolioDashboardConnectorStatus> Connectors { get; set; }

        public bool IsOnline => (Status == ServiceStatus.Online || Status == ServiceStatus.OnlineWithErrors);
        public bool IsOffline => Status == ServiceStatus.Offline;

        public int ConnectorCount => Connectors?.Count(c => c.Status != ServiceStatus.NotOperational) ?? 0;
        public int OfflineConnectorCount => Connectors?.Count(c => c.Status == ServiceStatus.Offline) ?? 0;

        internal static List<PortfolioDashboardGatewayStatus> MapFromConnectors(List<PortfolioDashboardConnectorStatus> models)
        {
            if (models == null || !models.Any())
            {
                return new List<PortfolioDashboardGatewayStatus>();
            }

            return new List<PortfolioDashboardGatewayStatus> { 
                new PortfolioDashboardGatewayStatus
                {
                    GatewayId = models.FirstOrDefault()?.ConnectorId ?? Guid.Empty,
                    Name = "BMS",
                    Status = GetGatewayStatus(models),
                    Connectors = models,
                    LastUpdated = models.Max(m => m.LastUpdated)
                } 
            };
        }

        private static ServiceStatus GetGatewayStatus(List<PortfolioDashboardConnectorStatus> models) => models != null && models.Any() ? ServiceStatus.Online : ServiceStatus.NotOperational;
    }

    public class PortfolioDashboardConnectorStatus
    {
        public Guid ConnectorId { get; set; }
        public string Name { get; set; }
        public int? PointCount { get; set; }
        public int? ErrorCount { get; set; }
        public ServiceStatus Status { get; set; }
        public DateTime? LastUpdated { get; set; }

        public static PortfolioDashboardConnectorStatus MapFrom(Connector connector, ConnectorLogRecord connectorLogRecord)
        {
            return new PortfolioDashboardConnectorStatus
            {
                ConnectorId = connector.Id,
                Name = connector.Name,
                ErrorCount = connectorLogRecord?.ErrorCount,
                PointCount = connector.PointsCount > 0 ? connector.PointsCount : connectorLogRecord?.PointCount,
                LastUpdated = connectorLogRecord?.CreatedAt,
                Status = MapStatus(connector, connectorLogRecord)
            };
        }

        public static ServiceStatus MapStatus(Connector connector, ConnectorLogRecord connectorLogRecord, bool includeArchivedStatus = false)
        {
            if(includeArchivedStatus && connector.IsArchived)
            {
                return ServiceStatus.Archived;
            }

            if (!connector.IsEnabled)
            {
                return ServiceStatus.NotOperational;
            }

            if (connectorLogRecord == null || connectorLogRecord.CreatedAt < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(25)))
            {
                return ServiceStatus.Offline;
            }

            return ServiceStatus.Online;
        }
    }

    public class PortfolioDashboardConnectorLog
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int PointCount { get; set; }
        public int ErrorCount { get; set; }

        private const int PeriodDurationInMins = 15;

        internal static List<PortfolioDashboardConnectorLog> MapFrom(DateTime startTime, List<ConnectorLogRecord> logRecords)
        {
            if (logRecords == null)
            {
                return new List<PortfolioDashboardConnectorLog>();
            }

            var output = new Dictionary<DateTime, PortfolioDashboardConnectorLog>();

            DateTime endTime = MapQuarterHour(DateTime.UtcNow.AddMinutes(PeriodDurationInMins));
            var currentPeriod = startTime;

            while (currentPeriod < endTime)
            {
                output[currentPeriod] = new PortfolioDashboardConnectorLog { Start = currentPeriod, End = currentPeriod.AddMinutes(PeriodDurationInMins) };
                currentPeriod = currentPeriod.AddMinutes(PeriodDurationInMins);
            }

            foreach (var logRecord in logRecords)
            {
                if (output.TryGetValue(MapQuarterHour(logRecord.StartTime), out var currentItem))
                {
                    currentItem.ErrorCount += logRecord.ErrorCount;
                    currentItem.PointCount += logRecord.PointCount;
                }
            }

            return output.Values.ToList();
        }

        public static DateTime MapQuarterHour(DateTime dateTime)
        {
            var minutes = dateTime.Minute < 15 ? 0 : ((dateTime.Minute < 30) ? 15 : (dateTime.Minute < 45) ? 30 : 45);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, minutes, 0, DateTimeKind.Utc);
        }
    }
}
