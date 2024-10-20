using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomersService _customersService;
        private readonly IImagePathHelper _imagePathHelper;

        public CustomersController(
            ICustomersService customersService,
            IImagePathHelper imagePathHelper
        )
        {
            _customersService = customersService;
            _imagePathHelper = imagePathHelper;
        }

        [Authorize]
        [HttpGet("customers")]
        [ProducesResponseType(typeof(List<CustomerDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomers([FromQuery] bool? active)
        {
            var customers = await _customersService.GetCustomers(active);
            return Ok(CustomerDto.MapFrom(customers, _imagePathHelper));
        }

        [Authorize]
        [HttpGet("customers/{customerId}")]
        [ProducesResponseType(typeof(CustomerDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(CustomerDto), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCustomer(Guid customerId)
        {
            var customer = await _customersService.GetCustomer(customerId);
            if (customer == null)
            {
                return NotFound(null);
            }

            return Ok(CustomerDto.MapFrom(customer, _imagePathHelper));
        }

        [Authorize]
        [HttpGet("customers/{customerId}/sites")]
        [ProducesResponseType(typeof(IList<SiteDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSites(Guid customerId, string query)
        {
            var sites = await _customersService.GetSites(customerId, query);
            return Ok(SiteDto.MapFrom(sites));
        }

        [Authorize]
        [HttpPost("customers")]
        [ProducesResponseType(typeof(CustomerDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            var customer = await _customersService.CreateCustomer(request);
            return Ok(CustomerDto.MapFrom(customer, _imagePathHelper));
        }

        [Authorize]
        [HttpPut("customers/{customerId}")]
        [ProducesResponseType(typeof(CustomerDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCustomer(
            [FromRoute] Guid customerId,
            [FromBody] UpdateCustomerRequest request
        )
        {
            var customer = await _customersService.UpdateCustomer(customerId, request);
            return Ok(CustomerDto.MapFrom(customer, _imagePathHelper));
        }

        [Authorize]
        [HttpPut("customers/{customerId}/logo")]
        [ProducesResponseType(typeof(CustomerDto), (int)HttpStatusCode.OK)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCustomerLogo(
            [FromRoute] Guid customerId,
            IFormFile logoImage
        )
        {
            byte[] logoImageContent;
            using (var memoryStream = new MemoryStream())
            {
                logoImage.OpenReadStream().CopyTo(memoryStream);
                logoImageContent = memoryStream.ToArray();
            }
            var customer = await _customersService.UpdateCustomerLogo(customerId, logoImageContent);
            return Ok(CustomerDto.MapFrom(customer, _imagePathHelper));
        }

        [Authorize]
        [HttpPost("customers/{customerId}/impersonate")]
        [ProducesResponseType(typeof(ImpersonateInfo), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ImpersonateCustomer([FromRoute] Guid customerId)
        {
            var token = await _customersService.GetImpersonateAccessToken(customerId);
            return Ok(new ImpersonateInfo { AccessToken = token });
        }

        [Authorize]
        [HttpGet("customers/{customerId}/modelsOfInterest")]
        [ProducesResponseType(typeof(List<CustomerModelOfInterestDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResourceNotFoundException), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCustomerModelsOfInterest(Guid customerId)
        {
            var customerModelsOfInterest = await _customersService.GetCustomerModelsOfInterest(
                customerId
            );

            return Ok(CustomerModelOfInterestDto.MapFrom(customerModelsOfInterest));
        }

        [Authorize]
        [HttpGet("customers/{customerId}/modelsOfInterest/{id}")]
        [ProducesResponseType(typeof(CustomerModelOfInterestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResourceNotFoundException), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCustomerModelOfInterest(Guid customerId, Guid id)
        {
            var customerModelsOfInterest = await _customersService.GetCustomerModelsOfInterest(
                customerId
            );

            var customerModelOfInterest = customerModelsOfInterest.FirstOrDefault(x => x.Id == id);
            if (customerModelOfInterest == null)
            {
                throw new ResourceNotFoundException("model", id);
            }

            return Ok(CustomerModelOfInterestDto.MapFrom(customerModelOfInterest));
        }

        [Authorize]
        [HttpDelete("customers/{customerId}/modelsOfInterest/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ResourceNotFoundException), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BadRequestException), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteCustomerModelOfInterest(
            [FromRoute] Guid customerId,
            [FromRoute] Guid id
        )
        {
            await _customersService.DeleteCustomerModelOfInterest(customerId, id);

            return NoContent();
        }

        [Authorize]
        [HttpPost("customers/{customerId}/modelsOfInterest")]
        [ProducesResponseType(typeof(CustomerModelOfInterestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestException), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ResourceNotFoundException), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateCustomerModelOfInterest(
            [FromRoute] Guid customerId,
            [FromBody] CreateCustomerModelOfInterestRequest request
        )
        {
            var result = await _customersService.CreateCustomerModelOfInterest(customerId, request);

            return Ok(CustomerModelOfInterestDto.MapFrom(result));
        }

        [Authorize]
        [HttpPut("customers/{customerId}/modelsOfInterest/{id}")]
        [ProducesResponseType(typeof(CustomerModelOfInterestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestException), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ResourceNotFoundException), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateCustomerModelOfInterest(
            [FromRoute] Guid customerId,
            [FromRoute] Guid id,
            [FromBody] UpdateCustomerModelOfInterestRequest request
        )
        {
            var result = await _customersService.UpdateCustomerModelOfInterest(
                customerId,
                id,
                request
            );

            return Ok(CustomerModelOfInterestDto.MapFrom(result));
        }

        [Authorize]
        [HttpPut("customers/{customerId}/modelsOfInterest")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ResourceNotFoundException), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateCustomerModelsOfInterest(
            [FromRoute] Guid customerId,
            [FromBody] UpdateCustomerModelsOfInterestRequest request
        )
        {
            await _customersService.UpdateCustomerModelsOfInterest(customerId, request);

            return NoContent();
        }
    }
}
