using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Http;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    #if !DEBUG
    [Authorize]
    #endif
    public class InspectionReportController : TranslationController
    {
        private readonly IInspectionReportService _reportService;
        public InspectionReportController(IInspectionReportService reportService, IHttpRequestHeaders headers) : base(headers)
        {
            _reportService = reportService;
        }

        [HttpPost("inspections/reports")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [Obsolete("Remove when Workflow Services online")]
        public async Task<IActionResult> SendInspectionDailyReport()
        {
            await _reportService.SendInspectionDailyReport();
            return NoContent();
        }

        [HttpPost("inspections/reports/site/{siteId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SendSiteInspectionDailyReport(Guid siteId, [FromQuery] DateTime? utcNow = null)
        {
            await _reportService.SendReportForSite(siteId, utcNow);
            return NoContent();
        }

        [HttpGet("inspection/report/site/{siteId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteInspectionDailyReport(Guid siteId, [FromQuery] DateTime? utcNow = null)
        {
            var result = await _reportService.GetReportForSite(siteId, utcNow);

            return Ok(result);
        }
    }
}
