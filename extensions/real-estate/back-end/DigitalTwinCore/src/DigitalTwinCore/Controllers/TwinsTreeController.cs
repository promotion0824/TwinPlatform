using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;
using DigitalTwinCore.Configs;
using DigitalTwinCore.Services.Cacheless;

namespace DigitalTwinCore.Controllers
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    public class TwinsTreeController : ControllerBase
    {
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;
        private readonly IDigitalTwinService _digitalTwinService;
        private readonly IResiliencePipelineService _resiliencePipelineService;
        private readonly SingleTenantOptions _singleTenantOptions;

        public TwinsTreeController(
            IDigitalTwinServiceProvider digitalTwinServiceFactory,
            IDigitalTwinService digitalTwinService,
            IResiliencePipelineService resiliencePipelineService,
            IOptions<SingleTenantOptions> singleTenantOptions)
        {
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
            _digitalTwinService = digitalTwinService;
            _resiliencePipelineService = resiliencePipelineService;
            _singleTenantOptions = singleTenantOptions.Value;
        }


        /// <summary>
        /// Get twins in tree form
        /// Copied from the ADT API /tree endpoint and will be removed once we switch to single-tenant.
        /// </summary>
        /// <param name="request">
        ///     Request body to get twins in tree form
        ///     <param name="siteId">Site Id</param>
        ///     <param name="modelIds">Target model ids
        ///     <br/>             Default Ids : ["dtmi:com:willowinc:Building;1", "dtmi:com:willowinc:Substructure;1","dtmi:com:willowinc:OutdoorArea;1"] will be used when modelIds is not supplied</param>
        ///     <param name="outgoingRelationships">List of relationship types to be considered for traversal.
        ///     <br/>             Default Values : ["isPartOf", "locatedIn"] will be used when relationshipsToTraverse is not supplied</param>
        ///     <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        ///     <param name="exactModelMatch">Indicates if model filter must be exact match</param>
        /// </param>
        /// <returns>Nested twins with target models. Full tree is returned following relationships. If any twin has more than one parent, it only will be assigned to a single root</returns>
        [HttpPost("twins/tree")]
        [MapToApiVersion("1.0")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NestedTwinDto>>> GetTree([FromBody] GetTwinsTreeRequest request)
        {
            if (request?.SiteIds == null || !request.SiteIds.Any())
                throw new BadRequestException("SiteIds is required.");

            var service = await _resiliencePipelineService.ExecuteAsync(async _ =>  await _digitalTwinServiceFactory.GetForSitesAsync(request.SiteIds));

            // set default values
            if (request.OutgoingRelationships == null || !request.OutgoingRelationships.Any())
            {
                request.OutgoingRelationships = new List<string> { "isPartOf", "locatedIn" };
            }

            var tree = await service.GetTreeAsync(request.ModelIds, request.OutgoingRelationships, request.IncomingRelationships,request.ExactModelMatch);

            return Ok(NestedTwinDto.MapFrom(tree));
        }

        /// <summary>
        /// Get twins in tree form. Single-tenant cached version.
        /// </summary>
        /// <returns>
        /// Nested twins with target models. Full tree is returned following relationships.
        /// If any twin has more than one parent, it only will be assigned to a single root
        /// </returns>
        [HttpGet("v{version:apiVersion}/twins/tree")]
        [MapToApiVersion("2.0")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NestedTwinDto>>> GetTreeV2()
        {
            if (!(_singleTenantOptions?.IsSingleTenant == true))
            {
                throw new BadRequestException("This endpoint is only available in single-tenant mode.");
            }
            var tree = await _resiliencePipelineService.ExecuteAsync(async _ =>
                await (_digitalTwinService as CachelessAdtService).GetScopeTreeAsync());
            return Ok(NestedTwinDto.MapFrom(tree));
        }
    }
}
