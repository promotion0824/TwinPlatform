using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public class AssetSearchController : Controller
    {
        private readonly IAssetSearch _assetSearch;
        private readonly ICategoryStructure _categoryStructure;
        private readonly IMappingService _mappingService;

        public AssetSearchController(IAssetSearch assetSearch, ICategoryStructure categoryStructure, IMappingService mappingService)
        {
            _assetSearch = assetSearch;
            _categoryStructure = categoryStructure;
            _mappingService = mappingService;
        }

        /// <summary>
        /// Retrieves all the assets and its parameters on the particular category on the building the user has access to
        /// </summary>
        /// <returns></returns>
        [HttpPost("api/sites/{siteId}/categories/{categoryId}/assetssearch")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [ProducesResponseType(typeof(AssetSearchResponse), 200)]
        [SwaggerOperation(OperationId = "search assets", Tags = new string[] { "TwinCreator" })]
        public async Task<IActionResult> Search([FromRoute] Guid siteId, [FromRoute] Guid categoryId, [FromBody, Required]AssetSearchParametersDto searchRequestDto)
        {
            var categoryIdInt = categoryId.ToCategoryId();
            var structureForCategory = await _categoryStructure.GetStructureForCategory(categoryIdInt);

            if (structureForCategory.Any() == false)
            {
                return NotFound();
            }

            var searchRequest = await _mappingService.MapAssetSearchParameters(searchRequestDto);

            var assetSearchResponse = new AssetSearchResponse { CategoryColumns = await _mappingService.MapCategoryColumnsAsync(structureForCategory), Assets = new AssetDto[0] };

            if (searchRequest.LimitResultCount.HasValue == false || searchRequest.LimitResultCount.Value > 0)
            {
                var searchQuery = new AssetSearchQuery
                {
                    CategoryId = categoryIdInt,
                    IncludeAssetDetails = searchRequest.RetrieveAssetParameters,
                    Filters = new AssetSearchQuery.SearchFilters
                    {
                        FilterByFloorCode = searchRequest.FilterByFloorCode,
                        FilterByAssetRegisterIds = searchRequest.FilterByAssetRegisterIds,
                        FilterByKeyword = searchRequest.FilterByKeyword,
                        FilterByValidationStatus = searchRequest.FilterByValidationStatus
                    },
                    Sorting = new AssetSearchQuery.SearchSorting
                    {
                        SortBy = searchRequest.SortBy,
                        SortByAscending = searchRequest.SortByAscending
                    },
                    Pagination = new AssetSearchQuery.SearchPagination
                    {
                        LimitResultCount = searchRequest.LimitResultCount,
                        SkipResultCount = searchRequest.SkipResultCount
                    }
                };

                searchQuery.Instigator = new AssetSearchQuery.SearchInstigator { CanViewAllAssets = true };

                var result = await _assetSearch.Search(searchQuery);

                assetSearchResponse.Assets = await _mappingService.MapAssetsAsync<AssetDto>(result.Assets);
                assetSearchResponse.QueryAssetCount = result.TotalCount;
            }

            assetSearchResponse.SortBy = searchRequest.SortBy;
            assetSearchResponse.SortByAscending = searchRequest.SortByAscending;

            var assetListSerialised = JsonConvert.SerializeObject(assetSearchResponse, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
            return Content(assetListSerialised, "application/json");
        }
    }
}
