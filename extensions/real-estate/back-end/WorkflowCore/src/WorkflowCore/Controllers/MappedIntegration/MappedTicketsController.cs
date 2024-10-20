using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WorkflowCore.Services.MappedIntegration.Dtos.Requests;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Interfaces;

namespace WorkflowCore.Controllers.MappedIntegration;

/// <summary>
/// API for Mapped Integration
/// </summary>
[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
[Route("api/mapped")]
[Authorize]
public class MappedTicketsController : ControllerBase
{
    private readonly IMappedService _mappedService;

    public MappedTicketsController(IMappedService mappedService)
    {
        _mappedService = mappedService;
    }
    /// <summary>
    /// Upsert Tickets
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("tickets/upsert")]
    [ProducesResponseType(typeof(UpsertTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]

    public async Task<IActionResult> UpsertTicket(MappedTicketUpsertRequest request)
    {
        var result = await _mappedService.TicketUpsert(request);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }

    }

    /// <summary>
    /// Get Tickets by Site Id
    /// Additional filters, such as ExternalId, SourceId, CreatedAfter can be applied
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("sites/{siteId}/tickets")]
    [ProducesResponseType(typeof(GetTicketsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTickets([FromRoute] Guid siteId, [FromQuery] MappedGetTicketsRequest request)
    {
        var result = await _mappedService.GetTicketsAsync(siteId, request);
        return Ok(result);
    }


    /// <summary>
    /// Get Ticket by Ticket Id and Site Id
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="ticketId"></param>
    /// <returns></returns>
    [HttpGet("sites/{siteId}/tickets/{ticketId}")]
    [ProducesResponseType(typeof(GetTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicket([FromRoute] Guid siteId, [FromRoute] Guid ticketId)
    {
        var result = await _mappedService.GetTicketAsync(siteId, ticketId);
        if (result is null)
        {
            return NotFound("Ticket not found");
        }
        return Ok(result);
    } 

    /// <summary>
    ///  Get Categorical Data 
    /// </summary>
    /// <returns></returns>
    [HttpGet("categoricalData")]
    [ProducesResponseType(typeof(TicketCategoricalDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategoricalData()
    {
        var result = await _mappedService.GetCustomerCategoricalData();

        return Ok(result);
    }
}



