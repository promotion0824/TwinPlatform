using AssetCore.Extensions;
using AssetCore.Infrastructure.Exceptions;
using AssetCore.TwinCreatorAsset.Dto;
using AssetCoreTwinCreator.BusinessLogic;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.Features.Asset.Attachments;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Extensions;
using AssetCoreTwinCreator.MappingId.Models;
using AssetCoreTwinCreator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;

namespace AssetCoreTwinCreator.Controllers
{
    [Route("api/sites/{siteId}/categories/{categoryId}/assets")]
    public class AssetController : Controller
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMappingService _mappingService;
        private readonly IAttachmentsService _attachmentsService;
        private readonly ICategoryRepository _categoryRepository;

        public AssetController(
            IAssetRepository assetRepository,
            IMappingService mappingService,
            IAttachmentsService attachmentsService,
            ICategoryRepository categoryRepository)
        {
            _assetRepository = assetRepository;
            _mappingService = mappingService;
            _attachmentsService = attachmentsService;
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Retrieves an asset and its parameters
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [HttpGet("/api/sites/{siteId}/assets/{assetId}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore  = true)]
        [SwaggerOperation(OperationId = "getAsset", Tags = new string[] { "TwinCreator" })]
        [Authorize]
        public async Task<ActionResult<AssetDto>> Get([FromRoute] Guid siteId, [FromRoute] Guid assetId)
        {
            var buildingId = await _mappingService.GetBuildingIdAsync(siteId);
            int assetIdInt;

            if (buildingId < 0)
            {
                throw new ResourceNotFoundException(nameof(SiteMapping), siteId);
            }
            try
            {
                assetIdInt = assetId.ToAssetId();
            }
            catch(BadRequestException)
            {
                assetIdInt = await _mappingService.GetAssetIdByEquipmentId(assetId);
            }
            Asset asset;
            asset = await _assetRepository.Get(assetIdInt);

            if (asset == null)
            {
                throw new ResourceNotFoundException(nameof(Asset), assetId);
            }

            var dto = await _mappingService.MapAssetAsync<AssetDto>(asset);

            var categories = await _categoryRepository.GetRootCategoriesByBuildingId(buildingId, false, null, true);
            dto.ModuleTypeNamePath = await _mappingService.GetCategoryNearestModuleType(siteId, categories, dto.CategoryId);

            return dto;
        }

        /// <summary>
        /// Retrieves an asset and its parameters by equipment id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="equipmentId"></param>
        /// <returns></returns>
        [HttpGet("/api/sites/{siteId}/assets/byequipment/{equipmentId}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore  = true)]
        [SwaggerOperation(OperationId = "getAssetByEquipment", Tags = new string[] { "TwinCreator" })]
        [Authorize]
        public async Task<ActionResult<AssetDto>> GetByEquipmentId([FromRoute] Guid siteId, [FromRoute] Guid equipmentId)
        {
            var buildingId = await _mappingService.GetBuildingIdAsync(siteId);
            var assetId = await _mappingService.GetAssetIdByEquipmentId(equipmentId);

            if (buildingId < 0)
            {
                throw new ResourceNotFoundException(nameof(SiteMapping), siteId);
            }

            if (assetId < 0)
            {
                throw new ResourceNotFoundException(nameof(AssetEquipmentMapping), equipmentId);
            }

            var asset = await _assetRepository.Get(assetId);            
            var dto = await _mappingService.MapAssetAsync<AssetDto>(asset);

            return dto;
        }

        /// <summary>
        /// Retrieves an asset and its parameters by forge viewer model id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="forgeViewerModelId"></param>
        /// <returns></returns>
        [HttpGet("/api/sites/{siteId}/assets/byforgeviewermodelid/{forgeViewerModelId}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssetByForgeViewerModelId", Tags = new string[] { "TwinCreator" })]
        [Authorize]
        public async Task<ActionResult<AssetDto>> GetByForgeViewerModelId([FromRoute] Guid siteId, [FromRoute] Guid forgeViewerModelId)
        {
            var buildingId = await _mappingService.GetBuildingIdAsync(siteId);

            if (buildingId < 0)
            {
                throw new ResourceNotFoundException(nameof(SiteMapping), siteId);
            }

            var asset = await _assetRepository.GetByForgeViewerModelId(buildingId, forgeViewerModelId.ToString(), true);
            if (asset == null)
            {
                throw new ResourceNotFoundException(nameof(Asset), forgeViewerModelId.ToString(), "Could not find an asset with the provided forgeViewerModelId");
            }

            var dto = await _mappingService.MapAssetAsync<AssetDto>(asset);
            var categories = await _categoryRepository.GetRootCategoriesByBuildingId(buildingId, false, null, true);
            dto.ModuleTypeNamePath = await _mappingService.GetCategoryNearestModuleType(siteId, categories, dto.CategoryId);

            return dto;
        }

        /// <summary>
        /// Retrieves list of assets by parameters
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="categoryId"></param>
        /// <param name="floorId"></param>
        /// <param name="searchKeyword"></param>
        /// <returns></returns>
        [HttpGet("/api/sites/{siteId}/assets")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore  = true)]
        [SwaggerOperation(OperationId = "getAssets", Tags = new string[] { "TwinCreator" })]
        [Authorize]
        public async Task<IActionResult> Get([FromRoute] Guid siteId, [FromQuery] Guid? categoryId, [FromQuery] Guid? floorId, [FromQuery] string searchKeyword, [FromQuery] bool? includeProperties)
        {
            var buildingId = await _mappingService.GetBuildingIdAsync(siteId);
            if (buildingId < 0)
            {
                throw new ResourceNotFoundException(nameof(SiteMapping), siteId);
            }

            var categoryIdInt = categoryId?.ToCategoryId();
            var floorCode = floorId.HasValue ? await _mappingService.GetFloorCodeAsync(floorId.Value) : null;
            if (floorId.HasValue && string.IsNullOrEmpty(floorCode))
            {
                throw new ResourceNotFoundException(nameof(FloorMapping), floorId.Value);
            }

            try
            {
                if (includeProperties ?? false)
                {
                    var assets = await _assetRepository.GetAssetsAsync(buildingId, categoryIdInt.HasValue ? new int[] { categoryIdInt.Value } : null, floorCode, searchKeyword, true);
                    var dtos = await _mappingService.MapAssetsAsync<AssetDto>(assets);

                    return Ok(dtos);
                }
                else
                {
                    var assets = await _assetRepository.GetAssetsAsync(buildingId, categoryIdInt.HasValue ? new int[] { categoryIdInt .Value} : null, floorCode, searchKeyword);
                    var dtos = await _mappingService.MapAssetsAsync<AssetSimpleDto>(assets);

                    return Ok(dtos);
                }
            }
            catch (InvalidDataException invalidDataException)
            {
                throw new PreconditionFailedException(invalidDataException.Message, invalidDataException);
            }
        }

        /// <summary>
        /// Retrieves list of assets by parameters
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="categoryId"></param>
        /// <param name="floorId"></param>
        /// <param name="searchKeyword"></param>
        /// <returns></returns>
        [HttpGet]
        [Obsolete("Use new endpoint 'GET sites/{siteId}/assets' instead")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getAssets", Tags = new string[] { "TwinCreator" })]
        [Authorize]
        public async Task<IActionResult> GetByCategory([FromRoute] Guid siteId, [FromRoute] Guid categoryId, [FromQuery] Guid? floorId, [FromQuery] string searchKeyword, [FromQuery] bool? includeProperties)
        {
            return await Get(siteId, categoryId, floorId, searchKeyword, includeProperties);
        }


        [HttpGet("/api/sites/{siteId}/assets/{assetId}/changehistory")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore  = true)]
        [SwaggerOperation(OperationId = "getChangeHistory", Tags = new string[] { "TwinCreator" })]
        [Authorize]
        public async Task<ActionResult<AssetHistoryFilesDto>> GetChangeHistoryAndFiles([FromRoute] Guid siteId, [FromRoute] string assetId)
        {
            int assetIdInt;

            if (Guid.TryParse(assetId, out var assetGuid))
            {
                assetIdInt = assetGuid.ToAssetId();
            }
            else
            {
                var buildingId = await _mappingService.GetBuildingIdAsync(siteId);
                var asset = await _assetRepository.GetByAssetIdentifier(buildingId, assetId, false);
                if (asset == null)
                {
                    throw new ResourceNotFoundException(nameof(Asset), assetId);
                }
                assetIdInt = asset.Id;
            }

            var changeHistory = await _assetRepository.GetChangeHistory(assetIdInt);
            var files = await _attachmentsService.GetFiles(assetRegisterIds: new List<int> {assetIdInt});

            var dto = await _mappingService.MapAssetHistoryFiles(assetIdInt, changeHistory, files);

            return dto;
        }

        [HttpGet("/api/sites/{siteId}/assetTree")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore  = true)]
        [SwaggerOperation(OperationId = "getAssetTree", Tags = new string[] { "TwinCreator" })]
        [Authorize]
        public async Task<ActionResult<List<AssetTreeCategoryDto>>> GetAssetTree([FromRoute] Guid siteId)
        {
            var buildingId = await _mappingService.GetBuildingIdAsync(siteId);
            if (buildingId < 0)
            {
                throw new ResourceNotFoundException(nameof(SiteMapping), siteId);
            }

            var categories = await _categoryRepository.GetRootCategoriesByBuildingId(buildingId, false, null, true);

            var baseBuildingCategory = categories.FirstOrDefault(c => c.Name == "Base Building");
            var assetRegisterCategory =  baseBuildingCategory?.ChildCategories.FirstOrDefault(c => c.Name == "Asset Register") ?? categories.FirstOrDefault(c => c.Name == "Asset Register");
            if (assetRegisterCategory == null)
            {
                return new List<AssetTreeCategoryDto>();
            }

            var rootCategoryDtos = await _mappingService.MapCategoriesAsync(siteId, assetRegisterCategory.ChildCategories);

            var categoryIds = assetRegisterCategory.ChildCategories.Flatten(c => c.ChildCategories).Select(c => c.Id).Distinct().ToArray();

            var assets = await _assetRepository.GetBuildingAssets(buildingId, categoryIds);
            var assetDtos = await _mappingService.MapAssetsAsync<AssetSimpleDto>(assets);

            var assetEquipmentMappings = (await _mappingService.GetAssetEquipmentMappingByAssetIds(assets.Select(a => a.Id).ToArray())).ToDictionary(m => m.AssetRegisterId, m=> m.EquipmentId);

            var assetTreeCategories = AssetTreeCategoryDto.Map(rootCategoryDtos, assetDtos, assetEquipmentMappings);

            return assetTreeCategories;
        }

		/// <summary>
		/// Gets asset names mapped to equipment ids that should be filtered based of sites ids
		/// </summary>
		/// <param name="equipmentIds">An array of equipments ids</param>
		/// <returns>list of object contains the equipment id and the asset name mapped to it</returns>
		[HttpPost("/api/sites/assets/names")]
		[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
		[ProducesResponseType(typeof(List<EquipmentNameMappingDto>), StatusCodes.Status200OK)]
		[SwaggerOperation(" Gets asset names mapped to equipment ids that should be filtered based of sites ids", OperationId = "GetAssetsNamesbyEquipmentIds", Tags = new string[] { "TwinCreator" })]
		[Authorize]
		public async Task<ActionResult<List<EquipmentNameMappingDto>>> GetAssetsNamesbyEquipmentIds([FromBody] IEnumerable<Guid> equipmentIds)
		{
			var mappingNames = new List<EquipmentNameMappingDto>();
			if (!equipmentIds?.Any() ?? true)
			{
				return Ok(mappingNames);
			}

			equipmentIds = equipmentIds.Distinct().ToList();
			var assetEquipmentMappingData = await _mappingService.MapAssetsIdsAsync(equipmentIds);
			var assetNames = await _assetRepository.GetAssetsNamesAsync(assetEquipmentMappingData.Select(x => x.AssetRegisterId));
			var assetEquipmentDict = assetEquipmentMappingData.ToDictionary(x => x.AssetRegisterId, x => x.EquipmentId);
			foreach (var assetName in assetNames)
			{
				if (assetEquipmentDict.TryGetValue(assetName.AssetRegisterId, out var equipmentId))
				{
					mappingNames.Add(new EquipmentNameMappingDto { EquipmentId = equipmentId, Name = assetName.Name });
				}

			}
			return Ok(mappingNames);

		}
	}
}