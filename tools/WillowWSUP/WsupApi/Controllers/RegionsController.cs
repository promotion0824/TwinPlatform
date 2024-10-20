using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegionsController : ControllerBase
    {
        private readonly ILogger<RegionsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public RegionsController(ILogger<RegionsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Region> GetAll()
        {
            return wsupDbContext.Regions;
        }

        [HttpGet("Get")]
        public Region? Get(string shortName)
        {
            return wsupDbContext.Regions.FirstOrDefault(s => s.ShortName == shortName);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Region> Create(Region region)
        {
            await wsupDbContext.Regions.AddAsync(region);
            await wsupDbContext.SaveChangesAsync();
            return region;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Region> Update(Region region)
        {
            wsupDbContext.Regions.Update(region);
            await wsupDbContext.SaveChangesAsync();
            return region;
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(string shortName)
        {
            var region = wsupDbContext.Regions.FirstOrDefault(s => s.ShortName == shortName);

            if (region != null)
            {
                wsupDbContext.Regions.Remove(region);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
