namespace ConnectorCore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Entities.Validators;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Willow.Infrastructure.Exceptions;

    /// <summary>
    /// Manage set points.
    /// </summary>
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class SetPointCommandsController : Controller
    {
        private readonly ISetPointCommandsService setPointCommandsService;

        internal SetPointCommandsController(ISetPointCommandsService setPointCommandsService)
        {
            this.setPointCommandsService = setPointCommandsService;
        }

        /// <summary>
        /// List all set point commands for site, optionally filtered by equipment id.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="equipmentId">Optional equipment id to filter by.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<SetPointCommandEntity>>> GetList([FromRoute] Guid siteId, [FromQuery] Guid? equipmentId)
        {
            var result = await setPointCommandsService.GetListBySiteIdAsync(siteId, equipmentId);
            return Ok(result);
        }

        /// <summary>
        /// List all active set point commands for a connector.
        /// </summary>
        /// <param name="connectorId">Id of the connector.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/connectors/{connectorId}/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<SetPointCommandEntity>>> GetListByConnector([FromRoute] Guid connectorId)
        {
            var result = await setPointCommandsService.GetListByConnectorIdAsync(connectorId);
            return Ok(result);
        }

        /// <summary>
        /// Get a single set point command by id.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="id">Id of the set point command.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<SetPointCommandEntity>> GetItem([FromRoute] Guid siteId, [FromRoute] Guid id)
        {
            var result = await setPointCommandsService.GetItemAsync(id);
            if (result.SiteId != siteId)
            {
                throw new ResourceNotFoundException(nameof(SetPointCommandEntity), id);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets or sets the set point command entity.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="entity">The set point command entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="201">Successful operation.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SetPointCommandEntity>> Post([FromRoute] Guid siteId, [FromBody] SetPointCommandEntity entity)
        {
            if (entity.SiteId != siteId)
            {
                throw new BadRequestException("siteId in request URI must match SiteId in body");
            }

            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            ValidateEntity(entity);

            var result = await setPointCommandsService.InsertAsync(entity);
            return CreatedAtAction(nameof(GetItem), new { siteId = result.SiteId, id = result.Id }, result);
        }

        /// <summary>
        /// Gets or sets the updated set point command entity.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="setPointCommandId">Id of the set point command.</param>
        /// <param name="entity">The set point command entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{setPointCommandId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SetPointCommandEntity>> Put([FromRoute] Guid siteId, [FromRoute] Guid setPointCommandId, [FromBody] SetPointCommandEntity entity)
        {
            if (entity.SiteId != siteId)
            {
                throw new BadRequestException("siteId in request URI must match siteId in body");
            }

            if (entity.Id != setPointCommandId)
            {
                throw new BadRequestException("setPointCommandId in request URI must match Id in body");
            }

            ValidateEntity(entity);

            var result = await setPointCommandsService.UpdateAsync(entity);
            return Ok(result);
        }

        /// <summary>
        /// Gets or sets the deletion of a set point command entity.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="setPointCommandId">Id of the set point command.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="204">No content.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{setPointCommandId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete([FromRoute] Guid siteId, [FromRoute] Guid setPointCommandId)
        {
            await setPointCommandsService.DeleteAsync(siteId, setPointCommandId);
            return NoContent();
        }

        private static void ValidateEntity(SetPointCommandEntity entity)
        {
            var validator = new SetPointCommandValidator();
            var result = validator.Validate(entity);

            if (!result.IsValid)
            {
                throw new BadRequestException(result.ToString());
            }
        }
    }
}
