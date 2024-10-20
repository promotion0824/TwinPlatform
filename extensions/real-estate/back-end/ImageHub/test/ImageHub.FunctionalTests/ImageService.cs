using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Moq;

using Microsoft.AspNetCore.Http;

using ImageHub.Services;

using Willow.Azure.Storage;
using Willow.ImageHub.Services;

namespace ImageHub.FunctionalTests
{
    public class ImageServiceTests
    {
        // Emulator
        private const string _connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";        
        private readonly IImageService _svc;

        public ImageServiceTests()
        {
            var sourceStore     = new AzureBlobStorage(_connectionString, "original");
            var cacheStore      = new AzureBlobStorage(_connectionString, "cached");
            var sourceRepo      = new ImageRepository(sourceStore);
            var cacheRepo       = new ImageRepository(cacheStore);
            var processorEngine = new ImageEngine();

            _svc = new ImageService(new FileNameParser(), sourceRepo, cacheRepo, processorEngine);
        }
        
        [Fact]
        public async Task ImageService_DeleteImage()
        {
            var rootId = Guid.NewGuid();

            // First add some images
            var imageFile = new Mock<IFormFile>();
            var imageStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ImageHub.FunctionalTests.Willow Logo.png");
                        
            imageFile.Setup( f=> f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback( (Stream output, CancellationToken cancel)=> 
            { 
                imageStream.Position = 0; 
                imageStream.CopyTo(output); 
            });

            var segments = new List<string> { "c1", "sites", "s1", "logo" } ;

            var img1 = await _svc.CreateImage(rootId, segments, imageFile.Object);

            var thumb11 = await _svc.GetImage(rootId, $"{img1.ImageId}_1_w300_h420.jpg", segments);
            var thumb12 = await _svc.GetImage(rootId, $"{img1.ImageId}_1_w240_h160.jpg", segments);
            var thumb13 = await _svc.GetImage(rootId, $"{img1.ImageId}_1_w80_h60.jpg", segments);

            await _svc.DeleteImage(rootId, $"{img1.ImageId}_original_original.jpg", segments);
        }
    }
}

// https://wil-uat-lda-shr-glb-command.azurefd.net/au/api/images/159d12d0-7491-490a-8c61-54a21e94924d/sites/35d5e71c-7fe3-44e3-a8c8-261feadd125b/logo/443d85b0-38f3-40d3-a80a-8cdd3f93985a_1_w300_h420.jpg