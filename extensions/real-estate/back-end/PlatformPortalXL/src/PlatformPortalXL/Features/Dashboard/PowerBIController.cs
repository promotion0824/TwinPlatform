using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Services.PowerBI;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace PlatformPortalXL.Features.Dashboard
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class PowerBIController : ControllerBase
    {
        private readonly IPowerBIService _powerBIService;

        public PowerBIController(IPowerBIService powerBIService)
        {
            _powerBIService = powerBIService;
        }

        [Authorize]
        [HttpGet("powerbi/groups/{groupId}/reports/{reportId}/token")]
        [SwaggerOperation("Get a PowerBI view report token", Tags = new [] { "Dashboard" })]
        public async Task<ActionResult<PowerBIReportTokenDto>> GetReportToken([FromRoute] Guid groupId, [FromRoute] Guid reportId)
        {
            var reportToken = await _powerBIService.ViewReport(groupId, reportId);
            return PowerBIReportTokenDto.MapFrom(reportToken);
        }
    }
}
