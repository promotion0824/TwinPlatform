using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorTypesController : ControllerBase
    {
        private readonly ILogger<ConnectorTypesController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public ConnectorTypesController(ILogger<ConnectorTypesController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet]
        public IEnumerable<ConnectorType> GetAll()
        {
            return wsupDbContext.ConnectorTypes;
        }

        [HttpGet("{connectorTypeId}")]
        public ConnectorType? Get(string connectorTypeId)
        {
            return wsupDbContext.ConnectorTypes.FirstOrDefault(s => s.Id == connectorTypeId);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<ConnectorType> Create(ConnectorType connectorType)
        {
            await wsupDbContext.ConnectorTypes.AddAsync(connectorType);
            await wsupDbContext.SaveChangesAsync();
            return connectorType;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<ConnectorType> Update(ConnectorType connectorType)
        {
            wsupDbContext.ConnectorTypes.Update(connectorType);
            await wsupDbContext.SaveChangesAsync();
            return connectorType;
        }

        [HttpDelete("{connectorTypeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(string connectorTypeId)
        {
            var connectorType = wsupDbContext.ConnectorTypes.FirstOrDefault(s => s.Id == connectorTypeId);

            if (connectorType != null)
            {
                wsupDbContext.ConnectorTypes.Remove(connectorType);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
