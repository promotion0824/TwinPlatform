using System;
using AssetCoreTwinCreator.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Extensions;
using AssetCoreTwinCreator.MappingId.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Infrastructure.Exceptions;

namespace AssetCoreTwinCreator.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMappingService _mappingService;

        public CategoryController(ICategoryRepository categoryRepository, IMappingService mappingService)
        {
            _categoryRepository = categoryRepository;
            _mappingService = mappingService;
        }

        /// <summary>
        /// Retrieve a root categories by building Id
        /// </summary>
        [HttpGet("api/sites/{siteId}/categories/roots")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
        [SwaggerOperation(OperationId = "getRootCategories", Tags = new string[] { "TwinCreator" })]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetRootCategories([FromRoute] Guid siteId, [FromQuery] bool? includeAssetCount, [FromQuery] List<Guid> floorId, [FromQuery] bool includeChildren)
        {
            var buildingId = await _mappingService.GetBuildingIdAsync(siteId);
            if (buildingId < 0)
            {
                throw new ResourceNotFoundException(nameof(SiteMapping), siteId);
            }

            List<string> floorCodes = null;
            if (floorId != null && floorId.Any())
            {
                floorCodes = new List<string>();
                foreach (var guid in floorId)
                {
                    var floorCode = await _mappingService.GetFloorCodeAsync(guid);
                    if (string.IsNullOrEmpty(floorCode))
                    {
                        throw new ResourceNotFoundException(nameof(FloorMapping), guid);
                    }
                    floorCodes.Add(floorCode);
                }
            }

            var categories = await _categoryRepository.GetRootCategoriesByBuildingId(buildingId, includeAssetCount ?? false, floorCodes, includeChildren);

            var dtos = await _mappingService.MapCategoriesAsync(siteId, categories);
            return dtos;
        }

        /// <summary>
        /// Retrieve a Category by Parent Category Id
        /// </summary>
        [HttpGet("api/sites/{siteId}/categories/{categoryId}/children")]
        [Authorize]
        [SwaggerOperation(OperationId = "getCategoryChildren", Tags = new string[] { "TwinCreator" })]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetByParentCategoryId([FromRoute] Guid siteId, [FromRoute] Guid categoryId, [FromQuery] bool? includeAssetCount, [FromQuery] List<Guid> floorId)
        {
            var categoryIdInt = categoryId.ToCategoryId();

            List<string> floorCodes = null;
            if (floorId != null && floorId.Any())
            {
                floorCodes = new List<string>();
                foreach (var guid in floorId)
                {
                    var floorCode = await _mappingService.GetFloorCodeAsync(guid);
                    if (string.IsNullOrEmpty(floorCode))
                    {
                        throw new ResourceNotFoundException(nameof(FloorMapping), guid);
                    }
                    floorCodes.Add(floorCode);
                }
            }

            var categories = await _categoryRepository.GetCategoryChildren(categoryIdInt, includeAssetCount ?? false, floorCodes);

            var dtos = await _mappingService.MapCategoriesAsync(siteId, categories);

            return dtos;
        }

    }
}
