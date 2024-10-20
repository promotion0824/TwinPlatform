using InsightCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Api.Client;

namespace InsightCore.Services;
public interface IWorkflowServiceApi
{
	Task<bool> GetInsightNumberOfOpenTicketsAsync(Guid insightId);
    Task<List<SiteInsightTicketStatisticsDto>> GetSiteInsightStatistics(List<Guid> siteIds, List<int> statuses = null,
        bool? scheduled = null);
}
public class WorkflowServiceApi : IWorkflowServiceApi
{
	private readonly IRestApi _workflowServiceApi;
	public WorkflowServiceApi(IRestApi workflowServiceApi)
	{
		_workflowServiceApi = workflowServiceApi;
	}

	public Task<bool> GetInsightNumberOfOpenTicketsAsync(Guid insightId)
	{
		return _workflowServiceApi.Get<bool>($"insights/{insightId}/tickets/open");
	}

    public Task<List<SiteInsightTicketStatisticsDto>> GetSiteInsightStatistics(List<Guid> siteIds, List<int> statuses = null, bool? scheduled = null)
    {
        return _workflowServiceApi.Post<dynamic, List<SiteInsightTicketStatisticsDto>>("siteinsightStatistics", new
        {
            SiteIds = siteIds,
            Statuses = statuses,
            Scheduled = scheduled
        });
    }
}
