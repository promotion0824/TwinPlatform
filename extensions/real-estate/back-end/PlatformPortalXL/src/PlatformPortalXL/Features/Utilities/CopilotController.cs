using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Services.Copilot;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.Utilities
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CopilotController : ControllerBase
    {
        private readonly ICopilotService _copilotService;

        public CopilotController(ICopilotService copilotService)
        {
            _copilotService = copilotService;
        }

        [HttpPost("chat")]
        [Authorize]
        [ProducesResponseType(typeof(CopilotChatResponse), StatusCodes.Status200OK)]
        [SwaggerOperation("Copilot chat", Tags = ["Utilities"])]
        public async Task<ActionResult> Chat([FromBody] CopilotChatRequest request)
        {
            var response = await _copilotService.ChatAsync(request);

            return Ok(response);
        }

        [HttpPost("docs")]
        [Authorize]
        [ProducesResponseType(typeof(List<CopilotDocInfoResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation("Copilot documentation on citations and other blob files", Tags = ["Utilities"])]
        public async Task<ActionResult> GetDocInfo([FromBody] CopilotDocInfoRequest request)
        {
            var response = await _copilotService.GetDocInfoAsync(request);

            return Ok(response);
        }
    }
}
