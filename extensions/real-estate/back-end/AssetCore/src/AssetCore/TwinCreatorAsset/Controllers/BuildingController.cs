using AssetCoreTwinCreator.BusinessLogic.AssetOperations.ReadAssets;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using System;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.MappingId;
using Microsoft.AspNetCore.Authorization;

namespace AssetCoreTwinCreator.Controllers
{
    /// <summary>
    /// Building controller to do CRUD operations on building
    /// </summary>
    [Route("api/sites")]
    public class BuildingController : Controller
    {
        private readonly IReadAssets _readAssetsRepository;
        private readonly IMappingService _mappingService;

        public BuildingController(IReadAssets readAssetsRepository, IMappingService mappingService)
        {
            _readAssetsRepository = readAssetsRepository;
            _mappingService = mappingService;
        }

        /// <summary>
        /// Search Assets
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="searchRequestDto"></param>
        /// <returns></returns>
        [HttpPost("{siteId}/searchassets")]
        [Authorize]
        [SwaggerOperation(OperationId = "Search Assets", Tags = new string[] { "TwinCreator" })]
        public async Task<ActionResult<IEnumerable<AssetDto>>> SearchAssets([FromRoute] Guid siteId, [FromBody] AssetSearchRequestDto searchRequestDto, [FromQuery] bool includeCategory = false)
        {
            var searchRequest = await _mappingService.MapAssetSearchRequestAsync(searchRequestDto);
            var assets = await _readAssetsRepository.SearchAssetsAsync(searchRequest, Guid.Empty, true, includeCategory);
            var dtos = await _mappingService.MapAssetsAsync<AssetDto>(assets);

            return dtos;
        }

    }
}