using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Features.SiteStructure
{
    public class SitesBaseController : ControllerBase
    {
        protected readonly ISiteApiService _siteApiService;
        protected readonly IDirectoryApiService _directoryApiService;
        protected readonly IWorkflowApiService _workflowApiService;
        protected readonly IPortfolioDashboardService _portfolioDashboardService;
        protected readonly ITimeZoneService _timeZoneService;
        protected readonly IImageUrlHelper _imageUrlHelper;

        public SitesBaseController(
            ISiteApiService siteApiService,
            IDirectoryApiService directoryApiService,
            IWorkflowApiService workflowApiService,
            IPortfolioDashboardService portfolioDashboardService,
            ITimeZoneService timeZoneService,
            IImageUrlHelper imageUrlHelper)
        {
            _siteApiService = siteApiService;
            _directoryApiService = directoryApiService;
            _workflowApiService = workflowApiService;
            _portfolioDashboardService = portfolioDashboardService;
            _timeZoneService = timeZoneService;
            _imageUrlHelper = imageUrlHelper;
        }

        protected async Task<SiteDetailDto> MapToSiteDetailAsync(Site site, bool isConnectivityViewEnabled)
        {
			var siteDirectoryCoreDB = await _directoryApiService.GetSite(site.Id);

			var siteFeatures = site.Features ?? siteDirectoryCoreDB?.Features;
            var siteSettings = await _workflowApiService.GetSiteSettings(site.Id);

            var siteDto = SiteDetailDto.Map(site, _imageUrlHelper);
            siteDto.TimeZone = _timeZoneService.GetTimeZoneType(site.TimeZoneId);
            siteDto.Features = SiteFeaturesDto.Map(siteFeatures);
            siteDto.Settings = SiteSettingsDto.Map(siteSettings);

            if (isConnectivityViewEnabled)
            {
                var siteDashboardData = await _portfolioDashboardService.GetDashboardDataForSiteAsync(site.Id);
                siteDto.IsOnline = siteDashboardData?.IsOnline;
            }

			siteDto.ArcGisLayers = ArcGisLayerDto.Map(siteDirectoryCoreDB?.ArcGisLayers);
			siteDto.WebMapId = siteDirectoryCoreDB?.WebMapId;

            return siteDto;
        }
    }
}
