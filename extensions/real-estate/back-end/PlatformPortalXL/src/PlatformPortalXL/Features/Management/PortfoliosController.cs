using System;
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

namespace PlatformPortalXL.Features.Management
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

        [Obsolete("Not used in Willow App, to be deprecated")]
        [HttpGet("customers/{customerId}/portfolios/{portfolioId}")]
        [Authorize]
        [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get a portfolio with sites", Tags = new [] { "Management" })]
        public async Task<IActionResult> GetCustomerPortfolio([FromRoute] Guid customerId, [FromRoute] Guid portfolioId)
        {
            await _accessControl.EnsureAccessPortfolio(this.GetCurrentUserId(), Permissions.ViewPortfolios, portfolioId);
            var customerPortfolios = await _directoryApi.GetCustomerPortfolios(customerId, false);
            var portfolio = customerPortfolios.FirstOrDefault(x => x.Id == portfolioId);
            if (portfolio == null)
            {
                throw new ArgumentException().WithData(new { portfolioId, customerId });
            }
            var portfolioSites = await _directoryApi.GetPortfolioSites(customerId, portfolioId);
            return Ok(PortfolioDetailDto.MapFrom(portfolio, portfolioSites));
        }

        [HttpPost("customers/{customerId}/portfolios")]
        [Authorize]
        [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Creates a portfolio", Tags = new [] { "Management" })]
        public async Task<IActionResult> CreateCustomerPortfolio([FromRoute] Guid customerId, [FromBody] CreatePortfolioRequest request)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManagePortfolios, customerId);

            var validationError = new ValidationError();
            if (string.IsNullOrEmpty(request.Name))
            {
                validationError.Items.Add(new ValidationErrorItem(nameof(request.Name), "Portfolio name is required"));
            }
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var portfolio = await _directoryApi.CreateCustomerPortfolio(customerId, request.Name, request.Features);
            return Ok(PortfolioDto.MapFrom(portfolio));
        }

        [HttpPut("customers/{customerId}/portfolios/{portfolioId}")]
        [Authorize]
        [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Updates a portfolio", Tags = new[] { "Management" })]
        public async Task<IActionResult> UpdateCustomerPortfolio([FromRoute] Guid customerId, [FromRoute] Guid portfolioId, [FromBody] UpdatePortfolioRequest request)
        {
            await _accessControl.EnsureAccessCustomer(this.GetCurrentUserId(), Permissions.ManagePortfolios, customerId);

            var validationError = new ValidationError();
            if (string.IsNullOrEmpty(request.Name))
            {
                validationError.Items.Add(new ValidationErrorItem(nameof(request.Name), "Portfolio name is required"));
            }
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var portfolio = await _directoryApi.UpdateCustomerPortfolio(customerId, portfolioId, request);
            return Ok(PortfolioDto.MapFrom(portfolio));
        }
    }
}
