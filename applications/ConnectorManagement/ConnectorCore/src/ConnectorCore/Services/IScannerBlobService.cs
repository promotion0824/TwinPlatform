namespace ConnectorCore.Services;

internal interface IScannerBlobService
{
    Task DownloadScannerDataToStream(Guid connectorId,
                                     Guid scanId,
                                     Stream targetStream);
}
