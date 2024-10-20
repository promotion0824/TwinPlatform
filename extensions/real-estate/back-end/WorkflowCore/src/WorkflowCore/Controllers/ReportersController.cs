using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ReportersController : ControllerBase
    {
        private readonly IReportersService _reportersService;

        public ReportersController(IReportersService reportersService)
        {
            _reportersService = reportersService;
        }

        [HttpGet("sites/{siteId}/reporters")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReporterDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReporters([FromRoute] Guid siteId)
        {
            var reporters  = await _reportersService.GetReporters(siteId);
            var dtos = ReporterDto.MapFromModels(reporters);
            return Ok(dtos);
        }

        [HttpPost("sites/{siteId}/reporters")]
        [Authorize]
        [ProducesResponseType(typeof(ReporterDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateReporter([FromRoute] Guid siteId, [FromBody] CreateReporterRequest request)
        {
            var reporter  = await _reportersService.CreateReporter(siteId, request);
            var dtos = ReporterDto.MapFromModel(reporter);
            return Ok(dtos);
        }

        [HttpPut("sites/{siteId}/reporters/{reporterId}")]
        [Authorize]
        [ProducesResponseType(typeof(ReporterDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateReporter([FromRoute] Guid siteId, [FromRoute] Guid reporterId, [FromBody] UpdateReporterRequest request)
        {
            var reporter  = await _reportersService.UpdateReporter(siteId, reporterId, request);
            var dtos = ReporterDto.MapFromModel(reporter);
            return Ok(dtos);
        }

        [HttpDelete("sites/{siteId}/reporters/{reporterId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteReporter([FromRoute] Guid siteId, [FromRoute] Guid reporterId)
        {
            await _reportersService.DeleteReporter(siteId, reporterId);
            return NoContent();
        }
    }
}
