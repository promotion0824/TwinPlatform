using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;
using Environment = Willow.Infrastructure.Entities.Environment;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnvironmentsController : ControllerBase
    {
        private readonly ILogger<EnvironmentsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public EnvironmentsController(ILogger<EnvironmentsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Environment> GetAll()
        {
            return wsupDbContext.Environments;
        }

        [HttpGet("Get")]
        public Environment? Get(string name)
        {
            return wsupDbContext.Environments.FirstOrDefault(s => s.Name == name);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Environment> Create(Environment environment)
        {
            await wsupDbContext.Environments.AddAsync(environment);
            await wsupDbContext.SaveChangesAsync();
            return environment;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Environment> Update(Environment environment)
        {
            wsupDbContext.Environments.Update(environment);
            await wsupDbContext.SaveChangesAsync();
            return environment;
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(string name)
        {
            var environment = wsupDbContext.Environments.FirstOrDefault(s => s.Name == name);

            if (environment != null)
            {
                wsupDbContext.Environments.Remove(environment);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
