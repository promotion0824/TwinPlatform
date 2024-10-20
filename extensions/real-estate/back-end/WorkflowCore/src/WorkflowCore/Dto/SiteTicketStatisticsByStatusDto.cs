using System;
using System.Linq;
using System.Collections.Generic;

namespace WorkflowCore.Dto
{
    public class SiteTicketStatisticsByStatusDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Represents the number of open tickets
        /// </summary>
        public int OpenCount { get; set; }
        /// <summary>
        /// represents the number of resolved tickets
        /// </summary>
        public int ResolvedCount { get; set; }
        /// <summary>
        /// represents the number of closed tickets
        /// </summary>
        public int ClosedCount { get; set; }

        public static SiteTicketStatisticsByStatusDto MapFromModel(SiteTicketStatisticsByStatus siteTicketStatisticsByStatus)
        {
            if (siteTicketStatisticsByStatus == null)
            {
                return null;
            }

            return new SiteTicketStatisticsByStatusDto
            {
                Id = siteTicketStatisticsByStatus.Id,
                OpenCount = siteTicketStatisticsByStatus.OpenCount,
                ResolvedCount = siteTicketStatisticsByStatus.ResolvedCount,
                ClosedCount = siteTicketStatisticsByStatus.ClosedCount
            };
        }

        public static List<SiteTicketStatisticsByStatusDto> MapFromModels(List<SiteTicketStatisticsByStatus> siteTicketStatisticsByStatusList)
        {
            return siteTicketStatisticsByStatusList.Select(MapFromModel).ToList();
        }
    }

}
