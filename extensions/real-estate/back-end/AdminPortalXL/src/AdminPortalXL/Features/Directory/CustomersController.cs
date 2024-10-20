using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdminPortalXL.Dto;
using AdminPortalXL.Services;
using System.Collections.Generic;
using AdminPortalXL.Security;
using System;
using Willow.Infrastructure.Exceptions;
using Willow.Infrastructure.MultiRegion;
using System.Linq;
using System.IO;

namespace AdminPortalXL.Features.Directory
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly IMultiRegionSettings _regionSettings;
        private readonly IRegionalDirectoryApi _regionalDirectoryApi;

        public CustomersController(IMultiRegionSettings regionSettings, IRegionalDirectoryApi regionalDirectoryApi)
        {
            _regionSettings = regionSettings;
            _regionalDirectoryApi = regionalDirectoryApi;
        }

        [HttpGet("customers")]
        [AuthorizeForSupervisor]
        [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetCustomers()
        {
            var customers = await _regionalDirectoryApi.GetCustomers();
            customers = customers.OrderBy(c => c.Name).ToList();
            return Ok(CustomerDto.Map(customers));
        }

        [HttpPost("customers")]
        [AuthorizeForSupervisor]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateCustomer([FromForm] CreateCustomerRequest request, [FromForm]IFormFile logoImage)
        {
            if (!_regionSettings.RegionIds.Any(regionId => regionId == request.RegionId))
            {
                throw new ResourceNotFoundException("region", request.RegionId);
            }

            var customer = await _regionalDirectoryApi.CreateCustomer(request);

            if (logoImage != null && logoImage.Length > 0)
            {
                byte[] logoImageContent;
                using (var memoryStream = new MemoryStream())
                {
                    logoImage.OpenReadStream().CopyTo(memoryStream);
                    logoImageContent = memoryStream.ToArray();
                }
                customer = await _regionalDirectoryApi.UpdateCustomerLogo(request.RegionId, customer.Id, logoImageContent);
            }

            var dto = CustomerDto.Map(customer);
            dto.RegionId = request.RegionId;
            return Ok(dto);
        }

        [HttpPut("customers/{customerId}")]
        [AuthorizeForSupervisor]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateCustomer([FromRoute] Guid customerId, [FromForm] UpdateCustomerRequest request)
        {
            if (!_regionSettings.RegionIds.Any(regionId => regionId == request.RegionId))
            {
                throw new ResourceNotFoundException("region", request.RegionId);
            }

            var customer = await _regionalDirectoryApi.UpdateCustomer(customerId, request);

            var dto = CustomerDto.Map(customer);
            dto.RegionId = request.RegionId;
            return Ok(dto);
        }

        [HttpPost("customers/{customerId}/impersonate")]
        [AuthorizeForSupervisor]
        [ProducesResponseType(typeof(ImpersonateResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> ImpersonateCustomer([FromRoute] Guid customerId)
        {
            var regionId = await _regionalDirectoryApi.GetCustomerRegionId(customerId);
            if (string.IsNullOrEmpty(regionId))
            {
                throw new ResourceNotFoundException("customer", customerId);
            }

            var impersonateInfo = await _regionalDirectoryApi.Impersonate(regionId, customerId);

            return Ok(new ImpersonateResponse
            {
                RegionId = regionId,
                RegionCode = GetRegionCodeFromRegionId(regionId),
                AccessToken = impersonateInfo.AccessToken
            });
        }

        private static string GetRegionCodeFromRegionId(string regionId)
        {
            switch(regionId.Substring(0, 3))
            {
                case "aue":
                    return "au";
                case "eu2":
                    return "us";
                case "weu":
                    return "eu";
                default:
                    return string.Empty;
            }
        }
    }
}
