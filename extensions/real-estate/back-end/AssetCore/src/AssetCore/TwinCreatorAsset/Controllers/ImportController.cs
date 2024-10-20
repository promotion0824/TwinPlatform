using System.Threading.Tasks;
using AssetCoreTwinCreator.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AssetCoreTwinCreator.Controllers
{
    [ApiController]
    public class ImportController : Controller
    {
        private readonly IAssetMappingImportService _importService;

        public ImportController(IAssetMappingImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("import")]
        [Authorize]
        [SwaggerOperation(OperationId = "import", Tags = new string[] { "TwinCreator" })]
        public async Task<IActionResult> ImportData(IFormFile file)
        {
            await _importService.PerformImportAsync(file);
            return Ok();
        }
    }
}
