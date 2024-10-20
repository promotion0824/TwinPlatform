using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildingConnectorsController : ControllerBase
    {
        private readonly ILogger<BuildingConnectorsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public BuildingConnectorsController(ILogger<BuildingConnectorsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<BuildingConnector>> GetAll()
        {
            return wsupDbContext.BuildingConnectors;
        }

        [HttpGet("GetAllBuildingConnectorsByCustomerInstance/{customerInstanceId}")]
        public ActionResult<IEnumerable<BuildingConnector>> GetAll(Guid customerInstanceId)
        {
            return wsupDbContext.BuildingConnectors.Where(c => c.CustomerInstanceId == customerInstanceId).ToList();
        }

        [HttpGet("{customerInstanceId}/{buildingId}/{connectorId}")]
        public ActionResult<BuildingConnector> Get(Guid customerInstanceId, string buildingId, string connectorId)
        {
            var buildingConnector = wsupDbContext.BuildingConnectors.FirstOrDefault(c => c.CustomerInstanceId == customerInstanceId && c.BuildingId == buildingId && c.ConnectorId == connectorId);

            if (buildingConnector != null)
            {
                return Ok(buildingConnector);
            }

            return NotFound();
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<IActionResult> Create(BuildingConnector buildingConnector)
        {
            await wsupDbContext.BuildingConnectors.AddAsync(buildingConnector);
            await wsupDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<IActionResult> Update(BuildingConnector buildingConnector)
        {
            wsupDbContext.BuildingConnectors.Update(buildingConnector);
            await wsupDbContext.SaveChangesAsync();
            return Ok(buildingConnector);
        }

        [HttpDelete("{customerInstanceId}/{applicationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<IActionResult> Delete(Guid customerInstanceId, string buildingId, string connectorId)
        {
            var buildingConnector = wsupDbContext.BuildingConnectors.FirstOrDefault(a => a.CustomerInstanceId == customerInstanceId && a.BuildingId == buildingId && a.ConnectorId == connectorId);

            if (buildingConnector != null)
            {
                wsupDbContext.BuildingConnectors.Remove(buildingConnector);
                await wsupDbContext.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }
    }
}
