using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Controllers
{
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class PointsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public PointsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        /// <summary>
        /// Retrieves list of points for the given siteId, optionally including assets
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="includeAssets"></param>
        /// <returns></returns>
        [HttpGet("/sites/{siteId}/[controller]/paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPoints", Tags = new[] { "Points" })]
        public async Task<ActionResult<Page<PointDto>>> GetListAsync(
            [FromRoute] Guid siteId,
            [FromQuery] bool? includeAssets,
            [FromHeader] string continuationToken)
        {
            var output = await _assetService.GetPointsAsync(siteId, includeAssets.HasValue && includeAssets.Value, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

            return Ok(new Page<PointDto> { Content = output.Content.Select(x => PointDto.MapFrom(x, includeAssets, false)), ContinuationToken = output.ContinuationToken });
        }

        /// <summary>
        /// Retrieves list of points for the given siteId, optionally including assets
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="includeAssets"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPoints", Tags = new[] { "Points" })]
        public async Task<ActionResult<List<PointDto>>> GetListAsync(
            [FromRoute] Guid siteId,
            [FromQuery] bool? includeAssets,
            [FromQuery] int pageSize = int.MaxValue, int pageStart = 0)
        {
            var startItemIndex = pageStart * pageSize;
            var output = await _assetService.GetPointsAsync(siteId, includeAssets.HasValue && includeAssets.Value, startItemIndex, pageSize);

            return Ok(PointDto.MapFrom(output, includeAssets, false));
        }

        /// <summary>
        /// Retrieves list of points for the given siteId and connectorId, optionally including assets
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="connectorId"></param>
        /// <param name="includeAssets"></param>
        /// <returns></returns>
        [HttpGet("/sites/{siteId}/connectors/{connectorId}/[Controller]/paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointsByConnector", Tags = new[] { "Points" })]
        public async Task<ActionResult<Page<PointDto>>> GetListByConnectorAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid connectorId,
            [FromQuery] bool? includeAssets,
            [FromHeader] string continuationToken)
        {
            var output = await _assetService.GetPointsByConnectorAsync(siteId, connectorId, includeAssets.HasValue && includeAssets.Value, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

            return Ok(new Page<PointDto> { Content = output.Content.Select(x => PointDto.MapFrom(x, includeAssets, true)), ContinuationToken = output.ContinuationToken });
        }

        /// <summary>
        /// Retrieves list of points for the given siteId and connectorId, optionally including assets
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="connectorId"></param>
        /// <param name="includeAssets"></param>
        /// <returns></returns>
        [HttpGet("/sites/{siteId}/connectors/{connectorId}/[Controller]")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointsByConnector", Tags = new[] { "Points" })]
        public async Task<ActionResult<List<PointDto>>> GetListByConnectorAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid connectorId,
            [FromQuery] bool? includeAssets)
        {
            var output = await _assetService.GetPointsByConnectorAsync(siteId, connectorId, includeAssets.HasValue && includeAssets.Value);
            return Ok(PointDto.MapFrom(output, includeAssets, true));
        }

        /// <summary>
        /// Retrieves count of enabled points for the given siteId
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpGet("count")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getCount", Tags = new[] { "Points" })]
        public async Task<ActionResult<CountResponse>> GetCountAsync([FromRoute] Guid siteId)
        {
            var pointsCount = await _assetService.GetPointsCountAsync(siteId);
            return Ok(new CountResponse { Count = pointsCount });
        }

        /// <summary>
        /// Retrieves count of enabled points for the given siteId and connectorId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="connectorId"></param>
        /// <param name="includeAssets"></param>
        /// <returns></returns>
        [HttpGet("/sites/{siteId}/connectors/{connectorId}/[Controller]/count")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getCountByConnector", Tags = new[] { "Points" })]
        public async Task<ActionResult<List<PointDto>>> GetCountByConnectorAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid connectorId,
            [FromQuery] bool? includeAssets)
        {

            var pointsCount = await _assetService.GetPointsByConnectorCountAsync(siteId, connectorId);
            return Ok(new CountResponse { Count = pointsCount });
        }

        /// <summary>
        /// Retrieves a list of points that have the specified tag
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpGet("ByTag/{tag}/paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointsByTag", Tags = new[] { "Points" })]
        public async Task<ActionResult<Page<PointDto>>> GetPointsByTagAsync(
            [FromRoute] Guid siteId,
            [FromRoute] string tag,
            [FromHeader] string continuationToken)
        {
            var output = await _assetService.GetPointsByTagAsync(siteId, tag, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

            return Ok(new Page<PointDto> { Content = output.Content.Select(x => PointDto.MapFrom(x, true, false)), ContinuationToken = output.ContinuationToken });
        }

        /// <summary>
        /// Retrieves a list of points that have the specified tag
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpGet("ByTag/{tag}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointsByTag", Tags = new[] { "Points" })]
        public async Task<ActionResult<List<PointDto>>> GetPointsByTagAsync(
            [FromRoute] Guid siteId,
            [FromRoute] string tag)
        {
            var output = await _assetService.GetPointsByTagAsync(siteId, tag);
            return Ok(PointDto.MapFrom(output, true, false));
        }


        /// <summary>
        /// Retrieves a point by its unique id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointByUniqueId", Tags = new[] { "Points" })]
        public async Task<ActionResult<PointDto>> GetPointByUniqueIdAsync([FromRoute] Guid siteId, [FromRoute] Guid id)
        {
            var point = await _assetService.GetPointByUniqueIdAsync(siteId, id);
            if (point == null)
            {
                throw new ResourceNotFoundException("point", id);
            }
            return Ok(PointDto.MapFrom(point, true, false));
        }

        /// <summary>
        /// Retrieves a point by its trend time series id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="trendId"></param>
        /// <returns></returns>
        [HttpGet("trendId/{trendId}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointByTrendId", Tags = new[] { "Points" })]
        public async Task<ActionResult<PointDto>> GetPointByTrendIdAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid trendId)
        {
            var point = await _assetService.GetPointByTrendIdAsync(siteId, trendId);
            if (point == null)
            {
                throw new ResourceNotFoundException("point", trendId);
            }
            return Ok(PointDto.MapFrom(point, true, true));
        }

        /// <summary>
        /// Retrieves points by a list of trendIds
        /// TrendIds that are not found do not result in an error - they are omitted from the returned list.
        /// The order of returned results is not guaranteed to match the order of passed-in Ids.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="trendIds"></param>
        /// <returns></returns>
        [HttpPost("trendIds")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "postPointsByTrendIds", Tags = new[] { "Points" })]
        public async Task<ActionResult<List<PointDto>>> GetPointsByTrendIdsAsync(
            [FromRoute] Guid siteId,
            [FromBody] List<Guid> trendIds)
        {
            var points = await _assetService.GetPointsByTrendIdsAsync(siteId, trendIds);
            return Ok(PointDto.MapFrom(points, true, false));
        }

        /// <summary>
        /// Retrieves a point by its twin id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <returns></returns>
        [HttpGet("twinId/{twinId}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointByTwinId", Tags = new[] { "Points" })]
        public async Task<ActionResult<PointDto>> GetPointByTwinIdAsync(
            [FromRoute] Guid siteId,
            [FromRoute] string twinId)
        {
            var point = await _assetService.GetPointByTwinIdAsync(siteId, twinId);
            if (point == null)
            {
                throw new ResourceNotFoundException("point", twinId);
            }
            return Ok(PointDto.MapFrom(point, true, true));
        }

        /// <summary>
        /// Retrieves a point by its external id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="externalId"></param>
        /// <returns></returns>
        [HttpGet("externalId")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointByExternalId", Tags = new[] { "Points" })]
        public async Task<ActionResult<PointDto>> GetPointByExternalIdAsync(
            [FromRoute] Guid siteId,
            [FromQuery] string externalId)
        {
            var point = await _assetService.GetPointByExternalIdAsync(siteId, externalId);
            if (point == null)
            {
                throw new ResourceNotFoundException("point", externalId);
            }
            return Ok(PointDto.MapFrom(point, true, true));
        }
    }
}
