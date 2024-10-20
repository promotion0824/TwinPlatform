using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Exceptions;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace DigitalTwinCore.Controllers
{
    [ApiController]
    public class LiveDataIngestController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceProvider;

        // TODO: When we move to netcore5, add this to the serialized responses to avoid writing empty GUIDs to reduce return payload size:
        // private readonly JsonSerializerOptions options = new JsonSerializerOptions {
        //     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        // };

        private static readonly string LivedataLastValuePath = $"/{Constants.Properties.LivedataLastValue}";
        private static readonly string LivedataLastValueTimePath = $"/{Constants.Properties.LivedataLastValueTime}";
        private readonly ILogger<DigitalTwinService> _logger;

        public LiveDataIngestController(IAssetService assetService, IDigitalTwinServiceProvider adtProvider, ILogger<DigitalTwinService> logger = null)
        {
            _assetService = assetService;
            _digitalTwinServiceProvider = adtProvider;
            _logger = logger;
        }

        [HttpGet("LiveDataIngest/sites/{siteId}/points")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(Summary = "Gets points by IDs, ExternalIds or TrendIds as multiple params ", Tags = new[] { "LiveDataIngest" })]
        public async Task<ActionResult<List<LiveDataIngestPointDto>>> GetSitePointsAsync(
            [FromRoute] Guid siteId,
            [FromQuery] List<Guid> ids,
            [FromQuery] List<string> externalIds,
            [FromQuery] List<Guid> trendIds,
            [FromQuery] bool includePointsWithNoAssets = false)
        {
            return await GetSitePointInfo(siteId, ids, externalIds, trendIds, includePointsWithNoAssets);
        }

        [HttpGet("LiveDataIngest/sites/{siteId}/points/paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(Summary = "Gets points by IDs, ExternalIds or TrendIds as multiple params ", Tags = new[] { "LiveDataIngest" })]
        public async Task<ActionResult<List<LiveDataIngestPointDto>>> GetSitePointsAsync(
            [FromRoute] Guid siteId,
            [FromQuery] List<Guid> ids,
            [FromQuery] List<string> externalIds,
            [FromQuery] List<Guid> trendIds,
            [FromQuery] bool includePointsWithNoAssets = false,
            [FromHeader] string continuationToken = null)
        {
            return await GetSitePointInfo(siteId, ids, externalIds, trendIds, includePointsWithNoAssets, continuationToken);
        }


        [HttpPost("LiveDataIngest/sites/{siteId}/points")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(Summary = "Gets points by IDs, ExternalIds or TrendIds as specified in body", Tags = new[] { "LiveDataIngest" })]
        public async Task<ActionResult<List<LiveDataIngestPointDto>>> GetSitePointsAsync(
            [FromRoute] Guid siteId,
            [FromBody] LiveDataIngestPointsRequest pointsRequest)
        {
            return await GetSitePointInfo(siteId, pointsRequest.Ids, pointsRequest.ExternalIds,
                pointsRequest.TrendIds, pointsRequest.IncludePointsWithNoAssets);
        }

        [HttpPost("LiveDataIngest/sites/{siteId}/points/paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(Summary = "Gets points by IDs, ExternalIds or TrendIds as specified in body", Tags = new[] { "LiveDataIngest" })]
        public async Task<ActionResult<List<LiveDataIngestPointDto>>> GetSitePointsAsync(
            [FromRoute] Guid siteId,
            [FromBody] LiveDataIngestPointsRequest pointsRequest,
            [FromHeader] string continuationToken = null)
        {
            return await GetSitePointInfo(siteId, pointsRequest.Ids, pointsRequest.ExternalIds,
                pointsRequest.TrendIds, pointsRequest.IncludePointsWithNoAssets, continuationToken);
        }

        [HttpPost("LiveDataIngest/sites/{siteId}/pointvalues")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "postLivePointData", Tags = new[] { "PointData" })]
        public async Task<ActionResult<LiveDataUpdateResponse>> PostPointLiveData(
            [FromRoute] Guid siteId,
            [FromBody] LiveDataUpdateRequest liveData,
            [FromQuery] bool includeUpdatedTwins = false)
        {
            _logger?.LogInformation("Request size: {requestSize}", liveData.UpdatePoints.Count);
            // TODO: investigate why were are loosing correlation in scope
            //using (_logger?.BeginScope("Request size: {requestSize}", liveData.UpdatePoints.Count))
            {
                var twinService = await _digitalTwinServiceProvider.GetForSiteAsync(siteId);
                var updatedTwins = includeUpdatedTwins ? new List<TwinWithRelationshipsDto>() : null;
                var pointResp = new List<LiveDataUpdateResponse.PointUpdateResponse>();

                var uniqIds = liveData.UpdatePoints.Where(p => p.UniqueId != Guid.Empty).Select(p => p.UniqueId).ToList();
                var extIds = liveData.UpdatePoints.Where(p => p.ExternalId != null).Select(p => p.ExternalId).ToList();
                var trendIds = liveData.UpdatePoints.Where(p => p.TrendId != Guid.Empty).Select(p => p.TrendId).ToList();
                var nCoerced = 0;

                if (uniqIds.Count + extIds.Count + trendIds.Count == 0)
                {
                    _logger.LogWarning("No IDs specified - nothing to do");
                }

                var pointAssetPairs = await _assetService.GetPointAssetPairsByPointIdsAsync(
                    siteId, uniqIds, extIds, trendIds, true);

                // TODO: Change these fom HTTP status codes to a specific error enums/consts for this contract
                var notFoundUniques = uniqIds.Where(u => !pointAssetPairs.Any(pap => pap.PointTwin.UniqueId == u));
                foreach (var id in notFoundUniques)
                {
                    pointResp.Add(new LiveDataUpdateResponse.PointUpdateResponse
                    {
                        PointUniqueId = id,
                        Status = StatusCodes.Status404NotFound
                    });
                }
                var notFoundExternals = extIds.Where(e => !pointAssetPairs.Any(pap => string.Equals(pap.PointTwin.ExternalId, e, StringComparison.InvariantCultureIgnoreCase)));
                foreach (var id in notFoundExternals)
                {
                    pointResp.Add(new LiveDataUpdateResponse.PointUpdateResponse
                    {
                        PointExternalId = id,
                        Status = StatusCodes.Status404NotFound
                    });
                }
                var notFoundTrends = trendIds.Where(t => !pointAssetPairs.Any(pap => string.Equals(pap.PointTwin.TrendId, t.ToString(), StringComparison.InvariantCultureIgnoreCase)));
                foreach (var id in notFoundTrends)
                {
                    pointResp.Add(new LiveDataUpdateResponse.PointUpdateResponse
                    {
                        PointTrendId = id,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var patches = new List<(string, LiveDataUpdateResponse.PointUpdateResponse, JsonPatchDocument)>();

                LiveDataUpdateResponse.PointUpdateResponse response = null;
                foreach (var pointInfo in pointAssetPairs)
                {
                    string twinId = null;
                    try
                    {
                        var ldPoint = liveData.UpdatePoints.LastOrDefault(ldp => ldp.TrendId != Guid.Empty && ldp.TrendId == pointInfo.Point.TrendId)
                                    ?? liveData.UpdatePoints.LastOrDefault(ldp => ldp.ExternalId != null && ldp.ExternalId == pointInfo.Point.ExternalId)
                                    ?? liveData.UpdatePoints.LastOrDefault(ldp => ldp.UniqueId != Guid.Empty && ldp.UniqueId == pointInfo.Point.Id);

                        twinId = pointInfo.PointTwin.Id;
                        response = new LiveDataUpdateResponse.PointUpdateResponse
                        {
                            // Include all mapped IDs in response, even in case of error downstream
                            PointId = twinId,
                            PointUniqueId = pointInfo.Point.Id,
                            PointTrendId = pointInfo.Point.TrendId,
                            PointExternalId = pointInfo.Point.ExternalId,
                            AssetUniqueId = pointInfo.Asset?.Id ?? Guid.Empty,
                            Status = StatusCodes.Status200OK
                        }; // Note we may modify this status in-place below in case of error
                        pointResp.Add(response);

                        if (!pointInfo.PointTwin.CustomProperties.TryGetValue(Properties.Type, out var adtPointType))
                        {
                            _logger?.LogError($"Error updating pointValue: 'type' property is missing on ADT Capability twin {twinId}", twinId);
                            response.Status = StatusCodes.Status422UnprocessableEntity;
                        }
                        else if (includeUpdatedTwins)
                        {
                            updatedTwins.Add(TwinWithRelationshipsDto.MapFrom(pointInfo.PointTwin));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception processing twin {TwinId}", twinId);
                        if (response != null)
                            response.Status = (int)StatusCodes.Status500InternalServerError;
                    }
                } // foreach

                _logger.LogInformation("PostPointsLiveData summary: {Summary}",
                    JsonSerializer.Serialize(
                        pointResp.GroupBy(r => r.Status,
                            (status, r) => new { Status = status, Count = r.Count() })));

                var updateResponse = new LiveDataUpdateResponse { UpdatedTwins = updatedTwins, Responses = pointResp };

                if (pointResp.Any(pr => pr.Status != StatusCodes.Status200OK))
                    Response.StatusCode = StatusCodes.Status207MultiStatus;

                return updateResponse;
            }
        }

        private async Task<ActionResult<List<LiveDataIngestPointDto>>> GetSitePointInfo(
            Guid siteId,
            List<Guid> ids,
            List<string> externalIds,
            List<Guid> trendIds,
            bool includePointsWithNoAssets = false)
        {
            var fullCount = ids.Count + externalIds.Count + trendIds.Count;

            var pointAssetPairs = await _assetService.GetSimplePointAssetPairsByPointIdsAsync(siteId, ids, externalIds, trendIds, includePointsWithNoAssets);
                        
            if (pointAssetPairs.Count() != fullCount)
                Response.StatusCode = StatusCodes.Status207MultiStatus;

            return pointAssetPairs.ToList();
        }

        private async Task<ActionResult<List<LiveDataIngestPointDto>>> GetSitePointInfo(
            Guid siteId,
            List<Guid> ids,
            List<string> externalIds,
            List<Guid> trendIds,
            bool includePointsWithNoAssets = false,
            string continuationToken = null)
        {
            var fullCount = ids.Count + externalIds.Count + trendIds.Count;

            var pointAssetPairsPage = await _assetService.GetPointAssetPairsByPointIdsAsync(siteId, ids, externalIds, trendIds, includePointsWithNoAssets, continuationToken);

            var result = pointAssetPairsPage.Content.Select(
                x => new LiveDataIngestPointDto
                {
                    UniqueId = x.Point.Id,
                    AssetId = x.Asset?.Id ?? Guid.Empty,
                    TrendId = x.Point.TrendId,
                    ExternalId = x.Point.ExternalId
                })
                .ToList();

            if (pointAssetPairsPage.Content.Count() != fullCount)
                Response.StatusCode = StatusCodes.Status207MultiStatus;

            return result;
        }

        private (object,int,bool) GetPointValue(string adtPointType, LiveDataUpdatePointDto ldPoint, string twinId)
        {
            object pointValue = null;
            var coerced = false;

            // TODO: this logic needs to be re-factored out and generalized for when can have MULTISTATE or arbitrary JSON values

            switch (adtPointType.ToUpper())
            {
                case Dtos.PointTypes.Binary:

                    if (ldPoint.Value.ValueKind == JsonValueKind.True || ldPoint.Value.ValueKind == JsonValueKind.False)
                        pointValue = ldPoint.Value.GetBoolean();
                    else if (ldPoint.Value.ValueKind == JsonValueKind.Number)
                    {
                        pointValue = (int) ldPoint.Value.GetDouble() != 0;
                        coerced = true;
                    }
                    else if (ldPoint.Value.ValueKind == JsonValueKind.String)
                    {
                        pointValue = (int) double.Parse(ldPoint.Value.GetString()) != 0;
                        coerced = true;
                    }
                    else
                    {
                        _logger.LogError(StatusCodes.Status422UnprocessableEntity,
                            "Error updating pointValue: invalid binary value for {TwinId} ({LdPoint})", twinId, ldPoint.Value);
                        return (null, StatusCodes.Status422UnprocessableEntity, coerced);
                    }
                    break;

                case Dtos.PointTypes.Analog:

                    if (ldPoint.Value.ValueKind == JsonValueKind.Number)
                        pointValue = ldPoint.Value.GetDouble();
                    else if (ldPoint.Value.ValueKind == JsonValueKind.True || ldPoint.Value.ValueKind == JsonValueKind.False)
                    {
                        pointValue = ldPoint.Value.GetBoolean() ? 1.0 : 0.0;
                        coerced = true;
                    }
                    else if (ldPoint.Value.ValueKind == JsonValueKind.String)
                    {
                        pointValue = double.Parse(ldPoint.Value.GetString());
                        coerced = true;
                    }
                    else
                    {
                        _logger.LogError(StatusCodes.Status422UnprocessableEntity,
                            "Error updating pointValue: invalid analog value for {TwinId} ({LdPoint})", twinId, ldPoint.Value);
                        return (null, StatusCodes.Status422UnprocessableEntity, coerced);
                    }
                    break;

                default:
                    pointValue = ldPoint.Value;
                    break;
            }

            return (pointValue, StatusCodes.Status200OK, coerced);
        }
    }
}
