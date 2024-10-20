using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Insights;
using Willow.Platform.Models;
using Willow.Workflow.Models;

namespace PlatformPortalXL.Services
{
    /// <summary>
    /// Twin statistics
    /// </summary>
    public interface ITwinStatisticsService
    {
        /// <summary>
        /// Take a list of Twins and produce Ticket and Insight statistics for each Twin
        /// </summary>
        /// <param name="request">List of twin identifiers</param>
        /// <returns>List of twin statistics</returns>
        public Task<StatisticsResponse> GetTwinStatisticsAsync(StatisticsRequest request);
    }

    /// <summary>
    /// List of Twin identifiers for which to retrieve statistics
    /// </summary>
    public class StatisticsRequest
    {
        /// <summary>
        /// List of Twin identifiers
        /// </summary>
        public List<string> TwinIds { get; set; }
    }

    /// <summary>
    /// Twin statistics
    /// </summary>
    public class TwinStatisticsDto
    {
        /// <summary>
        /// Twin identifier
        /// </summary>
        public string TwinId { get; set; }

        /// <summary>
        /// Insights grouped by priority
        /// </summary>
        public PriorityCounts InsightsStats { get; set; }       

        /// <summary>
        /// The number of tickets
        /// </summary>
        public TicketStatsByStatus TicketStatsByStatus { get; set; }
    }

    /// <summary>
    /// A list of twin statistics
    /// </summary>
    public class StatisticsResponse
    {
        public List<TwinStatisticsDto> Twins { get; set; }
    }

    /// <summary>
    /// Twin Statistics
    /// </summary>
    public class TwinStatisticsService : ITwinStatisticsService
    {
        private readonly ITicketService _ticketService;
        private readonly IInsightService _insightService;

        /// <summary>
        /// Delegate to the ticket and insight services
        /// </summary>
        /// <param name="ticketService">Ticket Service</param>
        /// <param name="insightService">Insight Service</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TwinStatisticsService(
            ITicketService ticketService,
            IInsightService insightService)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _insightService = insightService ?? throw new ArgumentNullException(nameof(insightService));
        }

        /// <summary>
        /// Generate Ticket and Insight statistics for each Twin
        /// Take Ticket statistics and Insight statistics, combine them
        /// </summary>
        /// <param name="request">List of twins</param>
        /// <returns>A list of insight priority counts and ticket statistics by status for each twin</returns>
        public async Task<StatisticsResponse> GetTwinStatisticsAsync(StatisticsRequest request)
        {
            List<TwinTicketStatisticsByStatus> ticketStatistics = await _ticketService.GetTwinTicketStatisticsByStatusAsync(new()
            {
                DtIds = request.TwinIds
            });
            var ticketsDict = ticketStatistics.ToDictionary(x => x.TwinId, x => x);

            List<TwinInsightStatisticsDto> insightStatistics = await _insightService.GetTwinInsightStatisticsAsync(new() { DtIds = request.TwinIds });
            var insightsDict = insightStatistics.ToDictionary(x => x.TwinId, x => x);

            return new()
            {
                Twins = request.TwinIds.Select(dtId =>
                {
                    return new TwinStatisticsDto
                    {
                        TwinId = dtId,
                        InsightsStats = insightsDict.GetValueOrDefault(dtId)?.PriorityCounts ?? new PriorityCounts(),
                        TicketStatsByStatus = ticketsDict.GetValueOrDefault(dtId) ?? new TicketStatsByStatus()
                    };
                }).ToList()
            };
        }
    }
}
