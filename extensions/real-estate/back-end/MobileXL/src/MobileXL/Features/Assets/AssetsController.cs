using System;
using System.Net.Mime;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Dto;
using MobileXL.Services;

namespace MobileXL.Features.Assets
{
    [ApiController]
    #if !DEBUG
    [Authorize]
    #endif
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AssetsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDigitalTwinService _digitalTwinService;
        public AssetsController(IAccessControlService accessControl, IDigitalTwinService digitalTwinService)
        {
            _accessControl = accessControl;
            _digitalTwinService = digitalTwinService;
        }

        [HttpGet("sites/{siteId}/assets/{assetId}/files/{fileId}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> GetFile([FromRoute] Guid siteId, [FromRoute] Guid assetId, [FromRoute] Guid fileId, [FromQuery] bool inline)
        {
            #if !DEBUG
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);
            #endif

            var assetService = await _digitalTwinService.GetAssetServiceAsync(siteId);
            var fileDownload = await assetService.GetFileAsync(siteId, assetId, fileId);
            var contentDisposition = new ContentDisposition
            {
                FileName = fileDownload.FileName,
                Inline = inline
            };
            Response.Headers.Add("Content-Disposition", $"{contentDisposition}");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return File(fileDownload.Content, fileDownload.ContentType.MediaType);
        }

        [HttpGet("sites/{siteId}/assets/{assetId}/files")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<List<AssetFileDto>>> GetFiles([FromRoute] Guid siteId, [FromRoute] Guid assetId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            var assetService = await _digitalTwinService.GetAssetServiceAsync(siteId);
            var assetFiles = await assetService.GetFilesAsync(siteId, assetId);
            return AssetFileDto.MapFromModels(assetFiles);
        }

        [HttpGet("sites/{siteId}/assets/{assetId}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<AssetDetailDto>> GetAsset([FromRoute] Guid siteId, [FromRoute] Guid assetId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            var assetService = await _digitalTwinService.GetAssetServiceAsync(siteId);
            var asset = await assetService.GetAssetDetailsAsync(siteId, assetId);
            return AssetDetailDto.MapFromModel(asset);
        }

        [HttpGet("sites/{siteId}/categoryTree")]
        [Obsolete("It will be removed in future version.")]
        public async Task<ActionResult<IEnumerable<AssetCategoryDto>>> GetAssetCategoryTree([FromRoute] Guid siteId, [FromQuery] Guid? floorId, [FromQuery] bool? liveDataAssetsOnly)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            var assetService = await _digitalTwinService.GetAssetServiceAsync(siteId);
            var categories = await assetService.GetAssetCategoriesTreeAsync(siteId, floorId, liveDataAssetsOnly);
            return AssetCategoryDto.MapFromModels(categories);
        }

        [HttpGet("sites/{siteId}/assets/categories")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LightCategoryDto>>> GetCategoryTree([FromRoute] Guid siteId, [FromQuery] Guid? floorId, [FromQuery] bool? liveDataAssetsOnly)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);
            var assetService = await _digitalTwinService.GetAssetServiceAsync(siteId);
            var categories = await assetService.GetCategories(siteId, floorId, liveDataAssetsOnly);
            return Ok(categories);
        }

        [HttpGet("sites/{siteId}/assets")]
        public async Task<ActionResult<IEnumerable<AssetSimpleDto>>> GetAssets(
            [FromRoute] Guid siteId, 
            [FromQuery] Guid? categoryId, 
            [FromQuery] Guid? floorId, 
            [FromQuery] bool? liveDataAssetsOnly,
            [FromQuery] string searchKeyword,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            var assetService = await _digitalTwinService.GetAssetServiceAsync(siteId);
            var pagedAssets = await assetService.GetAssetsPagedAsync(siteId, categoryId, floorId, liveDataAssetsOnly, searchKeyword, pageNumber, pageSize);
            return AssetSimpleDto.MapFromModels(pagedAssets);
        }
    }
}
