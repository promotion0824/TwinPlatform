using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Api.DataValidation;
using Willow.Batch;

namespace PlatformPortalXL.Features.Notification;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class NotificationTriggersController : ControllerBase
{
    private readonly ILogger<NotificationTriggersController> _logger;
    private readonly INotificationTriggerService _notificationTriggerService;
    public NotificationTriggersController(
        ILogger<NotificationTriggersController> logger,
        INotificationTriggerService notificationTriggerService)
    {
        _logger = logger;
        _notificationTriggerService = notificationTriggerService;
    }

    /// <summary>
    /// Get Notification Trigger
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("notifications/triggers/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(NotificationTriggerDto), StatusCodes.Status200OK)]
    [SwaggerOperation("Get notification trigger", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> GetNotificationTriggerAsync([FromRoute] Guid id)
    {
        return Ok(await _notificationTriggerService.GetAsync(this.GetCurrentUserId(),this.GetUserEmail(), id));
    }

    /// <summary>
    /// Create Notification Trigger
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("notifications/triggers/all")]
    [Authorize]
    [ProducesResponseType(typeof(BatchDto<NotificationTriggerDto>), StatusCodes.Status200OK)]
    [SwaggerOperation("Get notification triggers", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> GetNotificationTriggersAsync([FromBody] BatchRequestDto request)
    {
        var userId = this.GetCurrentUserId();
        return Ok(await _notificationTriggerService.GetAllAsync(this.GetCurrentUserId(), this.GetUserEmail(), request));
    }

    /// <summary>
    /// Create Notification Trigger
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("notifications/triggers")]
    [Authorize]
    [ProducesResponseType(typeof(NotificationTriggerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Create notification trigger", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> CreateNotificationTriggerAsync([FromBody]CreateNotificationTriggerRequest request)
    {
        return Ok(await _notificationTriggerService.CreateAsync(this.GetCurrentUserId(), this.GetUserEmail(), request));
    }

    /// <summary>
    /// Update Notification Trigger
    /// </summary>
    /// <param name="id">the notification trigger id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("notifications/triggers/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Update notification trigger", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> UpdateNotificationTriggerAsync([FromRoute]Guid id,  [FromBody] UpdateNotificationTriggerRequest request)
    {
        await _notificationTriggerService.UpdateAsync(this.GetCurrentUserId(),this.GetUserEmail(), id, request);
        return NoContent();
    }

    /// <summary>
    /// Delete Notification Trigger
    /// </summary>
    /// <param name="id">the notification trigger id</param>
    /// <returns></returns>
    [HttpDelete("notifications/triggers/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerOperation("Delete notification trigger", Tags = new[] { "Notifications" })]
    public async Task<ActionResult> DeleteNotificationTriggerAsync([FromRoute] Guid id)
    {
        await _notificationTriggerService.DeleteAsync(this.GetCurrentUserId(), this.GetUserEmail(), id);
        return NoContent();
    }

    /// <summary>
    /// Batch disable/enable Notification Triggers by source
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("notifications/triggers/toggle")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Batch notification trigger toggle", Tags = new[] { "Notifications" })]
    public async Task<IActionResult> BatchNotificationToggle([FromBody] BatchNotificationTriggerToggleRequest request)
    {
        await _notificationTriggerService.BatchToggleAsync(this.GetCurrentUserId(),this.GetUserEmail(), request);
        return NoContent();
    }
}
