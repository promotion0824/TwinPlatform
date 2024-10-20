using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly ILogger<ApplicationsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public ApplicationsController(ILogger<ApplicationsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Application> GetAll()
        {
            return wsupDbContext.Applications;
        }

        [HttpGet("Get")]
        public Application? Get(int applicationId)
        {
            return wsupDbContext.Applications.FirstOrDefault(s => s.Id == applicationId);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Application> Create(Application application)
        {
            await wsupDbContext.Applications.AddAsync(application);
            await wsupDbContext.SaveChangesAsync();
            return application;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Application> Update(Application application)
        {
            wsupDbContext.Applications.Update(application);
            await wsupDbContext.SaveChangesAsync();
            return application;
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(int applicationId)
        {
            var application = wsupDbContext.Applications.FirstOrDefault(s => s.Id == applicationId);

            if (application != null)
            {
                wsupDbContext.Applications.Remove(application);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
