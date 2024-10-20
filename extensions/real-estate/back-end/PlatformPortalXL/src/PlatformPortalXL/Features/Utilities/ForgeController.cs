using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services.Forge;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PlatformPortalXL.Features.Utilities
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ForgeController : Controller
    {
        private readonly ISiteApiService _siteApi;
        private readonly IForgeService _forgeService;
        public readonly ILogger<ForgeController> _logger;

        public ForgeController(ISiteApiService siteApi, IForgeService forgeService, ILogger<ForgeController> logger)
        {
            _siteApi = siteApi;
            _forgeService = forgeService;
            _logger = logger;
        }

        [HttpGet("forge/oauth/token")]
        [Authorize]
        [ProducesResponseType(typeof(AutodeskTokenResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [SwaggerOperation("Get an Autodesk Forge twolegged access token", Tags = new [] { "Utilities" })]
        public async Task<ActionResult<AutodeskTokenResponse>> Get()
        {
            try
            {
                var token = await _forgeService.GetToken();
                var response = new AutodeskTokenResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer"

                };
                return Ok(response);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Failed to get Forge token");
                return BadRequest();
            }
           
           
        }
    }
}
