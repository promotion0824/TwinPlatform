using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Common;
using Willow.Scheduler;
using WorkflowCore.Http;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ScheduleController : TranslationController
    {
        private readonly ISchedulerService _svc;
        private readonly IDateTimeService _dtService;

        public ScheduleController(ISchedulerService svc, IDateTimeService dtService, IHttpRequestHeaders headers) : base(headers)
        {
            _svc = svc;
            _dtService = dtService;
        }

        [HttpPost("schedules/check")]
        [Authorize]
        public async Task<IActionResult> CheckSchedules()
        {
            await _svc.CheckSchedules(_dtService.UtcNow, Language);

            return Ok();
        }
    }
}
