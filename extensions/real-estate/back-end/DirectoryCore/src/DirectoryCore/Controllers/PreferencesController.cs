using System;
using System.Threading.Tasks;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Infrastructure.Extensions;
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
    public class CustomerUserPreferencesController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public CustomerUserPreferencesController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [Authorize]
        [HttpGet("customers/{customerId}/users/{userId}/preferences")]
        [ProducesResponseType(typeof(CustomerUserPreferences), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPreferences(
            [FromRoute] Guid customerId,
            [FromRoute] Guid userId
        )
        {
            var customerUserPreferences = await _usersService.GetCustomerUserPreferences(userId);
            return Ok(customerUserPreferences);
        }

        [Authorize]
        [HttpPut("customers/{customerId}/users/{userId}/preferences")]
        [ProducesResponseType(typeof(CustomerUserPreferences), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateCustomerUserPreferences(
            [FromRoute] Guid customerId,
            [FromRoute] Guid userId,
            [FromBody] CustomerUserPreferencesRequest customerUserPreferencesRequest
        )
        {
            if (!customerUserPreferencesRequest.Language.IsCultureCode())
            {
                throw new BadRequestException("Invalid language");
            }

            await _usersService.CreateOrUpdateCustomerUserPreference(
                userId,
                customerUserPreferencesRequest
            );
            return NoContent();
        }

        [Authorize]
        [HttpGet("customers/{customerId}/users/{userId}/preferences/timeSeries")]
        [ProducesResponseType(typeof(CustomerUserTimeSeriesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerUserTimeSeries(
            [FromRoute] Guid customerId,
            [FromRoute] Guid userId
        )
        {
            var customerUserTimeSeries = await _usersService.GetCustomerUserTimeSeries(userId);

            if (customerUserTimeSeries == null)
            {
                return NotFound();
            }

            return Ok(customerUserTimeSeries);
        }

        [Authorize]
        [HttpPut("customers/{customerId}/users/{userId}/preferences/timeSeries")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateCustomerUserTimeSeries(
            [FromRoute] Guid customerId,
            [FromRoute] Guid userId,
            [FromBody] CustomerUserTimeSeriesRequest customerUserTimeSeriesRequest
        )
        {
            await _usersService.CreateOrUpdateCustomerUserTimeSeries(
                userId,
                customerUserTimeSeriesRequest
            );
            return NoContent();
        }
    }
}
