using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerInstanceApplicationsController : ControllerBase
    {
        private readonly ILogger<CustomerInstanceApplicationsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public CustomerInstanceApplicationsController(ILogger<CustomerInstanceApplicationsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public ActionResult<IEnumerable<CustomerInstanceApplication>> GetAll()
        {
            return wsupDbContext.CustomerInstanceApplications;
        }

        [HttpGet("GetAllApplicationsByCustomer/{customerInstanceId}")]
        public ActionResult<IEnumerable<CustomerInstanceApplication>> GetAll(Guid customerInstanceId)
        {
            return wsupDbContext.CustomerInstanceApplications.Where(c => c.CustomerInstanceId == customerInstanceId).ToList();
        }

        [HttpGet("{customerInstanceId}/{applicationId}")]
        public ActionResult<CustomerInstanceApplication> Get(Guid customerInstanceId, int applicationId)
        {
            var customerInstanceApplication = wsupDbContext.CustomerInstanceApplications.FirstOrDefault(c => c.CustomerInstanceId == customerInstanceId && c.ApplicationId == applicationId);

            if (customerInstanceApplication != null)
            {
                return Ok(customerInstanceApplication);
            }

            return NotFound();
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<IActionResult> Create(CustomerInstanceApplication customerInstanceApplication)
        {
            var newCustomerInstanceApplication = new CustomerInstanceApplication
            {
                CustomerInstanceId = customerInstanceApplication.CustomerInstanceId,
                ApplicationId = customerInstanceApplication.ApplicationId,
                CustomerInstanceApplicationStatusId = customerInstanceApplication.CustomerInstanceApplicationStatusId
            };

            await wsupDbContext.CustomerInstanceApplications.AddAsync(newCustomerInstanceApplication);
            await wsupDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<IActionResult> Update(CustomerInstanceApplication customerInstanceApplication)
        {
            wsupDbContext.CustomerInstanceApplications.Update(customerInstanceApplication);
            await wsupDbContext.SaveChangesAsync();
            return Ok(customerInstanceApplication);
        }

        [HttpDelete("{customerInstanceId}/{applicationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<IActionResult> Delete(Guid customerInstanceId, int applicationId)
        {
            var customerInstanceApp = wsupDbContext.CustomerInstanceApplications.FirstOrDefault(a => a.CustomerInstanceId == customerInstanceId && a.ApplicationId == applicationId);

            if (customerInstanceApp != null)
            {
                wsupDbContext.CustomerInstanceApplications.Remove(customerInstanceApp);
                await wsupDbContext.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }
    }
}
