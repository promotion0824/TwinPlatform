using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Features.Connectivity
{
    public class ConnectivitySummaryResponse
    {
        public class SiteConnectivity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Suburb { get; set; }
            public string State { get; set; }
            public string Country { get; set; }

            public bool IsOnline { get; set; }
            public int GatewayCount { get; set; }
            public int OnlineGatewayCount { get; set; }
            [Obsolete("It will be removed in future version.")]
            public int OfflineGatewayCount { get; set; }
            public int ConnectorCount { get; set; }
            public int OnlineConnectorCount { get; set; }
            [Obsolete("It will be removed in future version.")]
            public int OfflineConnectorCount { get; set; }
            public int ConnectionErrorCount { get; set; }
            [Obsolete("It will be removed in future version.")]
            public int ErrorCount { get; set; }
            public int? DataIn { get; set; }
            public ServiceStatus Status { get; set; }
            public InsightsSummary Insights { get; set; }
            public TicketsSummary Tickets { get; set; }
        }

        public class InsightsSummary
        {
            public int OpenCount { get; set; }
            public int UrgentCount { get; set; }
            public int HighCount { get; set; }
            public int MediumCount { get; set; }

            internal static InsightsSummary MapFromInsights(PortfolioDashboardSiteInsights model) => 
                model == null
                    ? null
                    : new InsightsSummary
                    {
                        OpenCount = model.OpenCount,
                        UrgentCount = model.UrgentCount,
                        HighCount = model.HighCount,
                        MediumCount = model.MediumCount
                    };
        }

        public class TicketsSummary
        {
            public int OverdueCount { get; set; }
            public int ResolvedCount { get; set; }
            public int UnresolvedCount { get; set; }
            public int UrgentCount { get; set; }
            public int HighCount { get; set; }
            public int MediumCount { get; set; }

            internal static TicketsSummary MapFromTickets(PortfolioDashboardSiteTickets model) =>
                model == null
                    ? null
                    : new TicketsSummary
                    {
                        OverdueCount = model.OverdueCount,
                        ResolvedCount = model.ResolvedCount,
                        UnresolvedCount = model.UnresolvedCount,
                        UrgentCount = model.UrgentCount,
                        HighCount = model.HighCount,
                        MediumCount = model.MediumCount
                    };
        }

        public int ConnectedSiteCount { get; set; }
        public int OnlineGatewayCount { get; set; }
        public int ConnectionErrorCount { get; set; }
        public InsightsSummary Insights { get; set; }
        public TicketsSummary Tickets { get; set; }
        public IList<SiteConnectivity> Sites { get; set; }

        public static ConnectivitySummaryResponse MapFrom(List<PortfolioDashboardSiteStatus> sites) => 
            new ConnectivitySummaryResponse
            {
                ConnectedSiteCount = sites.Count(s => s.IsOnline),
                OnlineGatewayCount = sites.Sum(s => s.OnlineGatewayCount),
                ConnectionErrorCount = sites.Sum(s => s.OfflineConnectorCount),
                Insights = MapInsightTotals(sites),
                Tickets = MapTicketTotals(sites),
                Sites = sites.Select(s => new SiteConnectivity
                {
                    Id = s.SiteId,
                    Name = s.Name,
                    Suburb = s.Suburb,
                    State = s.State,
                    Country = s.Country,
                    IsOnline = s.IsOnline,
                    GatewayCount = s.GatewayCount,
                    OnlineGatewayCount = s.GatewayCount - s.OfflineGatewayCount,
                    OfflineGatewayCount = s.OfflineGatewayCount,
                    ConnectorCount = s.ConnectorCount,
                    OnlineConnectorCount = s.ConnectorCount - s.OfflineConnectorCount,
                    OfflineConnectorCount = s.OfflineConnectorCount,
                    ConnectionErrorCount = s.OfflineConnectorCount,
                    ErrorCount = s.OfflineConnectorCount,
                    DataIn = s.PointCount,
                    Status = s.Status,
                    Insights = InsightsSummary.MapFromInsights(s.Insights),
                    Tickets = TicketsSummary.MapFromTickets(s.Tickets)
                }).ToList()
            };

        private static InsightsSummary MapInsightTotals(List<PortfolioDashboardSiteStatus> sites)
        {
            var insights = sites.Where(s => s.Insights != null).Select(s => s.Insights).ToList();

            return new InsightsSummary
            {
                OpenCount = insights.Sum(i => i.OpenCount),
                UrgentCount = insights.Sum(i => i.UrgentCount),
                HighCount = insights.Sum(i => i.HighCount),
                MediumCount = insights.Sum(i => i.MediumCount)
            };
        }

        private static TicketsSummary MapTicketTotals(List<PortfolioDashboardSiteStatus> sites)
        {
            var tickets = sites.Where(s => s.Tickets != null).Select(s => s.Tickets).ToList();

            return new TicketsSummary
            {
                OverdueCount = tickets.Sum(t => t.OverdueCount),
                ResolvedCount = tickets.Sum(t => t.ResolvedCount),
                UnresolvedCount = tickets.Sum(t => t.UnresolvedCount),
                UrgentCount = tickets.Sum(t => t.UrgentCount),
                HighCount = tickets.Sum(t => t.HighCount),
                MediumCount = tickets.Sum(t => t.MediumCount)
            };
        }
    }
}
