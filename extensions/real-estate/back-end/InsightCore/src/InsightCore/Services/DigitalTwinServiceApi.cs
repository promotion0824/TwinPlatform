using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using InsightCore.Dto;
using InsightCore.Infrastructure.Helpers;
using Willow.Api.Client;

namespace InsightCore.Services;
public interface IDigitalTwinServiceApi
{
    Task<List<TwinSimpleDto>> GetTwinsByIdsAsync(List<SiteTwinIdsRequestDto> request);
    Task<List<PointTwinDto>> GetPointsByTwinIdsAsync(Guid siteId, List<string> twinIds);
}
public class DigitalTwinServiceApi: IDigitalTwinServiceApi
{
	private readonly IRestApi _digitalTwinCoreApi;
	public DigitalTwinServiceApi(IRestApi digitalTwinCoreApi)
	{
		_digitalTwinCoreApi = digitalTwinCoreApi;
	}

	public async Task<List<TwinSimpleDto>> GetTwinsByIdsAsync(List<SiteTwinIdsRequestDto> request)
	{
        return await _digitalTwinCoreApi.Post<List<SiteTwinIdsRequestDto>, List<TwinSimpleDto>>("sites/Assets/names", request);
    }

    public async Task<List<PointTwinDto>> GetPointsByTwinIdsAsync(Guid siteId, List<string> twinIds)
    {
        var url = $"admin/sites/{siteId}/Twins/twinIds/points";
        return await _digitalTwinCoreApi.Post<List<string>,List<PointTwinDto>>(url, twinIds);
    }
}
