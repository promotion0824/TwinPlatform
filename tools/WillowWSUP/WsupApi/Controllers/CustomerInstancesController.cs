using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerInstancesController : ControllerBase
    {
        private readonly ILogger<CustomerInstancesController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public CustomerInstancesController(ILogger<CustomerInstancesController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<CustomerInstance> GetAll(bool includeApplications = false)
        {
            if (includeApplications)
            {
                return wsupDbContext.CustomerInstances
                    .Include(s => s.CustomerInstanceApplications.Select(s => s.Application));
            }

            return wsupDbContext.CustomerInstances;
        }

        [HttpGet("Get")]
        public CustomerInstance? Get(Guid customerInstanceId, bool includeApplications = false)
        {
            if (includeApplications)
            {
                return wsupDbContext.CustomerInstances
                    .Include(s => s.CustomerInstanceApplications.Select(s => s.Application))
                    .FirstOrDefault(s => s.Id == customerInstanceId);
            }

            return wsupDbContext.CustomerInstances.FirstOrDefault(s => s.Id == customerInstanceId);

        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<CustomerInstance> Create(CustomerInstance customerInstance)
        {
            await wsupDbContext.CustomerInstances.AddAsync(customerInstance);
            await wsupDbContext.SaveChangesAsync();
            return customerInstance;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<CustomerInstance> Update(CustomerInstance customerInstance)
        {
            wsupDbContext.CustomerInstances.Update(customerInstance);
            await wsupDbContext.SaveChangesAsync();
            return customerInstance;
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(Guid customerInstanceId)
        {
            var customerInstance = wsupDbContext.CustomerInstances.FirstOrDefault(s => s.Id == customerInstanceId);

            if (customerInstance != null)
            {
                wsupDbContext.CustomerInstances.Remove(customerInstance);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
