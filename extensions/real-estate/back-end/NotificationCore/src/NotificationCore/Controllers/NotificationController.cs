using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using NotificationCore.Services;
using NotificationCore.Controllers.Requests;
using Willow.Batch;
using System.Net;
using NotificationCore.Dto;
using System.Collections.Generic;
using NotificationCore.Models;

namespace NotificationCore.Controllers;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Update Notification state and return the number of notifications that have been updated
    /// </summary>
    /// <returns></returns>
    [HttpPut("notifications/state")]
    [Authorize]
    [ProducesResponseType(typeof(NotificationStatesStats), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateNotificationState([FromBody] UpdateNotificationStateRequest request)
    {
        return Ok(await _notificationService.UpdateNotificationState(request));
    }

    /// <summary>
    /// Get Notification List
    /// </summary>
    /// <returns></returns>
    [HttpPost("notifications/all")]
    [Authorize]
    [ProducesResponseType(typeof(BatchDto<NotificationUserDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetNotifications([FromBody] BatchRequestDto request)
    {
        return Ok(await _notificationService.GetNotifications(request));
    }

    /// <summary>
    /// Get the number of notifications per user for each notification state
    /// </summary>
    /// <returns></returns>
    [HttpPost("notifications/states/stats")]
    [Authorize]
    [ProducesResponseType(typeof(List<NotificationStatesStats>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetNotificationsStatesStats([FromBody] IEnumerable<FilterSpecificationDto> request)
    {
        return Ok(await _notificationService.GetNotificationsStatesStats(request));
    }
}
