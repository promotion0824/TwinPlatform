using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Controllers.Responses;
using DirectoryCore.Dto;
using DirectoryCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeZoneConverter;
using Willow.Batch;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SitesController : ControllerBase
    {
        private readonly Services.IAuthorizationService _authorizationService;
        private readonly ISitesService _sitesService;
        private readonly IUsersService _usersService;

        public SitesController(
            ISitesService sitesService,
            IUsersService usersService,
            Services.IAuthorizationService authorizationService
        )
        {
            _sitesService = sitesService;
            _usersService = usersService;
            _authorizationService = authorizationService;
        }

        [Authorize]
        [HttpGet("sites/{siteId}")]
        [ProducesResponseType(typeof(SiteDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSite(Guid siteId)
        {
            var site = await _sitesService.GetSite(siteId);
            if (site == null)
            {
                throw new ResourceNotFoundException("site", siteId);
            }
            return Ok(SiteDto.MapFrom(site));
        }

        [Authorize]
        [HttpGet("customers/{customerId}/portfolios/{portfolioId}/sites")]
        [ProducesResponseType(typeof(List<SiteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPortfolioSites(
            [FromRoute] Guid customerId,
            [FromRoute] Guid portfolioId
        )
        {
            var sites = await _sitesService.GetPortfolioSites(customerId, portfolioId);
            return Ok(SiteDto.MapFrom(sites));
        }

        [Authorize]
        [HttpPost("customers/{customerId}/portfolios/{portfolioId}/sites")]
        [ProducesResponseType(typeof(SiteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSite(
            [FromRoute] Guid customerId,
            [FromRoute] Guid portfolioId,
            [FromBody] CreateSiteRequest request
        )
        {
            if (request.Id == Guid.Empty)
            {
                throw new BadRequestException("Site id must be provided.");
            }

            try
            {
                TZConvert.GetTimeZoneInfo(request.TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new BadRequestException($"Unknown timezone id {request.TimeZoneId}");
            }

            var site = await _sitesService.CreateSite(customerId, portfolioId, request);
            return Ok(SiteDto.MapFrom(site));
        }

        [Authorize]
        [HttpPut("customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSite(
            [FromRoute] Guid customerId,
            [FromRoute] Guid portfolioId,
            [FromRoute] Guid siteId,
            [FromBody] UpdateSiteRequest updateSiteRequest
        )
        {
            var site = await _sitesService.GetSite(siteId);
            if (site == null)
            {
                throw new ResourceNotFoundException("site", siteId);
            }

            try
            {
                TZConvert.GetTimeZoneInfo(updateSiteRequest.TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new BadRequestException(
                    $"Unknown timezone id {updateSiteRequest.TimeZoneId}"
                );
            }

            // "IsOccupancyEnabled" should not be updated
            updateSiteRequest.Features.IsOccupancyEnabled = site.Features.IsOccupancyEnabled;

            await _sitesService.UpdateSite(customerId, portfolioId, siteId, updateSiteRequest);
            return Ok(SiteDto.MapFrom(site));
        }

        [Authorize]
        [HttpPut("customers/{customerId}/sites/{siteId}/connectors/{connectorId}/account")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateConnectorAccountUser(
            [FromRoute] Guid customerId,
            [FromRoute] Guid siteId,
            [FromRoute] Guid connectorId,
            [FromBody] CreateConnectorAccountRequest request
        )
        {
            await _usersService.CreateOrUpdateConnectorAccount(
                customerId,
                siteId,
                connectorId,
                request.Password
            );
            return NoContent();
        }

        [Authorize]
        [HttpGet("sites")]
        [ProducesResponseType(typeof(List<SiteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSites([FromQuery] bool? isInspectionEnabled = null)
        {
            var sites = await _sitesService.GetSites(isInspectionEnabled);
            return Ok(SiteDto.MapFrom(sites));
        }

        [Authorize]
        [HttpGet("sites/customer/{customerId}")]
        [ProducesResponseType(typeof(List<SiteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSitesByCustomer(
            Guid customerId,
            [FromQuery] bool? isInspectionEnabled = null,
            [FromQuery] bool? isTicketingDisabled = null,
            [FromQuery] bool? isScheduledTicketsEnabled = null
        )
        {
            var sites = await _sitesService.GetSitesByCustomer(
                customerId,
                isInspectionEnabled,
                isTicketingDisabled,
                isScheduledTicketsEnabled
            );
            return Ok(SiteDto.MapFrom(sites));
        }

        [Authorize]
        [HttpGet("users/{userId}/sites")]
        [ProducesResponseType(typeof(IList<SiteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPermittedSites(
            [FromRoute] Guid userId,
            [FromQuery] string permissionId
        )
        {
            if (string.IsNullOrEmpty(permissionId))
            {
                throw new BadRequestException("'permissionId' is required.");
            }
            var user = await _usersService.GetUser(userId, Domain.UserType.Customer);
            if (user == null)
            {
                throw new ResourceNotFoundException("user", userId);
            }
            var sites = await _authorizationService.GetUserSites(
                user.CustomerId,
                userId,
                permissionId
            );
            return Ok(SiteDto.MapFrom(sites));
        }

        [Authorize]
        [HttpPost("users/{userId}/sites/paged")]
        [ProducesResponseType(typeof(BatchDto<SiteMiniDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPermittedSitesPaged(
            [FromRoute] Guid userId,
            [FromBody] BatchRequestDto request
        )
        {
            var user = await _usersService.GetUser(userId, Domain.UserType.Customer);
            if (user == null)
            {
                throw new ResourceNotFoundException("user", userId);
            }

            var sites = await _authorizationService.GetUserSites(
                user.CustomerId,
                userId,
                "view-sites"
            );

            var siteMiniDtos = await sites
                .AsQueryable()
                .FilterBy(request.FilterSpecifications)
                .SortBy(request.SortSpecifications)
                .Paginate(request.Page, request.PageSize, SiteMiniDto.MapFrom);

            return Ok(siteMiniDtos);
        }
    }
}
