using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using WorkflowCore.Models;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class DynamicsTicketsController : ControllerBase
    {
        private readonly IImagePathHelper _imagePathHelper;
        private readonly IWorkflowService _workflowCoreService;

        public DynamicsTicketsController(
            IImagePathHelper imagePathHelper,
            IWorkflowService workflowCoreService)
        {
            _imagePathHelper = imagePathHelper;
            _workflowCoreService = workflowCoreService;
        }

        [HttpPut("dynamics/tickets/{sequenceNumber}")]
        [ProducesResponseType(typeof(TicketDetailDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateDynamicTicket([FromRoute] String sequenceNumber, [FromBody] DynamicsUpdateTicketRequest request, string language)
        {
            if (String.IsNullOrEmpty(sequenceNumber))
            {
                throw new ArgumentNullException($"{nameof(sequenceNumber)} is empty");
            }
            var ticketBeforeUpdate = await _workflowCoreService.GetTicketBySequenceNumber(sequenceNumber);
            if (ticketBeforeUpdate == null)
            {
                throw new NotFoundException(new { SequenceNumber = sequenceNumber });
            }

            await _workflowCoreService.UpdateTicket(
                ticketBeforeUpdate.SiteId,
                ticketBeforeUpdate.Id,
                new UpdateTicketRequest
                {
                    Description = request.Description,
                    Priority = int.Parse(request.Priority),
                    Status = (int)Enum.Parse<TicketStatusEnum>(request.TicketStatus.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase)),
                    Summary = request.Summary,
                    ReporterName = request.ReporterName,      
                    ReporterPhone = request.ReporterPhone,
                    ReporterEmail = request.ReporterEmail       
                }, language);
            return NoContent();
        }

    }
}
