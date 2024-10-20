using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Batch;
using Willow.Workflow;

namespace PlatformPortalXL.Features.Insights
{
    public enum InsightActivityType
    {
        Tickets,
        PreviouslyResolved,
        PreviouslyIgnored,
        Reported
    }

	public static class InsightActivityExtensions
	{
		public static List<InsightActivityType> GetActivityFilter(this List<InsightSimpleDto> insights)
		{
            var activityFilter = new List<InsightActivityType>();

            if (insights.Any(x => x.TicketCount > 0)) activityFilter.Add(InsightActivityType.Tickets);
            if (insights.Any(x => x.PreviouslyResolved > 0)) activityFilter.Add(InsightActivityType.PreviouslyResolved);
            if (insights.Any(x => x.PreviouslyIgnored > 0)) activityFilter.Add(InsightActivityType.PreviouslyIgnored);
            if (insights.Any(x => x.Reported)) activityFilter.Add(InsightActivityType.Reported);

            return activityFilter;
        }

        public static List<string> MapToStrings(this List<InsightActivityType> activityFilter)
        {
            return activityFilter.Select(x => x.ToString()).ToList();
        }

        public static async Task<FilterSpecificationDto[]> UpsertActivityFilter(this FilterSpecificationDto[] filters, Task<List<InsightTicketStatistics>> stats)
        {
            var activityFilter = filters.FirstOrDefault(InsightFilterType.Activity.ToString());
            if (activityFilter != null)
            {
                filters = filters.RemoveFilters(InsightFilterType.Activity.ToString());

                var serializedActivityFilterValue = JsonSerializerHelper.Serialize(activityFilter.Value);

                if (serializedActivityFilterValue.Contains(InsightActivityType.Tickets.ToString())
                    || serializedActivityFilterValue.Contains(((int)InsightActivityType.Tickets).ToString()))
                {
                    filters = filters.Upsert(nameof(Insight.Id), (await stats).Select(x => x.Id));
                }

                if (serializedActivityFilterValue.Contains(InsightActivityType.PreviouslyResolved.ToString())
                    || serializedActivityFilterValue.Contains(((int)InsightActivityType.PreviouslyResolved).ToString()))
                {
                    filters = filters.Upsert("StatusLogs[Status]", InsightStatus.Resolved);
                }

                if (serializedActivityFilterValue.Contains(InsightActivityType.PreviouslyIgnored.ToString())
                    || serializedActivityFilterValue.Contains(((int)InsightActivityType.PreviouslyIgnored).ToString()))
                {
                    filters = filters.Upsert("StatusLogs[Status]", InsightStatus.Ignored);
                }

                if (serializedActivityFilterValue.Contains(InsightActivityType.Reported.ToString())
                    || serializedActivityFilterValue.Contains(((int)InsightActivityType.Reported).ToString()))
                {
                    filters = filters.Upsert(nameof(Insight.Reported), true);
                }
            }

            return filters;
        }

        public static async Task<FilterSpecificationDto[]> UpsertActivityFilter(this Task<FilterSpecificationDto[]> filterTask, Task<List<InsightTicketStatistics>> stats)
        {
            var filters = await filterTask;
            return await filters.UpsertActivityFilter(stats);
        }
    }
}
