using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Common;
using Willow.Scheduler;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Http;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using Willow.ExceptionHandling.Exceptions;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    [Authorize]
    public class TicketTemplateController : TranslationController
    {
        private readonly IImagePathHelper _imagePathHelper;
        private readonly IReportersService _reportersService;
        private readonly ITicketTemplateService _service;
        private readonly ISchedulerService _scheduleSvc;
        private readonly IDateTimeService _dateTimeService;

        public TicketTemplateController(IImagePathHelper imagePathHelper,
                                        ITicketTemplateService ticketTemplateSvc,
                                        ISchedulerService scheduleSvc,
                                        IReportersService reportersService,
                                        IDateTimeService dateTimeService,
                                        IHttpRequestHeaders headers)
            : base(headers)
        {
            _imagePathHelper  = imagePathHelper    ?? throw new ArgumentNullException(nameof(imagePathHelper));
            _service          = ticketTemplateSvc  ?? throw new ArgumentNullException(nameof(ticketTemplateSvc));
            _scheduleSvc      = scheduleSvc        ?? throw new ArgumentNullException(nameof(scheduleSvc));
            _reportersService = reportersService   ?? throw new ArgumentNullException(nameof(reportersService));
            _dateTimeService  = dateTimeService    ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        [HttpGet("sites/{siteId}/tickettemplate")]
        [ProducesResponseType(typeof(List<TicketTemplateDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteTicketTemplates([FromRoute] Guid siteId, [FromQuery] bool? archived)
        {
            var templates = (await _service.GetTicketTemplates(siteId, archived)).ToList();
            var dtos = templates.Select( t=> TicketTemplateDto.MapFromModel(t, _imagePathHelper, _dateTimeService) );
            return Ok(dtos);
        }

        [HttpGet("sites/{siteId}/tickettemplate/{templateId}")]
        [ProducesResponseType(typeof(TicketTemplateDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicketTemplate([FromRoute] Guid siteId, [FromRoute] Guid templateId)
        {
            var template = await _service.GetTicketTemplate(templateId);
            if (template == null)
            {
                throw new NotFoundException(new { TicketTemplateId = templateId });
            }
            var dto = TicketTemplateDto.MapFromModel(template, _imagePathHelper, _dateTimeService);
            return Ok(dto);
        }        
        
        [HttpPost("sites/{siteId}/tickettemplate")]
        [ProducesResponseType(typeof(TicketTemplateDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateTicketTemplate([FromRoute] Guid siteId, [FromBody]CreateTicketTemplateRequest request)
        {
            if (string.IsNullOrEmpty(request.SequenceNumberPrefix))
            {
                throw new ArgumentNullException($"{nameof(CreateTicketTemplateRequest.SequenceNumberPrefix)} should be provided").WithData(new { SiteId = siteId });
            }

            if (!request.ReporterId.HasValue)
            {
                var createReportRequest = new CreateReporterRequest
                {
                    CustomerId = request.CustomerId,
                    Name = request.ReporterName,
                    Phone = request.ReporterPhone,
                    Email = request.ReporterEmail,
                    Company = request.ReporterCompany
                };
                var reporter = await _reportersService.CreateReporter(siteId, createReportRequest);
                request.ReporterId = reporter.Id;
            }

            var template = await _service.CreateTicketTemplate(siteId, request, Language);
            var templateDto = TicketTemplateDto.MapFromModel(template, _imagePathHelper, _dateTimeService);

            return Ok(templateDto);
        }

        [HttpPut("sites/{siteId}/tickettemplate/{templateId}")]
        [ProducesResponseType(typeof(TicketTemplateDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateTicketTemplate([FromRoute] Guid siteId, [FromRoute] Guid templateId, [FromBody] UpdateTicketTemplateRequest request)
        {
            try
            {
                if (!request.ReporterId.HasValue && !string.IsNullOrEmpty(request.ReporterName))
                {
                    var createReportRequest = new CreateReporterRequest
                    {
                        CustomerId = request.CustomerId,
                        Name = request.ReporterName,
                        Phone = request.ReporterPhone,
                        Email = request.ReporterEmail,
                        Company = request.ReporterCompany
                    };
                    var reporter = await _reportersService.CreateReporter(siteId, createReportRequest);
                    request.ReporterId = reporter.Id;
                }

                var result = await _service.UpdateTicketTemplate(templateId, request, Language);
                var dto = TicketTemplateDto.MapFromModel(result, _imagePathHelper, _dateTimeService);

                return Ok(dto);
            }
            catch(NotFoundException)
            {
                throw new NotFoundException(new { TicketTemplateId = templateId });
            }
        }

        [HttpGet("sites/{siteId}/tickettemplate/schedules")]
        [ProducesResponseType(typeof(List<ScheduleHit>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicketTemplateSchedules([FromRoute] Guid siteId, [FromQuery] Guid correlationId)
        {
            var templates = await _service.GetTicketTemplates(siteId, false);
            var schedules = await _scheduleSvc.GetSchedulesByOwnerId(_dateTimeService.UtcNow, templates.Select( t=> t.Id ).ToList());

            return Ok(schedules);
        }


        /// <summary>
        /// Returns a list of expected scheduled ticket template data for a schedule hit
        /// </summary>
        /// <param name="scheduleHit"></param>
        /// <returns></returns>
        [HttpPost("tickettemplate/schedule/twins")]
        [ProducesResponseType(typeof(List<ScheduledTicketTwin>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetScheduledTwins([FromBody] ScheduleHit scheduleHit)
        {
            var twins = await _service.GetScheduledTwins(scheduleHit);

            return Ok(twins);
        }

        [Obsolete("Instead use \"tickettemplate/schedule/twins\"")]
        [HttpPost("tickettemplate/schedule/assets")]
        [ProducesResponseType(typeof(List<ScheduledTicketAsset>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetScheduledAssets([FromBody] ScheduleHit scheduleHit)
        {
            var assets = await _service.GetScheduledAssets(scheduleHit);

            return Ok(assets);
        }

        /// <summary>
        /// Given a scheduled ticket template data, creates its corresponding tickts
        /// </summary>
        /// <param name="scheduleHit"></param>
        /// <returns></returns>
        [HttpPost("tickettemplate/schedule/twin")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateScheduledTicketForTwin([FromBody] ScheduledTicketTwin twin)
        {
            await _service.CreateScheduledTicketForTwin(twin);

            return NoContent();
        }

        [Obsolete("Instead use \"tickettemplate/schedule/twin\"")]
        [HttpPost("tickettemplate/schedule/asset")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateScheduledTicketForAsset([FromBody] ScheduledTicketAsset asset)
        {
            await _service.CreateScheduledTicketForAsset(asset);

            return NoContent();
        }
    }
}
