using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.Adx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinCore.Controllers.Admin
{
	[Route("admin/sites/{siteId}/[controller]")]
	[ApiController]
	public class ExportController : ControllerBase
	{
		private readonly IAdxHelper _adxHelper;
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceProvider;

        public ExportController(IAdxHelper adxHelper, IDigitalTwinServiceProvider digitalTwinServiceProvider)
        {
            _adxHelper = adxHelper;
            _digitalTwinServiceProvider = digitalTwinServiceProvider;
        }

        [HttpPost("initializeadx")]
        [Authorize]
        public async Task<ActionResult> InitializeADX([FromRoute] Guid siteId)
        {
	        var service = await _digitalTwinServiceProvider.GetForSiteAsync(siteId);
	        await _adxHelper.SetupADX(service.SiteAdtSettings.AdxDatabase);

	        return NoContent();
        }

		[HttpPost]
		[Authorize]
		public async Task<ActionResult<Export>> TriggerExport([FromRoute] Guid siteId, [FromBody] SourceInfo exportSource)
		{
			if (exportSource != null && !exportSource.IsValid)
			{
				return BadRequest("Please provide valid source information.");
			}

			var nullOrValidSource = exportSource == null || exportSource.IsEmpty ? null : exportSource;

            var service = await _digitalTwinServiceProvider.GetForSiteAsync(siteId);

			var export = _adxHelper.QueueExport(siteId, service, nullOrValidSource);

			return Created($"status/{export.Id}", export);
		}

		[HttpGet("status")]
		[Authorize]
		public ActionResult<IEnumerable<Export>> GetExports([FromRoute] Guid siteId)
		{
			return Ok(_adxHelper.GetExports(siteId));
		}

		[HttpGet("status/{id}")]
		[Authorize]
		public ActionResult<Export> CheckStatus([FromRoute] Guid siteId, [FromRoute] Guid id)
		{
			var export = _adxHelper.GetExports(siteId).SingleOrDefault(x => x.Id == id);

			if (export == null)
				return NotFound();

			return Ok(export);
		}

		[HttpPut("status/{id}/cancel")]
		[Authorize]
		public ActionResult<Export> CancelQueued([FromRoute] Guid siteId, [FromRoute] Guid id)
		{
			var export = _adxHelper.GetExports(siteId).SingleOrDefault(x => x.Id == id);

			if (export == null)
				return NotFound();

			if (export.Status != ExportStatus.Queued)
				return BadRequest("Only exports in queued status can be cancelled.");

			export.Status = ExportStatus.Canceled;

			return Ok(export);
		}
	}
}
