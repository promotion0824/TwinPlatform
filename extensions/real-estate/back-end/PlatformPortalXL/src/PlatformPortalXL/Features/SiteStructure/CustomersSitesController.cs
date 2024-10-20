using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Common;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Features.SiteStructure
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CustomersSitesController : SitesBaseController
    {
        private readonly IAccessControlService _accessControlService;
        private readonly IAuthService _authService;
        private readonly IAuthFeatureFlagService _featureFlagService;

        public CustomersSitesController(
            ISiteApiService siteApiService,
            IDirectoryApiService directoryApiService,
            IWorkflowApiService workflowApiService,
            IPortfolioDashboardService portfolioDashboardService,
            ITimeZoneService timeZoneService,
            IImageUrlHelper imageUrlHelper,
            IAccessControlService accessControlService,
            IAuthService authService,
            IAuthFeatureFlagService authFeatureFlagService)
            : base(siteApiService, directoryApiService, workflowApiService, portfolioDashboardService, timeZoneService, imageUrlHelper)
        {
            _accessControlService = accessControlService;
            _authService = authService;
            _featureFlagService = authFeatureFlagService;
        }

        [HttpGet("customers/{customerId}/portfolios/{portfolioId}/sites")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteDetailDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of sites belonging to the given portfolio", Tags = new[] { "Customers" })]
        public async Task<ActionResult> GetPortfolioSites([FromRoute] Guid customerId, [FromRoute] Guid portfolioId)
        {
            await _accessControlService.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ViewPortfolios, customerId);

            var customer = await _directoryApiService.GetCustomer(customerId);
            var sites = await _siteApiService.GetSites(customerId, portfolioId);

            var output = new List<SiteDetailDto>();

            foreach (var site in sites)
            {
                var dto = await MapToSiteDetailAsync(site, customer.Features.IsConnectivityViewEnabled);
                output.Add(dto);
            }

            return Ok(output);
        }

        [HttpPost("customers/{customerId}/portfolios/{portfolioId}/sites")]
        [Authorize]
        [ProducesResponseType(typeof(SiteDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Creates a site", Tags = ["Customers"])]
        public async Task<IActionResult> CreateSite([FromRoute] Guid customerId, [FromRoute] Guid portfolioId, [FromBody] CreateSiteRequest request)
        {
            await _accessControlService.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManageSites, customerId);

            var customerPortfolios = await _directoryApiService.GetCustomerPortfolios(customerId, false);
            if (customerPortfolios.All(cp => cp.Id != portfolioId))
            {
                throw new ArgumentException("Portfolio does not exist.").WithData(new { portfolioId, customerId });
            }

            var site = await _siteApiService.CreateSite(
                customerId,
                portfolioId,
                new SiteApiCreateSiteRequest
                {
                    Name = request.Name,
                    Code = request.Code,
                    Address = request.Address,
                    Suburb = request.Suburb,
                    Country = request.Country,
                    State = request.State,
                    TimeZoneId = request.TimeZoneId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    FloorCodes = request.FloorCodes,
                    Area = request.Area,
                    Type = request.Type.Value,
                    Status = request.Status.Value,
                    ConstructionYear = request.ConstructionYear,
                    SiteCode = request.SiteCode,
                    SiteContactEmail = request.SiteContactEmail,
                    SiteContactName = request.SiteContactName,
                    SiteContactPhone = request.SiteContactPhone,
                    SiteContactTitle = request.SiteContactTitle,
                    DateOpened = request.DateOpened
                });

            await _directoryApiService.CreateSite(
                customerId,
                portfolioId,
                new DirectoryApiCreateSiteRequest
                {
                    Id = site.Id,
                    Name = request.Name,
                    Code = request.Code,
                    Features = request.Features,
                    TimeZoneId = request.TimeZoneId
                });

            return Ok(SiteDetailDto.Map(site, _imageUrlHelper));
        }

        [HttpPut("customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Updates a site", Tags = ["Customers"])]
        public async Task<IActionResult> UpdateSite([FromRoute] Guid customerId, [FromRoute] Guid portfolioId, [FromRoute] Guid siteId, UpdateSiteRequest request)
        {
            if (request.Status == SiteStatus.Deleted)
            {
                bool isUserInCustomerRole = false;
                if (_featureFlagService.IsFineGrainedAuthEnabled)
                {
                    isUserInCustomerRole = await _authService.HasPermission<CanActAsCustomerAdmin>(User);
                }
                else
                {
                    isUserInCustomerRole = await _accessControlService.IsUserInCustomerAdminRole(this.GetCurrentUserId(), customerId);
                }
                if (!isUserInCustomerRole)
                {
                    return Unauthorized();
                }
            }

            await _accessControlService.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);
            var customerPortfolios = await _directoryApiService.GetCustomerPortfolios(customerId, false);
            if (customerPortfolios.All(cp => cp.Id != portfolioId))
            {
                throw new ArgumentException("Portfolio does not exist.").WithData(new { portfolioId, customerId });
            }

            var site = await _siteApiService.UpdateSite(customerId, portfolioId, siteId, new SiteApiUpdateSiteRequest
            {
                Name = request.Name,
                Address = request.Address,
                Suburb = request.Suburb,
                Country = request.Country,
                State = request.State,
                TimeZoneId = request.TimeZoneId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Area = request.Area,
                Type = request.Type.Value,
                Status = request.Status.Value,
                ConstructionYear = request.ConstructionYear,
                SiteCode = request.SiteCode,
                SiteContactEmail = request.SiteContactEmail,
                SiteContactName = request.SiteContactName,
                SiteContactPhone = request.SiteContactPhone,
                SiteContactTitle = request.SiteContactTitle,
                DateOpened = request.DateOpened
            });

            await _directoryApiService.UpdateSite(customerId, portfolioId, site.Id, new DirectoryApiUpdateSiteRequest
            {
                Name = request.Name,
                Features = request.Features,
                TimeZoneId = request.TimeZoneId,
                Status = request.Status.Value,
                ArcGisLayers = request.ArcGisLayers,
				WebMapId = request.WebMapId
            });

            await _workflowApiService.UpsertSiteSettings(siteId, new WorkflowApiUpsertSiteSettingsRequest
            {
                InspectionDailyReportWorkgroupId = request.Settings?.InspectionDailyReportWorkgroupId
            });
            return NoContent();
        }
    }
}
