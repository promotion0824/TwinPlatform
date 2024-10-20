using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Directory.Models;
using Willow.Infrastructure.Exceptions;
using IAuthorizationService = DirectoryCore.Services.IAuthorizationService;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class PermissionController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public PermissionController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [Authorize]
        [HttpGet("users/{userId}/permissions/{permissionId}/eligibility")]
        [ProducesResponseType(typeof(AuthorizationInfo), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AuthorizePermission(
            [FromRoute] Guid userId,
            [FromRoute] string permissionId,
            [FromQuery] Guid? customerId,
            [FromQuery] Guid? portfolioId,
            [FromQuery] Guid? siteId
        )
        {
            var givenResourceCount =
                (customerId.HasValue ? 1 : 0)
                + (portfolioId.HasValue ? 1 : 0)
                + (siteId.HasValue ? 1 : 0);
            if (givenResourceCount > 1)
            {
                throw new BadRequestException("More than one resource ids were provided.");
            }

            AuthorizationInfo result;
            if (customerId.HasValue)
            {
                result = await _authorizationService.CheckPermissionOnCustomer(
                    userId,
                    permissionId,
                    customerId.Value
                );
            }
            else if (portfolioId.HasValue)
            {
                result = await _authorizationService.CheckPermissionOnPortfolio(
                    userId,
                    permissionId,
                    portfolioId.Value
                );
            }
            else if (siteId.HasValue)
            {
                result = await _authorizationService.CheckPermissionOnSite(
                    userId,
                    permissionId,
                    siteId.Value
                );
            }
            else
            {
                throw new BadRequestException("No resource id was provided");
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPost("users/{userId}/permissionAssignments")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateUserAssignment(
            [FromRoute] Guid userId,
            [FromBody] CreateUserAssignmentRequest request
        )
        {
            await _authorizationService.CreateUserAssignment(
                userId,
                request.RoleId,
                request.ResourceType,
                request.ResourceId
            );
            return NoContent();
        }

        [Authorize]
        [HttpPost("users/{userId}/permissionAssignments/list")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateUserAssignments(
            [FromRoute] Guid userId,
            [FromBody] List<RoleAssignment> roles
        )
        {
            await _authorizationService.CreateUserAssignments(userId, roles);

            return NoContent();
        }

        [Authorize]
        [HttpPut("users/{userId}/permissionAssignments")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateUserAssignment(
            [FromRoute] Guid userId,
            [FromBody] UpdateUserAssignmentRequest request
        )
        {
            await _authorizationService.UpdateUserAssignment(
                userId,
                request.RoleId,
                request.ResourceType,
                request.ResourceId
            );
            return NoContent();
        }

        [Authorize]
        [HttpGet("users/{userId}/permissionAssignments")]
        [ProducesResponseType(typeof(List<RoleAssignmentDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> VerifyRoleAssignment(
            [FromRoute] Guid userId,
            [FromQuery] Guid? customerId,
            [FromQuery] Guid? portfolioId,
            [FromQuery] Guid? siteId
        )
        {
            var givenResourceCount =
                (customerId.HasValue ? 1 : 0)
                + (portfolioId.HasValue ? 1 : 0)
                + (siteId.HasValue ? 1 : 0);
            if (givenResourceCount > 1)
            {
                throw new BadRequestException("More than one resource ids were provided.");
            }

            var result = await _authorizationService.GetRoleAssignments(
                userId,
                customerId,
                portfolioId,
                siteId
            );

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("users/{userId}/permissionAssignments")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteUserAssignment(
            [FromRoute] Guid userId,
            [FromQuery] Guid resourceId
        )
        {
            await _authorizationService.DeleteUserAssignmentsByResource(userId, resourceId);
            return NoContent();
        }
    }
}
