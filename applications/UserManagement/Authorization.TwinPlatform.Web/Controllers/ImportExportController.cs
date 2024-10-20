using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImportExportController(IImportExportManager importExportManager) : ControllerBase
{
    [Authorize]
    [HttpGet("recordTypes")]
    public string[] GetSupportedFileTypes()
    {
        return importExportManager.GetSupportedRecordTypes();
    }

    [Authorize(AppPermissions.CanExportData)]
    [HttpGet("export")]
    [Produces("application/zip")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> Export([FromQuery]string[] recordTypes)
    {
        var fileBytes = await importExportManager.ExportRecordsByTypesAsync(recordTypes);
        return File(fileBytes, "application/zip", "UserManagement.zip");
    }

    [Authorize(AppPermissions.CanImportData)]
    [HttpPost("import")]
    [Produces("application/zip")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Import([FromForm] IFormFile formFile)
    {
        var stream = formFile.OpenReadStream();
        var outputBytes = await importExportManager.ImportRecordsAsync(stream);
        stream.Close();
        await stream.DisposeAsync();
        return File(outputBytes, "application/zip", "Report.zip");
    }
}
