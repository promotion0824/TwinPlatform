using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using DigitalTwinCore.Models;
using System.Linq;
using DigitalTwinCore.DTO;
using System.Threading;

namespace DigitalTwinCore.Controllers
{
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet("AssetTree")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetTree", Tags = new[] { "Assets" })]
        [Obsolete]
        public async Task<ActionResult<List<AssetTreeCategoryDto>>> GetAssetTreeAsync(
            [FromRoute] Guid siteId,
            // We can also add other params (as in getAssets) so all work is done here 
            //  rather than in PlatformPortalXL or the front-end. 
            // TODO: Verify that client code needs both getAssets and getAssetTree, or if the caller can simply transform one to the other
            [FromQuery] Guid? floorId,
            [FromQuery] bool isCategoryOnly,
            // Top Level Models to be returned
            [FromQuery] List<string> modelNames)
        {
            // TODO: If we need to, use controller interceptor for extra controller logging

            using (_assetService.Logger?.BeginScope("GetAssetTree. site:{siteId}, floor:{floorId}", siteId, floorId))
            {
                var output = await _assetService.GetCategoriesAndAssetsAsync(siteId, isCategoryOnly, modelNames, floorId);
                return Ok(AssetTreeCategoryDto.Map(output));
            }
        }

        [HttpGet("categories")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetCategories", Tags = new[] { "Assets" })]
        public async Task<ActionResult<IEnumerable<AssetTreeCategoryDto>>> GetAssetCategories(
            [FromRoute] Guid siteId,
            [FromQuery] Guid? floorId,
            [FromQuery] bool isLiveDataOnly = false)
        {
            using (_assetService.Logger?.BeginScope("GetAssetTree. site:{siteId}", siteId))
            {
                var output = await _assetService.GetCategories(siteId, floorId, isLiveDataOnly);
                return Ok(output);
            }
        }

        [HttpGet("assettree/paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetTree", Tags = new[] { "Assets" })]
        [Obsolete]
        public async Task<ActionResult<List<AssetTreeCategoryDto>>> GetAssetTreeAsync(
            [FromRoute] Guid siteId,
            [FromQuery] Guid? floorId,
            [FromQuery] bool isCategoryOnly,
            [FromQuery] List<string> modelNames, // Top Level Models
            [FromHeader] string continuationToken = null)
        {
            using (_assetService.Logger?.BeginScope("GetAssetTree. site:{siteId}, floor:{floorId}", siteId, floorId))
            {
                var output = await _assetService.GetCategoriesAndAssetsAsync(siteId, isCategoryOnly, modelNames, floorId, continuationToken);

                return Ok(new Page<AssetTreeCategoryDto>
                {
                    Content = output.Content.Select(x => AssetTreeCategoryDto.Map(x)),
                    ContinuationToken = output.ContinuationToken
                });
            }
        }

        /// <summary>
        /// Retrieves an asset and its parameters by its unique id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetByUniqueId", Tags = new[] { "Assets" })]
        public async Task<ActionResult<AssetDto>> GetByUniqueIdAsync([FromRoute] Guid siteId, [FromRoute] Guid id)
        {
            var output = await _assetService.GetAssetByUniqueId(siteId, id);

            if (output == null)
                return NotFound();

            return Ok(AssetDto.MapFrom(output));
        }

        /// <summary>
        /// Retrieves an asset and its parameters by its twin id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("twinId/{id}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetById", Tags = new[] { "Assets" })]
        public async Task<ActionResult<AssetDto>> GetByIdAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var output = await _assetService.GetAssetById(siteId, id);

            if (output == null)
                return NotFound();

            return Ok(AssetDto.MapFrom(output));
        }

        /// <summary>
        /// Retrieves an asset and its parameters by its unique forgeViewer id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("forgeViewerId/{id}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetByForgeViewerId", Tags = new[] { "Assets" })]
        public async Task<ActionResult<AssetDto>> GetByForgeViewerIdAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var output = await _assetService.GetAssetByForgeViewerId(siteId, id);

            if (output == null)
                return NotFound();

