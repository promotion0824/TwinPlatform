using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models.Notification;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Api.DataValidation;
using Willow.Batch;

namespace PlatformPortalXL.Features.Notification;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly ILogger<NotificationTriggersController> _logger;
    private readonly INotificationsService _notificationService;
    public NotificationsController(
        ILogger<NotificationTriggersController> logger,
        INotificationsService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Update the notification state for the current user and a list of notification ids, return the number of updated notifications
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("notifications/state")]
    [Authorize]
    [ProducesResponseType(typeof(NotificationStatesStats), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Update notifications state", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> UpdateNotificationTriggerAsync([FromBody] UpdateNotificationStateRequest request)
    {
        return Ok(await _notificationService.UpdateNotifiactionStateAsync(this.GetCurrentUserId(), request));
    }

    /// <summary>
    /// Get all notifications for the current user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("notifications/all")]
    [Authorize]
    [ProducesResponseType(typeof(BatchDto<NotificationUser>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Get current user notifications", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> GetNotificationsAsync([FromBody] BatchRequestDto request)
    {
        return Ok(await _notificationService.GetNotifications(this.GetCurrentUserId(), request));
    }

    /// <summary>
    /// Get notifications counts by state for the current user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("notifications/states/stats")]
    [Authorize]
    [ProducesResponseType(typeof(List<NotificationStatesStats>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Get current user notification count by state", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> GetNotificationsStatesStatsAsync([FromBody] IEnumerable<FilterSpecificationDto> request)
    {
        return Ok(await _notificationService.GetNotificationsStatesStats(this.GetCurrentUserId(), request));
    }
}
