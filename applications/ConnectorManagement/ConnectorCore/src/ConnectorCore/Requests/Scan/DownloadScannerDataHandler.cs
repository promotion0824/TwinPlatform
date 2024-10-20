namespace ConnectorCore.Requests.Scan;

using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class DownloadScannerDataHandler
{
    public static async Task<Results<FileStreamHttpResult, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId, [FromRoute] Guid scanId, [FromServices] IScannerBlobService scannerBlobService)
    {
        var stream = new MemoryStream();
        await scannerBlobService.DownloadScannerDataToStream(connectorId, scanId, stream);
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Length == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Stream(stream, "application/zip", "scan.zip");
    }
}
