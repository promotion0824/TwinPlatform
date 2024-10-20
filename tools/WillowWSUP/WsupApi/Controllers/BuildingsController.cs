using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildingsController : ControllerBase
    {
        private readonly ILogger<BuildingsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public BuildingsController(ILogger<BuildingsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet]
        public IEnumerable<Building> GetAll()
        {
            return wsupDbContext.Buildings;
        }

        [HttpGet("GetAllByCustomerInstanceId/{customerInstanceId}")]
        public IEnumerable<Building> GetAllByCustomerInstanceId(Guid customerInstanceId)
        {
            return wsupDbContext.Buildings.Where(b => b.CustomerInstanceId == customerInstanceId);
        }

        [HttpGet("{buildingId}")]
        public Building? Get(string buildingId)
        {
            return wsupDbContext.Buildings.FirstOrDefault(s => s.Id == buildingId);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Building> Create(Building building)
        {
            await wsupDbContext.Buildings.AddAsync(building);
            await wsupDbContext.SaveChangesAsync();
            return building;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Building> Update(Building building)
        {
            wsupDbContext.Buildings.Update(building);
            await wsupDbContext.SaveChangesAsync();
            return building;
        }

        [HttpDelete("{buildingId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(string buildingId)
        {
            var building = wsupDbContext.Buildings.FirstOrDefault(s => s.Id == buildingId);

            if (building != null)
            {
                wsupDbContext.Buildings.Remove(building);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
