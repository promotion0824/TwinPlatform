using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Controllers
{
    [ApiController]
    public class ScopesController : ControllerBase
    {
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;

        public ScopesController(IDigitalTwinServiceProvider digitalTwinServiceFactory)
        {
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
        }

        /// <summary>
        /// Get list of Twins by ScopeId
        /// </summary>
        /// <param name="request">list of the siteIds for adt settings and the scopeId</param>
        /// <returns>Return list of the twins by scopeId</returns>
        [HttpPost("scopes/sites")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TwinDto>>> GetSites([FromBody] GetScopeSitesRequest request)
        {
            if(request?.Scope?.SiteIds==null || !request.Scope.SiteIds.Any())
                throw new BadRequestException("SiteIds is required.");

            var twinService = await _digitalTwinServiceFactory.GetForSitesAsync(request.Scope.SiteIds);

            var twinSites = await twinService.GetSitesByScopeAsync(request.Scope.DtId);

            return Ok(TwinDto.MapFrom(twinSites));
        }
    }
}
