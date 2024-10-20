using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Import;

namespace SiteCore.Controllers
{
    [ApiController]
    public class ImportController : Controller
    {
        private readonly ISiteCoreDataImportService _importService;

        public ImportController(ISiteCoreDataImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("import")]
        [Authorize]
        public async Task<IActionResult> ImportData(IFormFile file)
        {
            await _importService.PerformImportAsync(file);
            return Ok();
        }
    }
}
