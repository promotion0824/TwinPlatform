using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using PlatformPortalXL.Services.MarketPlaceApi;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.MarketPlace
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AppCategoriesController : ControllerBase
    {
        private readonly IMarketPlaceApiService _coreApi;

        public AppCategoriesController(IMarketPlaceApiService coreApi)
        {
            _coreApi = coreApi;
        }

        [HttpGet("appCategories")]
        [Authorize]
        [ProducesResponseType(typeof(List<AppCategoryDto>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets app categories", Tags = new [] { "MarketPlace" })]
        public async Task<IActionResult> GetAppCategories()
        {
            var categories = await _coreApi.GetCategories();
            return Ok(AppCategoryDto.MapFrom(categories));
        }
    }
}
