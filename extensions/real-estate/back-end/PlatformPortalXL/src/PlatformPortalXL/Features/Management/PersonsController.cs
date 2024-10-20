using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Http;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Api.DataValidation;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Workflow;
using Willow.ExceptionHandling.Exceptions;

namespace PlatformPortalXL.Features.Management
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class PersonsController : TranslationController
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly ISiteApiService _siteApi;
        private readonly INotificationService _notificationService;
        private readonly IPersonManagementService _personManagementService;

        private const string ContactMandatoryErrorMessage = "Contact is required";
        private const string ContractInvalidErrorMessage = "Contact is invalid";

        public PersonsController(IAccessControlService accessControl,
                                 IDirectoryApiService directoryApi,
                                 IWorkflowApiService workflowApi,
                                 ISiteApiService siteApi,
                                 INotificationService notificationService,
                                 IPersonManagementService personManagementService, 
                                 IHttpRequestHeaders headers)
                                 : base(headers)
        {
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _workflowApi = workflowApi;
            _siteApi = siteApi;
            _notificationService = notificationService;
            _personManagementService = personManagementService;
        }

        [HttpGet("sites/{siteId}/persons")]
        [Authorize]
        [ProducesResponseType(typeof(List<PersonDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets site's person list", Tags = new [] { "Management" })]
        public async Task<ActionResult> GetSiteUsers([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewUsers, siteId);

            var reporters = await _workflowApi.GetReporters(siteId);
            var users = await _directoryApi.GetSiteUsers(siteId);

            var result = new List<PersonDto>();
            result.AddRange(PersonDto.Map(users));
            result.AddRange(PersonDto.Map(reporters));
            return Ok(result);
        }

        [HttpPost("sites/{siteId}/persons")]
        [Authorize]
        [ProducesResponseType(typeof(CreatePersonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Create a site's person", Tags = new [] { "Management" })]
        public async Task<ActionResult> CreateSiteUser([FromRoute] Guid siteId, [FromBody] CreateSiteUserRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageUsers, siteId);

            var validationError = await ValidateCreatePersonRequest(request, siteId);
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            PersonDto result;
            var message = string.Empty;
            switch (request.Type.Value)
            {
                case SitePersonType.CustomerUser:
                    (result, message) = await CreateCustomerUser(siteId, request, site);
                    break;
                case SitePersonType.Reporter:
                    var reporter = await _workflowApi.CreateReporter(siteId, 
                                                        new WorkflowCreateReporterRequest 
                                                        { 
                                                            Name = request.FullName,
                                                            CustomerId = site.CustomerId,
                                                            Company = request.Company,
                                                            Email = request.Email,
                                                            Phone = request.ContactNumber
                                                        });
                    result = PersonDto.Map(reporter);
                    break;
                default:
                    throw new ArgumentException().WithData(new { requestType = request.Type });
            }

            return Ok(new CreatePersonResponse { Person = result, Message = message });
        }

        private async Task<(PersonDto,string message)> CreateCustomerUser(Guid siteId, CreateSiteUserRequest request, Site site)
        {
            string message = string.Empty;
            var account = await _directoryApi.GetAccount(request.Email);
            User user;

            if (account != null)
            {
                user = await _directoryApi.GetUser(account.UserId);
                var customer = await _directoryApi.GetCustomer(site.CustomerId);

                await _directoryApi.CreateUserAssignment(account.UserId, request.RoleId.Value, RoleResourceType.Site, siteId);
                await SendEmail(customer.Id, user.Id, site.Name, user.Type.ToString());

                message = "User already exists and will be provided access to the site.";
            }
            else
            {
                user = await _directoryApi.CreateCustomerUser(site.CustomerId,
                    new DirectoryCreateCustomerUserRequest
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        Mobile = request.ContactNumber,
                        Company = request.Company,
                    });
                await _directoryApi.CreateUserAssignment(user.Id, request.RoleId.Value, RoleResourceType.Site, siteId);
            }
            return (PersonDto.Map(user), message);
        }

        private async Task<ValidationError> ValidateCreatePersonRequest(CreateSiteUserRequest request, Guid siteId)
        {
            var error = new ValidationError();

            if (!request.Type.HasValue)
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Type), "Type is required"));
                return error;
            }

            switch (request.Type.Value)
            {
                case SitePersonType.CustomerUser:
                    await ValidateCreateCustomerUser(request, siteId, error);
                    break;
                case SitePersonType.Reporter:
                    ValidateCreateReporter(request, error);
                    break;
                default:
                    throw new ArgumentException().WithData(new { requestType = request.Type });
            }
            return error;
        }

        private static void ValidateCreateReporter(CreateSiteUserRequest request, ValidationError error)
        {
            if (string.IsNullOrEmpty(request.FullName))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.FullName), "Name is required"));
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email address is required"));
            }
            else if (!ValidationHelper.IsEmailValid(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email address is invalid"));
            }

            if (string.IsNullOrEmpty(request.ContactNumber))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContactMandatoryErrorMessage));
            }
            else if (!ValidationHelper.IsPhoneNumberValid(request.ContactNumber))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContractInvalidErrorMessage));
            }
        }

        private async Task ValidateCreateCustomerUser(CreateSiteUserRequest request, Guid siteId, ValidationError error)
        {
            if (string.IsNullOrEmpty(request.FirstName))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.FirstName), "First name is required"));
            }

            if (string.IsNullOrEmpty(request.LastName))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.LastName), "Last name is required"));
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email address is required"));
            }
            else if (!ValidationHelper.IsEmailValid(request.Email))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email address is invalid"));
            }
            else
            {
                await ValidateCustomerEmailOnCreate(request, siteId, error);
            }

            if (string.IsNullOrEmpty(request.ContactNumber))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContactMandatoryErrorMessage));
            }
            else if (!ValidationHelper.IsPhoneNumberValid(request.ContactNumber))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContractInvalidErrorMessage));
            }

            if (!request.RoleId.HasValue)
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.RoleId), "Role is required"));
            }
        }

        private async Task ValidateCustomerEmailOnCreate(CreateSiteUserRequest request, Guid siteId, ValidationError error)
        {
            var account = await _directoryApi.GetAccount(request.Email);
            if (account != null)
            {
                var roleAssignments = await _directoryApi.GetRoleAssignments(account.UserId, siteId: siteId);
                if (roleAssignments.Any())
                {
                    error.Items.Add(new ValidationErrorItem(nameof(request.Email), "User already exists"));
                }
            }
        }

        [HttpPut("sites/{siteId}/persons/{personId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Update a site's person", Tags = new [] { "Management" })]
        public async Task<ActionResult> UpdateSiteUser([FromRoute] Guid siteId, [FromRoute] Guid personId, [FromBody] UpdateSiteUserRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageUsers, siteId);

            var validationError = ValidateUpdatePersonRequest(request);
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            switch (request.Type)
            {
                case SitePersonType.CustomerUser:
                    var siteUsers = await _directoryApi.GetSiteUsers(site.Id);
                    if (!siteUsers.Any(x => x.Id == personId))
                    {
                        throw new NotFoundException().WithData(new { personId });
                    }

                    await _directoryApi.UpdateCustomerUser(site.CustomerId, personId,
                                                            new DirectoryUpdateCustomerUserRequest
                                                            {
                                                                FirstName = request.FirstName,
                                                                LastName = request.LastName,
                                                                Mobile = request.ContactNumber,
                                                                Company = request.Company
                                                            });
                    await _directoryApi.UpdateUserAssignment(personId, request.RoleId.Value, RoleResourceType.Site, siteId);
                    break;
                case SitePersonType.Reporter:
                    var reporters = await _workflowApi.GetReporters(siteId);
                    if (!reporters.Any(x => x.Id == personId))
                    {
                        throw new NotFoundException().WithData(new { personId });
                    }

                    await _workflowApi.UpdateReporter(siteId, personId,
                                                        new UpdateReporterRequest
                                                        {
                                                            Name = request.FullName,
                                                            Company = request.Company,
                                                            Email = request.Email,
                                                            Phone = request.ContactNumber
                                                        });
                    break;
                default:
                    throw new ArgumentException().WithData(new { requestType = request.Type });
            }

            return NoContent();
        }

        private static ValidationError ValidateUpdatePersonRequest(UpdateSiteUserRequest request)
        {
            var error = new ValidationError();

            switch (request.Type)
            {
                case SitePersonType.CustomerUser:
                    if (string.IsNullOrEmpty(request.FirstName))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.FirstName), "First name is required"));
                    }
                    if (string.IsNullOrEmpty(request.LastName))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.LastName), "Last name is required"));
                    }
                    if (string.IsNullOrEmpty(request.ContactNumber))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContactMandatoryErrorMessage));
                    }
                    else if (!ValidationHelper.IsPhoneNumberValid(request.ContactNumber))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContractInvalidErrorMessage));
                    }
                    if (!request.RoleId.HasValue)
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.RoleId), "Role is required"));
                    }
                    break;
                case SitePersonType.Reporter:
                    if (string.IsNullOrEmpty(request.FullName))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.FullName), "Name is required"));
                    }
                    if (string.IsNullOrEmpty(request.Email))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email address is required"));
                    }
                    else if (!ValidationHelper.IsEmailValid(request.Email))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.Email), "Email address is invalid"));
                    }
                    if (string.IsNullOrEmpty(request.ContactNumber))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContactMandatoryErrorMessage));
                    }
                    else if (!ValidationHelper.IsPhoneNumberValid(request.ContactNumber))
                    {
                        error.Items.Add(new ValidationErrorItem(nameof(request.ContactNumber), ContractInvalidErrorMessage));
                    }
                    break;
                default:
                    throw new ArgumentException().WithData(new { requestType = request.Type });
            }
            return error;
        }

        [HttpDelete("sites/{siteId}/persons/{personId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Delete a site's person", Tags = new [] { "Management" })]
        public async Task<ActionResult> DeleteSiteUser([FromRoute] Guid siteId, [FromRoute] Guid personId, [FromQuery] SitePersonType personType)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageUsers, siteId);

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            switch (personType)
            {
                case SitePersonType.CustomerUser:
                    await _directoryApi.DeleteCustomerUser(site.CustomerId, personId);
                    break;
                case SitePersonType.Reporter:
                    await _workflowApi.DeleteReporter(siteId, personId);
                    break;
                default:
                    throw new ArgumentException().WithData(new { requestType = personType });
            }
            return NoContent();
        }

        [HttpDelete("sites/{siteId}/persons/{personId}/assignments")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Revoke site user's role assignment ", Tags = new[] { "Management" })]
        public async Task<ActionResult> DeleteSiteUsersAssignments([FromRoute] Guid siteId, [FromRoute] Guid personId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageUsers, siteId);

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            await _directoryApi.DeleteUserAssignment(personId, siteId);

            return NoContent();
        }

        [HttpGet("me/persons")]
        [Authorize]
        [ProducesResponseType(typeof(List<PersonDetailDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets person list based on permission of requester", Tags = new[] { "Management" })]
        public async Task<ActionResult> GetUsersOnPermission()
        {
            var userId = this.GetCurrentUserId();
            var result = await _personManagementService.GetUsersBasedOnUserPermission(userId);
            return Ok(result);
        }

        #region Private
      
        private async Task SendEmail(Guid customerId, Guid userId, string siteName,string userType)
        {
            var parameters = new 
            {
                SitesInTitle = siteName,
                SitesInBody  = $"<li>{siteName}</li>",
                SitesLabel   = "site"
            };
            await _notificationService.SendNotificationAsync(new Willow.Notifications.Models.Notification
            {
                CorrelationId = Guid.NewGuid(),
                CommunicationType = CommunicationType.Email,
                CustomerId = customerId,
                Data = parameters.ToDictionary(),
                Tags = null,
                TemplateName = "SiteAssigned",
                UserId = userId,
                UserType = userType,
                Locale = this.Language

            });
          
        }

        #endregion
    }
}
