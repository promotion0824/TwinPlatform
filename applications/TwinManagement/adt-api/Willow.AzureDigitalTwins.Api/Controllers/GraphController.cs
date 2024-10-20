using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.Model.Graph;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GraphController : Controller
    {
        private readonly ITwinsService _twinsService;

        public GraphController(ITwinsService twinsService)
        {
            _twinsService = twinsService;
        }

        [HttpPost]
        public async Task<ActionResult<TwinGraph>> GetTwinGraph([FromBody][Required] string[] twinIds)
        {
            if (!twinIds.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "Root twin ids is mandatory" });

            var graph = await _twinsService.GetTwinSystemGraph(twinIds);

            var result = TwinGraph.From(graph);
            return result;
        }
    }
}
