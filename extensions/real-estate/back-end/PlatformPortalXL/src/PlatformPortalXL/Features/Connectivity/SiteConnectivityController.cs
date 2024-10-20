using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.Assets;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.Connectivity
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SiteConnectivityController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IFloorsApiService _floorApi;
        private readonly IPortfolioDashboardService _portfolioDashboard;
        private readonly IDigitalTwinAssetService _digitalTwinService;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;

        public SiteConnectivityController(
            IAccessControlService accessControl, 
            IDirectoryApiService directoryApi, 
            IFloorsApiService floorApi, 
            IPortfolioDashboardService portfolioDashboard,
            IDigitalTwinAssetService digitalTwinService,
            IUserAuthorizedSitesService userAuthorizedSitesService)
        {
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _floorApi = floorApi;
            _portfolioDashboard = portfolioDashboard;
            _digitalTwinService = digitalTwinService;
            _userAuthorizedSitesService = userAuthorizedSitesService;
        }

        [HttpGet("connectivity")]
        [Authorize]
        [SwaggerOperation("Gets sites connectivity", Tags = new [] { "Connectivity" })]
        public async Task<ActionResult<ConnectivitySummaryResponse>> GetConnectivitySummary()
        {
            var userId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(userId);

            var allowedSites = await _userAuthorizedSitesService.GetAuthorizedSites(userId, Permissions.ViewSites);

            var siteStatus = await _portfolioDashboard.GetDashboardDataForUserAsync(user, allowedSites);

            var result = ConnectivitySummaryResponse.MapFrom(siteStatus);

            return Ok(result);
        }

        [HttpGet("connectivity/sites/{siteId}/connectors")]
        [Authorize]
        [SwaggerOperation("Gets connectors of a given site", Tags = new [] { "Connectivity" })]
        public async Task<IActionResult> GetSiteConnectors([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var dashboardData = await _portfolioDashboard.GetDashboardDataForSiteAsync(siteId);
            var result = new List<ConnectorConnectivityDto>();

            if (dashboardData.Status != ServiceStatus.NotOperational)
            {
                foreach (var gateway in dashboardData.Gateways)
                {
                    foreach (var connector in gateway.Connectors)
                    {
                        result.Add(new ConnectorConnectivityDto
                        {
                            Id = connector.ConnectorId,
                            Name = connector.Name,
                            Status = connector.Status,
                            GatewayStatus = gateway.Status,
                            ErrorCount = connector.ErrorCount,
                            History = connector.Status != ServiceStatus.NotOperational
                                ? ConnectorConnectivityDataPointDto.MapFrom(await _portfolioDashboard.GetConnectivityHistoryForConnectorAsync(connector.ConnectorId))
                                : new List<ConnectorConnectivityDataPointDto>()
                        });
                    }
                }
            }

            return Ok(result);
        }

        [HttpGet("connectivity/sites/{siteId}/equipments")]
        [Authorize]
        [SwaggerOperation("Gets equipments of a given site", Tags = new [] { "Connectivity" })]
        public async Task<IActionResult> GetSiteEquipments([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var assets = await _digitalTwinService.GetAssetsAsync(siteId, null, null, true, null, null);
            var floors = await _floorApi.GetFloorsAsync(siteId, false);
            var assetDtos = new List<AssetSimpleDto>();
            foreach (var asset in assets)
            {
                var assetDto = AssetSimpleDto.MapFromModel(asset);
                assetDto.FloorCode = asset.FloorId.HasValue
                    ? floors.FirstOrDefault(x => x.Id == asset.FloorId.Value)?.Code ?? string.Empty
                    : string.Empty;
                assetDtos.Add(assetDto);
            }
            return Ok(assetDtos);
        }
    }
}
