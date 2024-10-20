using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.Twins
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class TenantsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDigitalTwinApiService _digitalTwinApiService;

        public TenantsController(
            IAccessControlService accessControl,
            IDigitalTwinApiService digitalTwinApiService)
        {
            _accessControl = accessControl ?? throw new ArgumentNullException(nameof(accessControl));
            _digitalTwinApiService = digitalTwinApiService ?? throw new ArgumentNullException(nameof(digitalTwinApiService));
        }
       
        [HttpGet("tenants")]
        [SwaggerOperation("Retrieves tenants information for the specified sites")]
        [Authorize]
        public async Task<ActionResult<List<TenantDto>>> GetTenants([FromQuery] List<Guid> siteIds)
        {
			await _accessControl.EnsureAccessSites(this.GetCurrentUserId(), Permissions.ViewSites, siteIds);

            return await _digitalTwinApiService.GetTenants(siteIds);
        }
    }
}
