using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.SiteApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Swashbuckle.AspNetCore.Annotations;
using PlatformPortalXL.Helpers;
using System.Linq;
using Willow.Api.DataValidation;
using Willow.Common;
using Willow.Platform.Users;
using Willow.ExceptionHandling.Exceptions;
using Willow.Workflow;

namespace PlatformPortalXL.Features.Workflow
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ReportersController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IWorkflowApiService _workflowApi;
        private readonly ISiteApiService _siteApi;
        private readonly IDirectoryApiService _directoryApi;

        public ReportersController(IAccessControlService accessControl, IWorkflowApiService workflowApi, ISiteApiService siteApi, IDirectoryApiService directoryApi)
        {
            _accessControl = accessControl;
            _workflowApi = workflowApi;
            _siteApi = siteApi;
            _directoryApi = directoryApi;
        }

        [HttpGet("sites/{siteId}/requestors")]
        [Authorize]
        [ProducesResponseType(typeof(List<RequestorDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of reporters", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> GetReporters([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var reporters = await _workflowApi.GetReporters(siteId);

            var users = await _directoryApi.GetSiteUsers(siteId);
            var activeUsers = users.FindAll(x => x.Status == UserStatus.Active);

            var result = new List<RequestorDto>();
            result.AddRange(RequestorDto.MapFromModels(activeUsers));
            result.AddRange(RequestorDto.MapFromModels(reporters));

            return Ok(result);
        }

        [HttpPost("sites/{siteId}/reporters")]
        [Authorize]
        [ProducesResponseType(typeof(ReporterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ReporterDto), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Creates a reporters", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> CreateReporter([FromRoute] Guid siteId, [FromBody] CreateReporterRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var error = new ValidationError();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), "Name is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email is required"));
            }
            else if (!ValidationHelper.IsEmailValid(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email is invalid"));
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Phone), "Phone is required"));
            }
            else if (!ValidationHelper.IsPhoneNumberValid(request.Phone))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Phone), "Phone is invalid"));
            }

            if (error.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            var requestToWorkflow = new WorkflowCreateReporterRequest
            {
                CustomerId = site.CustomerId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Company = request.Company
            };

            var reporter = await _workflowApi.CreateReporter(siteId, requestToWorkflow);
            return Ok(ReporterDto.MapFromModel(reporter));
        }

        [HttpPut("sites/{siteId}/reporters/{reporterId}")]
        [Authorize]
        [ProducesResponseType(typeof(ReporterDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Updates the specified reporter", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> UpdateReporter([FromRoute] Guid siteId, [FromRoute] Guid reporterId, [FromBody] UpdateReporterRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var error = new ValidationError();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), "Name is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email is required"));
            }
            else if (!ValidationHelper.IsEmailValid(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email is invalid"));
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Phone), "Phone is required"));
            }
            else if (!ValidationHelper.IsPhoneNumberValid(request.Phone))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Phone), "Phone is invalid"));
            }

            if (error.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            var reporter = await _workflowApi.UpdateReporter(siteId, reporterId, request);
            return Ok(ReporterDto.MapFromModel(reporter));
        }

        [HttpDelete("sites/{siteId}/reporters/{reporterId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Deletes the specified reporter", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> DeleteReporter([FromRoute] Guid siteId, [FromRoute] Guid reporterId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            await _workflowApi.DeleteReporter(siteId, reporterId);
            return NoContent();
        }
    }
}
