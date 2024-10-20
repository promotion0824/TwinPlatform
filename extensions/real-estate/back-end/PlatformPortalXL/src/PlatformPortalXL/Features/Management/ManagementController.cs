using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Http;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Api.DataValidation;
using Willow.Management;
using PlatformPortalXL.Helpers;

namespace PlatformPortalXL.Features.Management
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ManagementController : TranslationController
    {
        private readonly IDirectoryApiService _directoryApi;
        private readonly IControllerHelper _controllerHelper;
        private readonly IManagementService _managementService;

        public ManagementController(
            IDirectoryApiService directoryApi,
            IControllerHelper controllerHelper,
            IManagementService managementService,
            IHttpRequestHeaders headers)
            : base(headers)
        {
            _directoryApi      = directoryApi;
            _controllerHelper  = controllerHelper;
            _managementService = managementService ?? throw new ArgumentNullException(nameof(managementService));
        }

        [HttpGet("management/managedPortfolios")]
        [Authorize]
        [ProducesResponseType(typeof(List<ManagedPortfolioDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets managed portfolios for user", Tags = new [] { "Management" })]
        public async Task<ActionResult> GetManagedPortfolios()
        {
            var currentUserId = this.CurrentUserId;
            var currentUser = await _directoryApi.GetUser(currentUserId);

            var managedPortfolios = await _managementService.GetManagedPortfolios(currentUser.CustomerId, currentUserId);

            return Ok(managedPortfolios);
        }

        [HttpGet("management/customers/{customerId}/users/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ManagedUserDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get managed user", Tags = new [] { "Management" })]
        public async Task<ActionResult> GetManagedUser([FromRoute] Guid customerId, [FromRoute] Guid userId)
        {
            var managedUser = await _managementService.GetManagedUser(customerId, this.CurrentUserId, userId);

            return Ok(managedUser);
        }
       
        [HttpPost("management/customers/{customerId}/users")]
        [Authorize]
        [ProducesResponseType(typeof(ManagedUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Create managed user", Tags = new[] {"Management"})]
        public async Task<ActionResult> CreateManagedUser([FromRoute] Guid customerId,
            [FromBody] CreateManagedUserRequest request)
        {
            var managedUser = await _managementService.CreateManagedUser(customerId, this.CurrentUserId, request, this.Language);

            return Ok(managedUser);
        }

        [HttpPut("management/customers/{customerId}/users/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Update managed user", Tags = new[] {"Management"})]
        public async Task<ActionResult> UpdateManagedUser([FromRoute] Guid customerId, [FromRoute] Guid userId,
            [FromBody] UpdateManagedUserRequest request)
        {
            await _managementService.UpdateManagedUser(customerId, this.CurrentUserId, userId, request, this.Language);

            return NoContent();
        }

        [HttpDelete("management/customers/{customerId}/users/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ManagedUserDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Delete managed user", Tags = new [] { "Management" })]
        public async Task<ActionResult> DeleteManagedUser([FromRoute] Guid customerId, [FromRoute] Guid userId)
        {
            await _managementService.DeleteManagedUser(customerId, this.CurrentUserId, userId);

            return NoContent();
        }

        #region Private
        private Guid CurrentUserId => _controllerHelper.GetCurrentUserId(this);
        #endregion
    }
}
