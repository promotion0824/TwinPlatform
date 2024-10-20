namespace Connector.Nunit.Tests.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture.NUnit3;
    using Azure;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using ConnectorCore.Repositories;
    using ConnectorCore.Services;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using NUnit.Framework;

    public class ScannerBlobServiceTests
    {
        [Test]
        [AutoData]
        public async Task DownloadScannerDataToStream_RandomSuccess_ShouldReturnStream(
            ScanEntity scan,
            ConnectorEntity connector,
            ScannerBlobStorageOptions options)
        {
            connector.ConnectorTypeId = ScannerBlobService.WellKnownConnectorTypeIds.Bacnet;
            var mockOptions = Options.Create(options);
            var mockScanRepo = Substitute.For<IScansRepository>();
            mockScanRepo.GetById(scan.Id)
                        .Returns(scan);
            var containerName = "scannercsv";
            await using var targetStream = new MemoryStream();
            var mockAzureWrapper = Substitute.For<IAzureBlobService>();
            mockAzureWrapper.GetContainerClient(containerName, Arg.Any<ScannerBlobStorageOptions>())
                            .Returns(new FakeContainer());
            var msBlobStorageServiceMock = Substitute.For<IMSBlobStorageService>();
            msBlobStorageServiceMock.TryGetMSBlobStorage(Arg.Any<Guid>(), out Arg.Any<ScannerBlobStorageOptions>())
                                    .Returns(false);
            var service = new ScannerBlobService(mockOptions,
                                                 mockScanRepo,
                                                 mockAzureWrapper,
                                                 msBlobStorageServiceMock,
                                                 new NullLogger<ScannerBlobService>());

            await service.DownloadScannerDataToStream(connector.Id,
                                                      scan.Id,
                                                      targetStream);

            await mockScanRepo.Received(1).GetById(scan.Id);

            // errors.txt
            using var zip = new ZipArchive(targetStream);
            zip.Entries.Count.Should().Be(1);
            zip.Entries.First().Name.Should().Be("errors.txt");
        }

        public class FakeContainer : BlobContainerClient
        {
            public FakeContainer()
                : base(new Uri("https://localhost"))
            {
            }

            public override BlobClient GetBlobClient(string blobName)
            {
                return new FakeBlob();
            }
        }

        public class FakeBlob : BlobClient
        {
            public FakeBlob()
                : base(new Uri("https://localhost/blob"))
            {
            }

            public override Task<Response<bool>> ExistsAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Response.FromValue(true, null));
            }

            public override Task<Response> DownloadToAsync(Stream destination)
            {
                return Task.FromResult(null as Response);
            }
        }
    }
}
