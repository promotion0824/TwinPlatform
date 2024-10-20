namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Blobs.Specialized;
    using ConnectorCore.Data;
    using ConnectorCore.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    internal class ScannerBlobService : IScannerBlobService
    {
        private readonly IMSBlobStorageService msBlobStorageService;
        private readonly ScannerBlobStorageOptions options;
        private readonly AzureBlobService azureBlobService;
        private readonly IConnectorCoreDbContext dbContext;
        private readonly ILogger<ScannerBlobService> logger;

        public ScannerBlobService(IOptions<ScannerBlobStorageOptions> options,
                                  IConnectorCoreDbContext dbContext,
                                  AzureBlobService azureBlobService,
                                  IMSBlobStorageService msBlobStorageService,
                                  ILogger<ScannerBlobService> logger)
        {
            this.dbContext = dbContext;
            this.msBlobStorageService = msBlobStorageService;
            this.logger = logger;
            this.options = options.Value;
            this.azureBlobService = azureBlobService;
        }

        public async Task DownloadScannerDataToStream(Guid connectorId,
                                                      Guid scanId,
                                                      Stream targetStream)
        {
            var scan = await dbContext.Scans.FirstOrDefaultAsync(x => x.Id == scanId);

            var path = $"{connectorId}/{scanId}/";
            var result = await DownloadFiles(path,
                                             targetStream,
                                             scan.ErrorMessage,
                                             connectorId);

            // Fallback for OPC connectors. If nothing found at the new path - try the original OPC path.
            if (!result)
            {
                path = $"{connectorId}/{scanId}.csv";
                await DownloadFiles(path,
                                    targetStream,
                                    scan.ErrorMessage);
            }
        }

        private static async Task AddZippedErrors(ZipArchive archive, string errorMessages)
        {
            var errorFileEntry = archive.CreateEntry("errors.txt");
            await using var errorEntryStream = errorFileEntry.Open();
            await using var streamWriter = new StreamWriter(errorEntryStream);
            await streamWriter.WriteAsync(errorMessages);
            await streamWriter.FlushAsync();
        }

        private async Task<bool> DownloadFiles(string path,
                                               Stream stream,
                                               string errorMessages,
                                               Guid? connectorId = null)
        {
            var options = this.options;

            // check if the connector is a MS connector for key rotation, should make general for all other connectors
            // when we have blob storage separations for each client
            if (connectorId != null && msBlobStorageService.TryGetMSBlobStorage(connectorId.Value, out var result))
            {
                logger.LogInformation($"Use MS storage account, location {this.options.StorageAccountName}");
                options = result;
            }

            var importContainer = await azureBlobService.GetContainerClient("scannercsv", options);
            var blobItems = await azureBlobService.GetBlobsAsync(importContainer, path);

            // Check if there's already a zip file at the path.
            if (blobItems.Count == 1 &&
                blobItems.First()
                         .Name
                         .EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var item in blobItems)
                {
                    var blob = importContainer.GetBlobClient(item.Name);

                    if (!await blob.ExistsAsync())
                    {
                        return false;
                    }

                    await blob.DownloadToAsync(stream);
                }
            }
            else
            {
                return await DownloadAndZipFiles(importContainer,
                                                 blobItems,
                                                 stream,
                                                 errorMessages);
            }

            return true;
        }

        private static async Task<bool> DownloadAndZipFiles(BlobContainerClient importContainer,
                                                            IList<BlobItem> blobItems,
                                                            Stream stream,
                                                            string errorMessages)
        {
            using var archive = new ZipArchive(stream,
                                               ZipArchiveMode.Update,
                                               true);

            if (!string.IsNullOrWhiteSpace(errorMessages) && archive.GetEntry("errors.txt") == null)
            {
                await AddZippedErrors(archive, errorMessages);
            }

            if (!blobItems.Any())
            {
                return false;
            }

            foreach (var item in blobItems)
            {
                var blob = importContainer.GetBlockBlobClient(item.Name);

                if (!await blob.ExistsAsync())
                {
                    return false;
                }

                var fileEntry = archive.CreateEntry(blob.Uri.Segments.Last());
                await using var entryStream = fileEntry.Open();
                await blob.DownloadToAsync(entryStream);
            }

            return true;
        }

        public static class WellKnownConnectorTypeIds
        {
            public static readonly Guid Modbus = Guid.Parse("028D6FCD-DF1A-4F6D-BDE1-4617A7F7A96B");
            public static readonly Guid Bacnet = Guid.Parse("8FD6BE9C-1E67-4196-82F0-6DC6B48F1503");
            public static readonly Guid OpcUa = Guid.Parse("0F4D657A-A908-4F69-B214-79C0FF5B1E95");
            public static readonly Guid OpcDa = Guid.Parse("2112FC9C-F72F-4F5F-A342-9E4A499C452E");
        }
    }
}
