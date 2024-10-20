using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.DirectoryCore;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.Pilot;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using Willow.Api.Client;
using System.Net;
using Willow.Workflow;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Features.Directory
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IDigitalTwinApiService _digitalTwinApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;

        public CustomersController(IAccessControlService accessControl,
                                   IDirectoryApiService directoryApi,
                                   IDigitalTwinApiService digitalTwinApi,
                                   IWorkflowApiService workflowApi,
                                   IUserAuthorizedSitesService userAuthorizedSitesService)
        {
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _digitalTwinApi = digitalTwinApi;
            _workflowApi = workflowApi;
            _userAuthorizedSitesService = userAuthorizedSitesService;
        }

        [HttpGet("customers/{customerId}/users")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of users", Tags = new[] { "Customers" })]
        public async Task<ActionResult> GetCustomerUsers([FromRoute] Guid customerId)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManageUsers, customerId);

            var users = await _directoryApi.GetCustomerUsers(customerId);
            return Ok(UserSimpleDto.Map(users));
        }

        [HttpGet("customers/{customerId}/modelsOfInterest")]
        [Authorize]
        [ProducesResponseType(typeof(List<CustomerModelOfInterestDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of models", Tags = new[] { "Customers" })]
        public async Task<IActionResult> GetCustomerModelsOfInterest(Guid customerId)
        {
            var result = await _directoryApi.GetCustomerModelsOfInterest(customerId);
            return Ok(result);
        }

        [HttpGet("customers/{customerId}/modelsOfInterest/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerModelOfInterestDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a model", Tags = new[] { "Customers" })]
        public async Task<IActionResult> GetCustomerModelOfInterest(Guid customerId, Guid id)
        {
            var result = await _directoryApi.GetCustomerModelOfInterest(customerId, id);
            return Ok(result);
        }

        [HttpDelete("customers/{customerId}/modelsOfInterest/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerModelOfInterestDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Deletes a model", Tags = new[] { "Customers" })]
        public async Task<IActionResult> DeleteCustomerModelOfInterest(Guid customerId, Guid id)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManageUsers, customerId);

            await _directoryApi.DeleteCustomerModelOfInterest(customerId, id);
            return NoContent();
        }

        [HttpPost("customers/{customerId}/modelsOfInterest")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerModelOfInterestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Creates a new customer model of interest", Tags = new[] { "Customers" })]
        public async Task<ActionResult> CreateCustomerModelOfInterest([FromRoute] Guid customerId, [FromBody] CreateCustomerModelOfInterestRequest request)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManageUsers, customerId);

            var model = await GetModel(request.ModelId);
            if(model == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Model does not exist in ADT");
            }

            var createRequest = new CreateCustomerModelOfInterestApiRequest
            {
                ModelId = request.ModelId,
                Name = model.ModelDefinition?.DisplayName?.En,
                Color = request.Color,
                Text = request.Text,
                Icon = request.Icon
            };

            var result = await _directoryApi.CreateCustomerModelOfInterestAsync(customerId, createRequest);
            return Ok(result);
        }

        [HttpPut("customers/{customerId}/modelsOfInterest/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Updates a customer model of interest", Tags = new[] { "Customers" })]
        public async Task<ActionResult> UpdateCustomerModelOfInterest([FromRoute] Guid customerId, [FromRoute] Guid id, [FromBody] UpdateCustomerModelOfInterestRequest request)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManageUsers, customerId);

            var model = await GetModel(request.ModelId);
            if (model == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Model does not exist in ADT");
            }

            var updateRequest = new UpdateCustomerModelOfInterestApiRequest
            {
                ModelId = request.ModelId,
                Name = model.ModelDefinition?.DisplayName?.En,
                Color = request.Color,
                Text = request.Text,
                Icon = request.Icon
            };

            await _directoryApi.UpdateCustomerModelOfInterestAsync(customerId, id, updateRequest);
            return NoContent();
        }

        [HttpPut("customers/{customerId}/modelsOfInterest/{id}/reorder")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Updates order of the specified customer model of interest", Tags = new[] { "Customers" })]
        public async Task<ActionResult> UpdateCustomerModelOfInterestOrder([FromRoute] Guid customerId, [FromRoute] Guid id, [FromBody] UpdateCustomerModelOfInterestOrderRequest request)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManageUsers, customerId);

            var modelsOfInterest = await _directoryApi.GetCustomerModelsOfInterest(customerId);

            var currentIndex = modelsOfInterest.FindIndex(moi => moi.Id == id);
            if (currentIndex == -1)
            {
                return BadRequest("Model not found in the list.");
            }

            if (currentIndex == request.Index)
            {
                return NoContent();
            }

            var modelsOfInterestCount = modelsOfInterest.Count;
            var modelOfInterest = modelsOfInterest[currentIndex];
            modelsOfInterest.RemoveAt(currentIndex);
            modelsOfInterest.Insert(request.Index >= modelsOfInterestCount ? modelsOfInterestCount - 1 : request.Index, modelOfInterest);

            await _directoryApi.UpdateCustomerModelsOfInterestAsync(customerId,
                                                                    new UpdateCustomerModelsOfInterestApiRequest { ModelsOfInterest = modelsOfInterest });
            return NoContent();
        }

        [HttpGet("customers/{customerId}/ticketStatuses")]
        [Authorize]
        [ProducesResponseType(typeof(List<CustomerTicketStatusDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Get the available ticket statuses for the customer", Tags = new[] { "Customers" })]
        public async Task<IActionResult> GetCustomerTicketStatuses(Guid customerId)
        {
            var result = await _workflowApi.GetCustomerTicketStatus(customerId);
            return Ok(result);
        }

        [HttpPost("customers/{customerId}/ticketStatus")]
        [Authorize]
        [ProducesResponseType(typeof(List<CustomerTicketStatusDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Creates or updates ticket status", Tags = new[] { "Customers" })]
        public async Task<IActionResult> CreateOrUpdateCustomerTicketStatus(Guid customerId, [FromBody] WorkflowCreateTicketStatusRequest request)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManageUsers, customerId);

            var result = await _workflowApi.CreateOrUpdateTicketStatus(customerId, request);
            return Ok(result);
        }

        private async Task<AdtModel> GetModel(string modelId)
        {
            try
            {
                var userSites = await _userAuthorizedSitesService.GetAuthorizedSites(this.GetCurrentUserId(), Permissions.ViewSites);
                foreach (var site in userSites)
                {
                    var model = await _digitalTwinApi.GetAdtModelAsync(site.Id, modelId);
                    if (model != null)
                    {
                        return model;
                    }
                }
                return null;
            }
            catch (RestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
