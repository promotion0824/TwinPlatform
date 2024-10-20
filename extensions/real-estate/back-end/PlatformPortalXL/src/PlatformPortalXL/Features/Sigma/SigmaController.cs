using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.Sigma;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.Auth.Permissions;

namespace PlatformPortalXL.Features.Sigma
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SigmaController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ISigmaService _sigmaService;
        private readonly ISiteService _siteService; // temporary needed for scope authorization

        public SigmaController(IAccessControlService accessControl, IDirectoryApiService directoryApi, ISigmaService sigmaService, ISiteService siteService)
        {
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _sigmaService = sigmaService;
            _siteService = siteService;
        }

        [HttpGet("sigma/sites/{siteId}/embedurl")]
        [Authorize]
        [Obsolete("Use corresponding POST endpoint")]
        [SwaggerOperation("Get site's Sigma embed url", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SigmaEmbedUrlDto>> GetEmbedUrlBySiteId(
            [FromRoute] Guid siteId,
            [FromQuery] Guid reportId,
            [FromQuery] string reportName,
            [FromQuery] DateTime? start,    
            [FromQuery] DateTime? end,
            [FromQuery] string[] tenantIds)
        {
            var currentUserId = this.GetCurrentUserId();

            await _accessControl.EnsureAccessSite(currentUserId, Permissions.ViewSites, siteId);

            var currentUser = await _directoryApi.GetUser(currentUserId);
            var customer = await _directoryApi.GetCustomer(currentUser.CustomerId);

            var embedUrls = await _sigmaService.GetSiteEmbedUrls(currentUser.Id, customer.SigmaConnectionId, siteId, new WidgetRequest
            {
                ScopeId = siteId.ToString(),
                ReportId = reportId,
                ReportName = reportName,
                Start = start,
                End = end,
                TenantIds = tenantIds
            });

            return new SigmaEmbedUrlDto { Url = embedUrls.FirstOrDefault()?.Url ?? string.Empty };
        }

        [HttpGet("sigma/portfolios/{portfolioId}/embedurl")]
        [Authorize]
        [Obsolete("Use corresponding POST endpoint")]
        [SwaggerOperation("Get portfolio's Sigma embed url", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SigmaEmbedUrlDto>> GetEmbedUrlByPortfolioId(
                [FromRoute] Guid portfolioId,
                [FromQuery] Guid reportId,
                [FromQuery] string reportName,
                [FromQuery] Guid customerId,
                [FromQuery] DateTime? start,
                [FromQuery] DateTime? end)
        {
            var currentUserId = this.GetCurrentUserId();

            await _accessControl.EnsureAccessPortfolio(currentUserId, new CanViewReports(), Permissions.ViewPortfolios, portfolioId);

            var customer = await _directoryApi.GetCustomer(customerId);

            var embedUrls = await _sigmaService.GetPortfolioEmbedUrls(currentUserId, customer.SigmaConnectionId, portfolioId, new WidgetRequest
            {
                CustomerId = customerId,
                ReportId = reportId,
                ReportName = reportName,
                Start = start,
                End = end
            });

            return new SigmaEmbedUrlDto { Url = embedUrls.FirstOrDefault()?.Url ?? string.Empty };
        }

        [HttpPost("sigma/scopes/{scopeId}/embedurl")]
        [Authorize]
        [SwaggerOperation("Get scope's Sigma embed url using a POST", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<SigmaEmbedUrlDto>> PostGetEmbedUrlByScopeId([FromRoute] string scopeId, [FromBody] WidgetRequest request)
        {
            var currentUserId = this.GetCurrentUserId();

            if (Guid.TryParse(scopeId, out Guid portfolioId))
            {
                await _accessControl.EnsureAccessPortfolio(currentUserId, new CanViewReports(), Permissions.ViewPortfolios, portfolioId);
            }
            else
            {
                await _siteService.GetAuthorizedSiteIds(currentUserId, scopeId);
            }

            var currentUser = await _directoryApi.GetUser(currentUserId);
            var customer = await _directoryApi.GetCustomer(currentUser.CustomerId);

            var embedUrls = await _sigmaService.GetScopeEmbedUrls(currentUser.Id, customer.SigmaConnectionId, scopeId, request);

            return new SigmaEmbedUrlDto { Url = embedUrls.FirstOrDefault()?.Url ?? string.Empty };
        }

        [HttpPost("sigma/sites/{siteId}/embedurl")]
		[Authorize]
		[SwaggerOperation("Get site's Sigma embed url using a POST", Tags = new[] { "Dashboard" })]
		public async Task<ActionResult<SigmaEmbedUrlDto>> PostGetEmbedUrlBySiteId([FromRoute] Guid siteId, [FromBody] WidgetRequest request)
		{
			var currentUserId = this.GetCurrentUserId();

			await _accessControl.EnsureAccessSite(currentUserId, Permissions.ViewSites, siteId);

			var currentUser = await _directoryApi.GetUser(currentUserId);
			var customer = await _directoryApi.GetCustomer(currentUser.CustomerId);

			var embedUrls = await _sigmaService.GetSiteEmbedUrls(currentUser.Id, customer.SigmaConnectionId, siteId, request);

			return new SigmaEmbedUrlDto { Url = embedUrls.FirstOrDefault()?.Url ?? string.Empty };
		}

        [HttpPost("sigma/portfolios/{portfolioId}/embedurls")]
        [Authorize]
        [SwaggerOperation("Get portfolios Sigma embed urls", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<List<SigmaEmbedUrlDto>>> GetEmbedUrlByPortfolioId([FromRoute] Guid portfolioId, [FromBody] WidgetRequest request)
        {
            var currentUserId = this.GetCurrentUserId();

            await _accessControl.EnsureAccessPortfolio(currentUserId, new CanViewReports(), Permissions.ViewPortfolios, portfolioId);

            var currentUser = await _directoryApi.GetUser(currentUserId);
            var customer = await _directoryApi.GetCustomer(currentUser.CustomerId);

            var embedUrls = await _sigmaService.GetPortfolioEmbedUrls(currentUser.Id, customer.SigmaConnectionId, portfolioId, request);

            return Ok(embedUrls.Where(x => x.EmbedLocation != SigmaService.MetadataConstants.EmbedLocations.ReportsTab));
        }
    }
}
