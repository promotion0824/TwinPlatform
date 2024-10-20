using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StampsController : ControllerBase
    {
        private readonly ILogger<StampsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public StampsController(ILogger<StampsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Stamp> GetAll()
        {
            return wsupDbContext.Stamps;
        }

        [HttpGet("Get")]
        public Stamp? Get(Guid stampId)
        {
            return wsupDbContext.Stamps.FirstOrDefault(s => s.Id == stampId);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Stamp> Create(Stamp stamp)
        {
            await wsupDbContext.Stamps.AddAsync(stamp);
            await wsupDbContext.SaveChangesAsync();
            return stamp;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Stamp> Update(Stamp stamp)
        {
            wsupDbContext.Stamps.Update(stamp);
            await wsupDbContext.SaveChangesAsync();
            return stamp;
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(Guid stampId)
        {
            var stamp = wsupDbContext.Stamps.FirstOrDefault(s => s.Id == stampId);

            if (stamp != null)
            {
                wsupDbContext.Stamps.Remove(stamp);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