            return Ok(AssetDto.MapFrom(output));
        }

        /// <summary>
        /// Get List of Twins with GeometryViewerId by specific Building, Floor or array of ModuleTypeName
        /// </summary>
        /// <returns>Returns Twins with GeometryViewerId </returns>
        [HttpPost("/assets/GeometryViewerIds")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "GetTwinsWithGeometryId", Tags = new[] { "Assets" })]
        public async Task<ActionResult<List<TwinGeometryViewerIdDto>>> GetTwinsWithGeometryIdAsync([FromBody] GetTwinsWithGeometryIdRequest request)
        {
            var output = await _assetService.GetTwinsWithGeometryIdAsync(request);

            if (output == null)
                return NotFound();

            return Ok(output);
        }

        /// <summary>
        /// Retrieves a page of assets by parameters
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="categoryId"></param>
        /// <param name="floorId"></param>
        /// <param name="searchKeyword"></param>
        /// <param name="liveDataOnly"></param>
        /// <param name="includeExtraProperties"></param>
        /// <returns></returns>
        [HttpGet("paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssets", Tags = new[] { "Assets" })]
        public async Task<ActionResult<Page<AssetDto>>> GetAssetsAsync(
            [FromRoute] Guid siteId,
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid? floorId,
            [FromQuery] string searchKeyword,
            [FromQuery] bool? liveDataOnly,
            [FromQuery] bool? includeExtraProperties,
            [FromHeader] string continuationToken = null)
        {
            using (_assetService.Logger?.BeginScope("GetAssets. site:{siteId}, floor:{floorId}", siteId, floorId))
            {
                var assets = await _assetService.GetAssets(
                    siteId,
                    categoryId,
                    floorId,
                    searchKeyword,
                    liveDataOnly ?? false,
                    includeExtraProperties ?? false,
                    continuationToken);

                return Ok(new Page<AssetDto>
                {
                    Content = assets.Content.Select(x => AssetDto.MapFrom(x)),
                    ContinuationToken = assets.ContinuationToken
                });
            }
        }

        /// <summary>
        /// Retrieves list of assets by parameters.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="categoryId"></param>
        /// <param name="floorId"></param>
        /// <param name="searchKeyword"></param>
        /// <param name="liveDataOnly"></param>
        /// <param name="includeExtraProperties"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssets", Tags = new[] { "Assets" })]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssetsAsync(
            [FromRoute] Guid siteId,
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid? floorId,
            [FromQuery] string searchKeyword,
            [FromQuery] bool? liveDataOnly,
            [FromQuery] bool? includeExtraProperties,
            [FromQuery] int pageNumber = 0,
            [FromQuery] int pageSize = 500)
        {
            using (_assetService.Logger?.BeginScope("GetAssets. site:{siteId}, floor:{floorId}", siteId, floorId))
            {
                // Poor-man's paging: no snapshot is made and associated w/a continuation-token -- it's possible the twins can change between calls
                var startItemIndex = pageNumber * pageSize;
                var assets = await _assetService.GetAssetsAsync(
                    siteId,
                    categoryId,
                    floorId,
                    searchKeyword,
                    liveDataOnly ?? false,
                    includeExtraProperties ?? false,
                    startItemIndex,
                    pageSize);

                return Ok(AssetDto.MapFrom(assets));
                // To achieve filtering by specific props, we can return a JSON result, however System.Text.Json does not currently support 
                //   conditional serialization of properties, so we would need to switch to Newtonsoft.Json
                // public async Task<JsonResult> GetAssetsAsync(
                //var json = JsonSerializer.Serialize(dtos);
                //return new JsonResult(json);
                //return new JsonResult(dtos);
            }
        }

        /// <summary>
        /// Retrieves the available documents for an asset
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/documents")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetDocuments", Tags = new[] { "Assets" })]
        public async Task<ActionResult<List<DocumentDto>>> GetAssetDocumentsAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid id)
        {
            var output = await _assetService.GetDocumentsForAssetAsync(siteId, id);

            if (output == null)
                return NotFound();

            return Ok(DocumentDto.MapFrom(output));
        }

        /// <summary>
        /// Retrieves the relationships for an asset
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/relationships")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetRelationships", Tags = new[] { "Assets" })]
        public async Task<ActionResult<AssetRelationshipsDto>> GetAssetRelationshipsAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid id)
        {
            var output = await _assetService.GetAssetRelationshipsAsync(siteId, id);

            if (output == null)
                return NotFound();

            return Ok(output);
        }

        /// <summary>
        /// Retrieves names by asset unique id.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("names")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetNamesById", Tags = new[] { "Assets" })]
        public async Task<ActionResult<IEnumerable<AssetNameDto>>> GetAssetNamesAsync(
            [FromRoute] Guid siteId,
            [FromBody] IEnumerable<Guid> ids)
        {
            if (!ids.Any())
                return BadRequest("No asset IDs provided.");

            var output = await _assetService.GetAssetNames(siteId, ids);

            return Ok(output);
        }

        /// <summary>
        /// Retrieves Twin base data by ids for multiple sites.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of TwinSimpleDto which include Id, siteId, UniqueId and Name</returns>
        [HttpPost("/sites/[Controller]/names")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation("Get twins for multiple sites by ids ", OperationId = "getSitesTwinsBaseDataAsync", Tags = new[] { "Twins" })]
        public async Task<ActionResult<List<TwinSimpleDto>>> GetSimpleTwinsDataAsync([FromBody] IEnumerable<TwinsForMultiSitesRequest> request, CancellationToken cancellationToken)
        {
	        if (request == null || !request.Any())
	        {
		        return Ok(new List<TwinSimpleDto>());
	        }
	        var data = await _assetService.GetSimpleTwinsDataAsync(request, cancellationToken);

	        return Ok(data);
        }

      
	}
}
