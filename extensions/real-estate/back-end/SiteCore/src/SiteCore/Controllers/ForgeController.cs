using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SiteCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ForgeController : Controller
    {
        private readonly IAutodeskForgeTokenProvider _tokenProvider;

        public ForgeController(IAutodeskForgeTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        [HttpGet("forge/oauth/token")]
        [Authorize]
        [ProducesResponseType(typeof(AutodeskTokenResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [SwaggerOperation("Get an Autodesk Forge twolegged access token")]
        public async Task<ActionResult<AutodeskTokenResponse>> Get()
        {
            return await _tokenProvider.GetTokenAsync();
        }
    }
}
