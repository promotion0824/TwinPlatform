using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Willow.Api.Client;
using WorkflowCore.Dto;
using WorkflowCore.Infrastructure.Helpers;
using WorkflowCore.Services.Apis.Requests;

namespace WorkflowCore.Services.Apis;

public interface IDigitalTwinServiceApi
{
	Task<List<TwinIdDto>> GetTwinIdsByUniqueIdsAsync(Guid siteId, IEnumerable<Guid> uniqueIds);
    Task<TwinDto> GetTwinById(Guid siteId, string twinId);
    Task<List<BuildingsTwinDto>> GetBuildingTwinsByExternalIds(GetBuildingTwinsByExternalIdsRequest request);
}
public class DigitalTwinServiceApi : IDigitalTwinServiceApi
{
	private readonly IRestApi _digitalTwinCoreApi;
	public DigitalTwinServiceApi(IRestApi digitalTwinCoreApi)
	{
		_digitalTwinCoreApi = digitalTwinCoreApi;
	}

    public async Task<List<TwinIdDto>> GetTwinIdsByUniqueIdsAsync(Guid siteId, IEnumerable<Guid> uniqueIds)
	{
		var queryString = HttpHelper.ToQueryString(new { uniqueIds });

        try
        {
            return await _digitalTwinCoreApi.Get<List<TwinIdDto>>($"admin/sites/{siteId}/twins/byUniqueId/batch?{queryString}");
        }
        catch
        {
            return [];
        }
	}

    /// <summary>
    /// Get twin by id
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="twinId"></param>
    /// <returns></returns>
    public async Task<TwinDto> GetTwinById(Guid siteId, string twinId)
    {
        if (string.IsNullOrEmpty(twinId))
            return null;
        var url = $"admin/sites/{siteId}/twins/{twinId}";
        return await _digitalTwinCoreApi.Get<TwinDto>(url);
    }

    public async Task<List<BuildingsTwinDto>> GetBuildingTwinsByExternalIds(GetBuildingTwinsByExternalIdsRequest request)
    {
        var url = "twins/buildings";
        return await _digitalTwinCoreApi.Post<GetBuildingTwinsByExternalIdsRequest, List<BuildingsTwinDto>>(url, request);
    }
}
