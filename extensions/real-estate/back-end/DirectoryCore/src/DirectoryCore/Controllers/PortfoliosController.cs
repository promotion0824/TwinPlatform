using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Dto;
using DirectoryCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Directory.Models;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class PortfoliosController : ControllerBase
    {
        private readonly ICustomersService _customersService;
        private readonly ISitesService _sitesService;
        private readonly Services.IAuthorizationService _authorizationService;

        public PortfoliosController(
            ICustomersService customersService,
            ISitesService sitesService,
            Services.IAuthorizationService _authorizationService
        )
        {
            _customersService = customersService;
            _sitesService = sitesService;
            this._authorizationService = _authorizationService;
        }

        [Authorize]
        [HttpGet("customers/{customerId}/portfolios")]
        [ProducesResponseType(typeof(List<PortfolioDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerPortfolios(
            [FromRoute] Guid customerId,
            [FromQuery] bool includeSites
        )
        {
            var portfolios = await _customersService.GetPortfolios(customerId, includeSites);
            return Ok(PortfolioDto.MapFrom(portfolios));
        }

        [Authorize]
        [HttpPost("customers/{customerId}/portfolios")]
        [ProducesResponseType(typeof(PortfolioDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCustomerPortfolio(
            [FromRoute] Guid customerId,
            [FromBody] CreatePortfolioRequest request
        )
        {
            var portfolio = await _customersService.CreatePortfolio(
                customerId,
                request.Name,
                request.Features
            );
            return Ok(PortfolioDto.MapFrom(portfolio));
        }

        [Authorize]
        [HttpPut("customers/{customerId}/portfolios/{portfolioId}")]
        [ProducesResponseType(typeof(PortfolioDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateCustomerPortfolio(
            [FromRoute] Guid customerId,
            [FromRoute] Guid portfolioId,
            [FromBody] UpdatePortfolioRequest request
        )
        {
            var portfolio = await _customersService.UpdatePortfolio(
                customerId,
                portfolioId,
                request.Name,
                request.Features
            );
            return Ok(PortfolioDto.MapFrom(portfolio));
        }

        [Authorize]
        [HttpDelete("customers/{customerId}/portfolios/{portfolioId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteCustomerPortfolio(
            [FromRoute] Guid customerId,
            [FromRoute] Guid portfolioId
        )
        {
            var sites = await _sitesService.GetPortfolioSites(customerId, portfolioId);
            if (sites.Any())
            {
                throw new BadRequestException("Cannot delete the portfolio which has sites.");
            }
            await _authorizationService.DeleteAssignmentsByResource(
                RoleResourceType.Portfolio,
                portfolioId
            );
            var success = await _customersService.DeletePortfolio(customerId, portfolioId);
            if (!success)
            {
                throw new ResourceNotFoundException("portfolio", portfolioId);
            }
            return NoContent();
        }
    }
}
