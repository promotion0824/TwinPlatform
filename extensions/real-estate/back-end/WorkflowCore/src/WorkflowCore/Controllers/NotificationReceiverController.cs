using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Dto;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class NotificationReceiverController : ControllerBase
    {
        private readonly INotificationReceiverService _receiversService;

        public NotificationReceiverController(INotificationReceiverService receiversService)
        {
            _receiversService = receiversService;
        }

        [HttpGet("sites/{siteId}/notificationReceivers")]
        [Authorize]
        [ProducesResponseType(typeof(List<ReporterDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReceivers([FromRoute] Guid siteId)
        {
            var receivers  = await _receiversService.GetReceivers(siteId);
            var dtos = NotificationReceiverDto.MapFromModels(receivers);
            return Ok(dtos);
        }
    }
}
