using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Swashbuckle.AspNetCore.Annotations;

using Willow.Api.DataValidation;
using Willow.Common;
using Willow.Directory.Models;
using Willow.ExceptionHandling.Exceptions;
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.Directory
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class PortfoliosController : ControllerBase
    {
        private readonly IDirectoryApiService _directoryApi;
        private readonly IAccessControlService _accessControl;

        public PortfoliosController(IDirectoryApiService directoryApi, IAccessControlService accessControl)
        {
            _directoryApi = directoryApi;
            _accessControl = accessControl;
        }

        [HttpGet("me/portfolios")]
        [Authorize]
        [ProducesResponseType(typeof(List<PortfolioDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of portfolios which the signed-in user can access", Tags = new [] { "Users" })]
        public async Task<ActionResult> GetPortfolios()
        {
            var userId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(userId);
            var allPortfolios = await _directoryApi.GetCustomerPortfolios(user.CustomerId, includeSites:true);
            var userPortfolios = new List<Portfolio>();
            foreach (var portfolio in allPortfolios)
            {
                var canAccess = await _accessControl.CanAccessPortfolio(userId, Permissions.ViewPortfolios, portfolio.Id);
                if (canAccess)
                {
                    userPortfolios.Add(portfolio);
                }
            }
            return Ok(PortfolioDto.MapFrom(userPortfolios));
        }

        [HttpDelete("customers/{customerId}/portfolios/{portfolioId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Delete the portfolio", Tags = new [] { "Customers" })]
        public async Task<ActionResult> DeletePortfolio([FromRoute] Guid customerId, [FromRoute] Guid portfolioId)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManagePortfolios, customerId);
            var usedSites = await _directoryApi.GetPortfolioSites(customerId, portfolioId);
            if (usedSites.Any())
            {
                var validationError = new ValidationError();
                validationError.Items.Add(new ValidationErrorItem(null, "Cannot delete the portfolio which has sites."));
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }
            await _directoryApi.DeleteCustomerPortfolio(customerId, portfolioId);
            return NoContent();
        }

        [HttpGet("customers/{customerId}/portfolios/{portfolioId}/users")]
        [Authorize]
        [ProducesResponseType(typeof(List<PortfolioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Get list of all portfolio users", Tags = new [] { "Users" })]
        public async Task<IActionResult> GetPortfolioUsers([FromRoute] Guid customerId, [FromRoute] Guid portfolioId)
        {
            var userId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(userId);
            if (customerId != user.CustomerId)
            {
                throw new NotFoundException().WithData(new { customerId });
            }
            var allPortfolios = await _directoryApi.GetCustomerPortfolios(user.CustomerId, false);
            var portfolio = allPortfolios.FirstOrDefault(x => x.Id == portfolioId);
            if (portfolio == null)
            {
                throw new NotFoundException().WithData(new { portfolioId });
            }
            var canAccess = await _accessControl.CanAccessPortfolio(userId, Permissions.ViewPortfolios, portfolio.Id);
            if (!canAccess)
            {
                throw new UnauthorizedAccessException().WithData(new { userId, Permissions.ViewPortfolios, RoleResourceType.Portfolio, portfolioId });
            }
            var users = await _directoryApi.GetPortfolioUsers(portfolio.Id);
            return Ok(UserSimpleDto.Map(users));
        }
    }
}
