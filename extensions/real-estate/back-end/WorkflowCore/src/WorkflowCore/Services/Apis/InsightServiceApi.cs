using System.Threading.Tasks;
using System;
using Willow.Api.Client;

namespace WorkflowCore.Services.Apis;

public interface IInsightServiceApi
{
	Task UpdateInsightStatusAsync(Guid siteId, BatchUpdateInsightStatusRequest request);
}
public class InsightServiceApi : IInsightServiceApi
{
	private readonly IRestApi _insightApi;

    public InsightServiceApi(IRestApi insightApi)
	{
        _insightApi = insightApi;
	}

    public Task UpdateInsightStatusAsync(Guid siteId, BatchUpdateInsightStatusRequest request)
    {
        return _insightApi.PutCommand($"sites/{siteId}/insights/status",request);
    }
}
