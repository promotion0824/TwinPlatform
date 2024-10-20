using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ILogger<CustomersController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public CustomersController(ILogger<CustomersController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Customer> GetAll()
        {
            return wsupDbContext.Customers;
        }

        [HttpGet("Get")]
        public Customer? Get(Guid customerId)
        {
            return wsupDbContext.Customers.FirstOrDefault(s => s.Id == customerId);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Customer> Create(Customer customer)
        {
            await wsupDbContext.Customers.AddAsync(customer);
            await wsupDbContext.SaveChangesAsync();
            return customer;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Customer> Update(Customer customer)
        {
            wsupDbContext.Customers.Update(customer);
            await wsupDbContext.SaveChangesAsync();
            return customer;
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(Guid customerId)
        {
            var customer = wsupDbContext.Customers.FirstOrDefault(s => s.Id == customerId);

            if (customer != null)
            {
                wsupDbContext.Customers.Remove(customer);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
