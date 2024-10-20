using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkflowCore.Dto;
using WorkflowCore.Services;
using WorkflowCore.Controllers.Request;


namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InspectionGenerationController : ControllerBase
    {
        private readonly IInspectionRecordGenerator _generator;
        private readonly ILogger _logger;

        public InspectionGenerationController(IInspectionRecordGenerator generator, ILogger<InspectionGenerationController> logger)
        {
            _generator = generator ??  throw new ArgumentNullException(nameof(generator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("inspectionRecords/generate")]
        [Authorize]
        [ProducesResponseType(typeof(GenerationResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GenerateInspectionRecords()
        {
            var result = await _generator.Generate();
            return Ok(result);
        }

        [HttpGet("scheduledinspections/site/{siteId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<GenerateInspectionDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetScheduledInspectionsForSite(Guid siteId)
        {
            var result = await _generator.GetScheduledInspectionsForSite(siteId, DateTime.UtcNow);

            return Ok(result);
        }

        [HttpPost("scheduledinspection/generate")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionRecordDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GenerateInspectionRecordForInspection([FromBody] GenerateInspectionRecordRequest request)
        {
            var result = await _generator.GenerateInspectionRecordForInspection(request);

            _logger.LogInformation("GenerateInspectionRecordForInspection", request);

            return Ok(result);
        }

        [HttpPost("scheduledinspection/generate/check")]
        [Authorize]
        [ProducesResponseType(typeof(GenerateCheckRecordDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GenerateCheckRecord([FromBody] GenerateCheckRecordRequest request)
        {
            var result = await _generator.GenerateCheckRecord(request);

            _logger.LogInformation("GenerateCheckRecordRequest", request);

            return Ok(result);
        }
    }
}
