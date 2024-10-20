using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using NotificationCore.Services;
using NotificationCore.Controllers.Requests;
using NotificationCore.Dto;
using Willow.Batch;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace NotificationCore.Controllers;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class NotificationTriggerController : ControllerBase
{
    private readonly INotificationTriggerService _notificationTriggerService;

    public NotificationTriggerController(INotificationTriggerService notificationTriggerService)
    {
        _notificationTriggerService = notificationTriggerService;
    }

    /// <summary>
    /// Get Notification Trigger
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("notifications/triggers/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(NotificationTriggerDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetNotificationTrigger([FromRoute] Guid id)
    {
        return Ok(await _notificationTriggerService.GetNotificationTriggerAsync(id));
    }

    /// <summary>
    /// Create Notification Trigger
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("notifications/triggers")]
    [Authorize]
    [ProducesResponseType(typeof(NotificationTriggerDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateNotificationTrigger([FromBody] CreateNotificationTriggerRequest request)
    {
        return Ok(await _notificationTriggerService.CreateNotificationTriggerAsync(request));
    }

    /// <summary>
    /// Update Notification Trigger
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id"> the notification trigger Id</param>
    /// <returns></returns>
    [HttpPatch("notifications/triggers/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(NotificationTriggerDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateNotificationTrigger([FromRoute] Guid id,[FromBody] UpdateNotificationTriggerRequest request)
    {
        return Ok(await _notificationTriggerService.UpdateNotificationTriggerAsync(id, request));
    }

    /// <summary>
    /// Get Notification Triggers
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("notifications/triggers/all")]
    [Authorize]
    [ProducesResponseType(typeof(BatchDto<NotificationTriggerDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetNotificationTriggers([FromBody] BatchRequestDto request)
    {
        return Ok(await _notificationTriggerService.GetNotificationTriggers(request));
    }
    
    /// <summary>
    /// Delete Notification Trigger
    /// </summary>
    /// <param name="id">the notification trigger id</param>
    /// <returns></returns>
    [HttpDelete("notifications/triggers/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteNotificationTrigger([FromRoute] Guid id)
    {
        await _notificationTriggerService.DeleteNotificationTriggerAsync(id);
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
    public async Task<IActionResult> BatchNotificationToggle([FromBody] BatchNotificationTriggerToggleRequest request)
    {
        await _notificationTriggerService.BatchToggleNotificationTriggerAsync( request);
        return NoContent();
    }
}
