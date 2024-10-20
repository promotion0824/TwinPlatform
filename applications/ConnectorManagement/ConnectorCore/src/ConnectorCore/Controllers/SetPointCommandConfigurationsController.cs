namespace ConnectorCore.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Get set point command configurations.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SetPointCommandConfigurationsController : Controller
    {
        private readonly ISetPointCommandConfigurationsService setPointCommandConfigurationsService;

        internal SetPointCommandConfigurationsController(ISetPointCommandConfigurationsService setPointCommandConfigurationsService)
        {
            this.setPointCommandConfigurationsService = setPointCommandConfigurationsService;
        }

        /// <summary>
        /// List all set point command configurations.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<SetPointCommandConfigurationEntity>>> GetList()
        {
            var result = await setPointCommandConfigurationsService.GetListAsync();
            return Ok(result);
        }
    }
}
