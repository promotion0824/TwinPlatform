using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorsController : ControllerBase
    {
        private readonly ILogger<ConnectorsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public ConnectorsController(ILogger<ConnectorsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet]
        public IEnumerable<Connector> GetAll()
        {
            return wsupDbContext.Connectors;
        }

        [HttpGet("GetAllByCustomerInstance/{customerInstanceId}")]
        public IEnumerable<Connector> GetAllByCustomerInstance(Guid customerInstanceId)
        {
            return wsupDbContext.Connectors.Where(c => c.CustomerInstanceId == customerInstanceId);
        }

        [HttpGet("{connectorId}")]
        public Connector? Get(string connectorId)
        {
            return wsupDbContext.Connectors.FirstOrDefault(s => s.Id == connectorId);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Connector> Create(Connector connector)
        {
            await wsupDbContext.Connectors.AddAsync(connector);
            await wsupDbContext.SaveChangesAsync();
            return connector;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Connector> Update(Connector connector)
        {
            wsupDbContext.Connectors.Update(connector);
            await wsupDbContext.SaveChangesAsync();
            return connector;
        }

        [HttpDelete("{connectorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(string connectorId)
        {
            var connector = wsupDbContext.Connectors.FirstOrDefault(s => s.Id == connectorId);

            if (connector != null)
            {
                wsupDbContext.Connectors.Remove(connector);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
