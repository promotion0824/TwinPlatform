namespace ConnectorCore.Services;

using System.Diagnostics.CodeAnalysis;
using ConnectorCore.Models;

/// <summary>
/// This service is a temp solution for MS key rotations, the blob storage part
/// We should have a more general service when we are ready to use client specific storage account instead of
/// a regional shared one.
/// </summary>
internal interface IMSBlobStorageService
{
    bool TryGetMSBlobStorage(Guid connectorId, [NotNullWhen(true)] out ScannerBlobStorageOptions options);
}
