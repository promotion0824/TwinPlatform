using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Enums;
using DirectoryCore.Http;
using DirectoryCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CustomerUsersController : TranslationController
    {
        private readonly ICustomerUsersService _customerUsersService;

        public CustomerUsersController(
            ICustomerUsersService customerUsersService,
            IHttpRequestHeaders headers
        )
            : base(headers)
        {
            _customerUsersService = customerUsersService;
        }

        [Authorize]
        [HttpGet("customers/{customerId}/users")]
        [ProducesResponseType(typeof(IList<UserDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerUsers([FromRoute] Guid customerId)
        {
            var users = await _customerUsersService.GetCustomerUsers(customerId);
            return Ok(UserDto.MapFrom(users));
        }

        [Authorize]
        [HttpGet("customers/{customerId}/users/{userId}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerUser(Guid customerId, Guid userId)
        {
            var user = await _customerUsersService.GetCustomerUser(customerId, userId);
            return Ok(UserDto.MapFrom(user));
        }

        [Authorize]
        [HttpPost("customers/{customerId}/users")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateCustomerUser(
            Guid customerId,
            CreateCustomerUserRequest createCustomerUserRequest
        )
        {
            var user = await _customerUsersService.CreateCustomerUser(
                customerId,
                createCustomerUserRequest,
                Language
            );
            return Ok(UserDto.MapFrom(user));
        }

        [Authorize]
        [HttpPut("customers/{customerId}/users/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateCustomerUser(
            Guid customerId,
            Guid userId,
            UpdateCustomerUserRequest updateCustomerUserRequest
        )
        {
            await _customerUsersService.UpdateCustomerUser(
                customerId,
                userId,
                updateCustomerUserRequest
            );
            return NoContent();
        }

        [Authorize]
        [HttpPut("customers/{customerId}/users/{userId}/status")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateCustomerUserStatus(
            Guid customerId,
            Guid userId,
            UpdateUserStatusRequest request
        )
        {
            switch (request.Status)
            {
                case UserStatus.Inactive:
                    await _customerUsersService.InactivateCustomerUser(customerId, userId);
                    break;
                default:
                    throw new BadRequestException($"Unsupported status: {request.Status}");
            }
            return NoContent();
        }

        [Authorize]
        [HttpPost("customers/{customerId}/users/{userId}/sendActivation")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SendActivation(Guid customerId, Guid userId)
        {
            await _customerUsersService.SendActivationEmail(customerId, userId, Language);
            return NoContent();
        }
    }
}
