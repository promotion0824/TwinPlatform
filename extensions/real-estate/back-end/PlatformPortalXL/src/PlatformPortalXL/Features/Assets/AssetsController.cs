using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.Assets;
using PlatformPortalXL.Services.LiveDataApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Microsoft.Extensions.Logging;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using Willow.Logging;
using Willow.Platform.Localization;
using static PlatformPortalXL.Dto.AssetPinDto;

namespace PlatformPortalXL.Features.Assets
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AssetsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDigitalTwinAssetService _digitalTwinService;
        private readonly ISiteApiService _siteApi;
        private readonly ILiveDataApiService _liveDataApi;
        private readonly IFloorsService _floorService;
        private readonly IAssetLocalizerFactory _assetLocalizerFactory;
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(IAccessControlService accessControl,
                                IDigitalTwinAssetService digitalTwinService,
                                ISiteApiService directoryApi,
                                ILiveDataApiService liveDataApi,
                                IFloorsService floorService,
                                IAssetLocalizerFactory assetLocalizerFactory,
                                ILogger<AssetsController> logger)
        {
            _accessControl = accessControl;
            _digitalTwinService = digitalTwinService;
            _siteApi = directoryApi;
            _liveDataApi = liveDataApi;
            _floorService = floorService;
            _assetLocalizerFactory = assetLocalizerFactory;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an asset and its parameters by equipment id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="equipmentId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/assets/byequipment/{equipmentId}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<AssetDetailDto>> GetByEquipment([FromRoute] Guid siteId, [FromRoute] Guid equipmentId, [FromHeader] string language)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var assetLocalizer = await _assetLocalizerFactory.GetLocalizer(language);

            var asset = await _digitalTwinService.GetAssetDetailsByEquipmentIdAsync(siteId, equipmentId);
            return AssetDetailDto.MapFromModel(asset, assetLocalizer);
        }

        /// <summary>
        /// Returns the file with the given fileId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="assetId"></param>
        /// <param name="fileId"></param>
        /// <param name="inline"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/assets/{assetId}/files/{fileId}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> GetFile([FromRoute] Guid siteId, [FromRoute] Guid assetId, [FromRoute] Guid fileId, [FromQuery] bool inline)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var fileDownload = Building121Demo.DownloadAssetFile(fileId);
            if (fileDownload == null)
            {
                fileDownload = await _digitalTwinService.GetFileAsync(siteId, assetId, fileId);
            }

            var fileName      = fileDownload.FileName
		        .Replace("\r", " ")
		        .Replace("\n", " ")
		        .Replace("\t", " ")
		        .Replace("  ", " ")
		        .Trim();
            var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName).Trim();
            var extension     = Path.GetExtension(fileName).Trim();

            if(extension.StartsWith("."))
                extension = "." + extension.Substring(1).Trim();

            var contentDisposition = new ContentDisposition
            {
                FileName = Uri.EscapeDataString(fileNameNoExt + extension),
                Inline = inline
            };

            Response.Headers.Add("Content-Disposition", $"{contentDisposition}");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            var contentType = extension == ".pdf" ? "application/pdf" : fileDownload.ContentType.MediaType;

            return File(fileDownload.Content, contentType);
        }

        /// <summary>
        /// Retrieves the files for an asset
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/assets/{assetId}/files")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<List<AssetFileDto>>> GetFiles([FromRoute] Guid siteId, [FromRoute] Guid assetId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var assetFiles = await _digitalTwinService.GetFilesAsync(siteId, assetId);
            await Building121Demo.UpdateAssetFiles(_digitalTwinService, siteId, assetId, assetFiles);
            return AssetFileDto.MapFromModels(assetFiles);
        }

        /// <summary>
        /// Retrieves the files for an asset
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/assets/{assetId}/warranty")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<List<AssetDetailDto>>> GetWarranty([FromRoute] Guid siteId, [FromRoute] Guid assetId, [FromHeader] string language)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var assetLocalizer = await _assetLocalizerFactory.GetLocalizer(language);
            var assets = await _digitalTwinService.GetWarrantyAsync(siteId, assetId);
            return AssetDetailDto.MapFromModels(assets, assetLocalizer);
        }

        /// <summary>
        /// Retrieves an asset and its parameters by forge viewer model id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="forgeViewerModelId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/assets/byforgeviewermodelid/{forgeViewerModelId}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<AssetDetailDto>> GetByForgeViewerModelId([FromRoute] Guid siteId, [FromRoute] string forgeViewerModelId, [FromHeader] string language)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var assetLocalizer = await _assetLocalizerFactory.GetLocalizer(language);
            var asset = await _digitalTwinService.GetAssetDetailsByForgeViewerModelIdAsync(siteId, forgeViewerModelId);
            return AssetDetailDto.MapFromModel(asset, assetLocalizer);
        }

        /// <summary>
        /// Retrieves an asset and its properties
        /// </summary>
        [HttpGet("sites/{siteId}/assets/{assetId}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<AssetDetailDto>> GetAsset([FromRoute] Guid siteId, [FromRoute] Guid assetId, [FromHeader] string language)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var assetLocalizer = await _assetLocalizerFactory.GetLocalizer(language);
            var asset = await _digitalTwinService.GetAssetDetailsAsync(siteId, assetId);
            return AssetDetailDto.MapFromModel(asset, assetLocalizer);
        }

        /// <summary>
        /// Retrieves pin information of the specified asset
        /// </summary>
        [HttpGet("sites/{siteId}/assets/{assetId}/pinOnLayer")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<AssetPinDto>> GetAssetPin([FromRoute] Guid siteId, [FromRoute] Guid assetId, [FromQuery] bool includeAllPoints = false)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var asset = await _digitalTwinService.GetAssetDetailsAsync(siteId, assetId);

            var site = await _siteApi.GetSite(siteId);

            List<AssetPoint> points;
            if (includeAllPoints)
            {
                points = asset.Points;
            }
            else
            {
                if (asset.Points != null)
                {
                   // if ADT site, use DisplayPriority to determine if point should be returned
                   points = asset.Points.Where(x => x.DisplayPriority != null || x.DisplayPriority < AdtConstants.MaxDisplayPriority).ToList();
                }
                else
                {
                    points = new List<AssetPoint>();
                }
            }

            var liveDataPoints = new List<LiveDataPoint>();
            var twinIds = points.Select(x => x.TwinId).ToList();

            if (twinIds.Any())
            {
                var livedataResult = await _liveDataApi.GetLastTrendlogsAsync(site.CustomerId, siteId, twinIds);

                var liveDataPointsQuery = from point in points
                                     join rawValue in livedataResult on point.TwinId equals rawValue.Id into joinedValues
                                     from joinedValue in joinedValues.DefaultIfEmpty()
                                     select new AssetPinDto.LiveDataPoint
                                     {
                                         Id = point.Id,
                                         Tag = point.DisplayName,
                                         Unit = point.Unit,
                                         LiveDataTimestamp = joinedValue?.Timestamp,
                                         LiveDataValue = joinedValue?.Value
                                     };

                liveDataPoints = liveDataPointsQuery.ToList();
            }

            return new AssetPinDto
            {
                Title = asset.Name,
                LiveDataPoints = liveDataPoints.ToList()
            };
        }

        /// <summary>
        /// Retrieves the asset category tree by building Id
        /// </summary>
        [HttpGet("sites/{siteId}/categoryTree")]
        [Authorize]
        [Obsolete]
        public async Task<ActionResult<IEnumerable<AssetCategoryDto>>> GetAssetCategoryTree([FromRoute] Guid siteId, [FromQuery] Guid? floorId, [FromQuery] bool? liveDataAssetsOnly)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var categories = await _digitalTwinService.GetAssetCategoriesTreeAsync(siteId, floorId, liveDataAssetsOnly);
            return AssetCategoryDto.MapFromModels(categories);
        }

		[HttpGet("sites/{siteId}/assets/categories")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LightCategoryDto>>> GetCategoryTree([FromRoute] Guid siteId,
                                                                                       [FromQuery] Guid? floorId,
                                                                                       [FromQuery] bool? liveDataAssetsOnly,
                                                                                       [FromHeader] string language)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            _logger.LogInformation("Language requested is {Language}", language);

            var categories   = await _digitalTwinService.GetCategories(siteId, floorId, liveDataAssetsOnly);
            var localizer    = await _assetLocalizerFactory.GetLocalizer(language);

            if(localizer.Locale != language)
            {
                _logger.LogWarning("Language that will be used is {Locale}", localizer.Locale);
            }

            if (localizer.Locale != "en")
            {
                localizer.Localize(categories);
            }

            return Ok(categories);
        }

        /// <summary>
        /// Retrieves list asset and equipment combination
        /// </summary>
        [HttpGet("sites/{siteId}/assets")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AssetSimpleDto>>> GetAssets(
            [FromRoute] Guid siteId,
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid? floorId,
            [FromQuery] string floorCode,
            [FromQuery] bool? liveDataAssetsOnly,
            [FromQuery] bool? subCategories,
            [FromQuery] string searchKeyword,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            Guid? FinalFloorId = floorId;

			await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
			var floors = await _floorService.GetFloorsAsync(siteId, false);
			if (!string.IsNullOrEmpty(floorCode))
            {
				var floor = floors.FirstOrDefault(x => x.Code == floorCode);
                if (floor == null)
                    throw new NotFoundException().WithData(new { floorCode });

                FinalFloorId = floor.Id;
            }

            var pagedAssets = await _digitalTwinService.GetAssetsPagedAsync(siteId, categoryId, FinalFloorId, liveDataAssetsOnly, subCategories, searchKeyword, pageNumber, pageSize);
			var assetList = AssetSimpleDto.MapFromModels(pagedAssets);
			EnrichAssets(assetList, floors);

			return assetList;

		}
		private List<AssetSimpleDto> EnrichAssets(List<AssetSimpleDto> assetList, List<Floor> floors)
		{
			foreach (var asset in assetList)
			{
				var floor = floors.FirstOrDefault(x => x.Id == asset.FloorId);
				if (floor is not null)
				{
					asset.FloorCode = floor.Code;
				}
			}
			return assetList;
		}
	}

    public class Building121Demo
    {
        public static string SinkDocumentFileName => "Kohler-K-2030_spec_US-CA_Kohler_en.pdf";
        public static Guid SinkDocumentFileId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static string FanPoweredBoxDocumentFileName => "NailorCatalog-TerminalUnitsFPTUSeriesStndHeight35S.pdf";
        public static Guid FanPoweredBoxDocumentFileId => Guid.Parse("22222222-1111-1111-1111-111111111111");
        public static string TraneSxhkDocumentFileName => "Trane-SXHK.pdf";
        public static Guid TraneSxhkDocumentFileId => Guid.Parse("33333333-1111-1111-1111-111111111111");
        public static string TraneThcyscDocumentFileName => "Trane-THC-YSC.pdf";
        public static Guid TraneThcyscDocumentFileId => Guid.Parse("55555555-1111-1111-1111-111111111111");

        public static Guid Building121SiteIdInProduction => Guid.Parse("53d380c2-d31a-4cd1-8958-795407407a82");
        public static Guid Building121SiteIdInUat => Guid.Parse("3148e57e-9366-4438-a9af-90f0ed175cf6");

        public static async Task UpdateAssetFiles(Services.Assets.IDigitalTwinAssetService assetService, Guid siteId, Guid assetId, List<AssetFile> assetFiles)
        {
            if (siteId != Building121SiteIdInProduction && siteId != Building121SiteIdInUat)
            {
                return;
            }

            var asset = await assetService.GetAssetDetailsAsync(siteId, assetId);
            if (asset.CategoryId == Guid.Parse("f5a18a96-670b-e8e8-46e3-d734f381de78") // Category "Fan Powered Box with Reheat"
                || asset.CategoryId == Guid.Parse("0e6beeec-a9af-80b0-0c46-1c9d41b63f29")) // Category "Fan Powered Box"
            {
                assetFiles.Add(new AssetFile
                {
                    Id = FanPoweredBoxDocumentFileId,
                    FileName = FanPoweredBoxDocumentFileName
                });
            }
            if (asset.Identifier == "MS-PS-B121-1001.P3B.1")
            {
                assetFiles.Add(new AssetFile
                {
                    Id = SinkDocumentFileId,
                    FileName = SinkDocumentFileName
                });
            }
            if (asset.Identifier == "MS-PS-B121-AHU.LR.01" || asset.Identifier == "MS-PS-B121-AHU.LR.02" || asset.Identifier == "MS-PS-B121-AHU.LR.03" || asset.Identifier == "MS-PS-B121-AHU.LR.04")
            {
                assetFiles.Add(new AssetFile
                {
                    Id = TraneSxhkDocumentFileId,
                    FileName = TraneSxhkDocumentFileName
                });
            }
            if (asset.Identifier == "MS-PS-B121-AHU.LR.05" || asset.Identifier == "MS-PS-B121-AHU.LR.06" || asset.Identifier == "MS-PS-B121-AHU.LR.07")
            {
                assetFiles.Add(new AssetFile
                {
                    Id = TraneThcyscDocumentFileId,
                    FileName = TraneThcyscDocumentFileName
                });
            }
        }

        public static Models.FileStreamResult DownloadAssetFile(Guid fileId)
        {
            var fileName = fileId switch
            {
                var id when id == SinkDocumentFileId => SinkDocumentFileName,
                var id when id == FanPoweredBoxDocumentFileId => FanPoweredBoxDocumentFileName,
                var id when id == TraneSxhkDocumentFileId => TraneSxhkDocumentFileName,
                var id when id == TraneThcyscDocumentFileId => TraneThcyscDocumentFileName,
                _ => null
            };

            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }
            var assembly = typeof(TimeZoneService).Assembly;
            var resourceName = assembly.GetManifestResourceNames().Single(s => s.EndsWith(fileName, StringComparison.InvariantCulture));
            var stream = assembly.GetManifestResourceStream(resourceName);
            return new Models.FileStreamResult
            {
                Content = stream,
                ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf"),
                FileName = Uri.EscapeDataString(fileName),
            };
        }
    }


}
