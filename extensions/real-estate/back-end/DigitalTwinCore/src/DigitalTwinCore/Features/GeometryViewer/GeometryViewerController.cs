using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinCore.Features.GeometryViewer
{
    [Route("admin/geometryviewer")]
    [ApiController]
    public class GeometryViewerController : ControllerBase
    {
        private readonly IGeometryViewerService _geometryViewerService;

        public GeometryViewerController(IGeometryViewerService geometryViewerService)
        {
            _geometryViewerService = geometryViewerService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Authorize]
        public async Task<ActionResult> AddGeometryViewerModel(GeometryViewerModel request)
        {
            await _geometryViewerService.AddGeometryViewerModel(request);

            return Created("admin/geometryviewer", request);
        }

        [HttpDelete]
        [Route("{urn}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> RemoveGeometryViewerModel([FromRoute]string urn)
        {
            await _geometryViewerService.RemoveGeometryViewerModel(urn);

            return NoContent();
        }

        [HttpGet]
        [Route("{urn}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<ActionResult> ExistsGeometryViewerModel([FromRoute]string urn)
        {
            var existsModel = await _geometryViewerService.ExistsGeometryViewerModel(urn);

            return existsModel ? Ok() : NotFound();
        }

        [HttpGet]
        [Route("urns/{urn}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GeometryViewerModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<ActionResult> GetGeometryViewerModelsByUrn([FromRoute]string urn)
        {
            var geometryViewerModels = await _geometryViewerService.GetGeometryViewerModelsByUrn(urn);

            if (geometryViewerModels.Any())
            {
                return Ok(geometryViewerModels);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("twins/{twinId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GeometryViewerModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<ActionResult> GetGeometryViewerModelsByTwinId([FromRoute]string twinId)
        {
            var geometryViewerModels = await _geometryViewerService.GetGeometryViewerModelsByTwinId(twinId);

            if (geometryViewerModels.Any())
            {
                return Ok(geometryViewerModels);
            }

            return NotFound();
        }
    }
}
