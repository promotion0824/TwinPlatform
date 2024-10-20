using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using Xunit;
using Moq;
using Newtonsoft.Json;

using Willow.Common;

using ImageHub.Models;
using ImageHub.Services;

using Willow.ImageHub.Services;

namespace ImageHub.UnitTests
{
    public class ImageServiceTests
    {
        private readonly ImageService  _svc;
        private readonly Guid  _rootId = Guid.NewGuid();

         private readonly Mock<IFileNameParser> _fileNameParser = new Mock<IFileNameParser>();
         private readonly Mock<IImageRepository> _sourceRepo    = new Mock<IImageRepository>();
         private readonly Mock<IImageRepository> _cacheRepo     = new Mock<IImageRepository>();
         private readonly Mock<IImageEngine> _processorEngine   = new Mock<IImageEngine>();

        public ImageServiceTests()
        {
            _svc = new ImageService(_fileNameParser.Object, _sourceRepo.Object, _cacheRepo.Object, _processorEngine.Object);
        }

        [Fact]
        public async Task ImageService_GetImage_success()
        {
            var segments = new List<string> { "bob" };
            var imageId = Guid.NewGuid();

            var content = new MemoryStream(UTF8Encoding.Default.GetBytes("bobs your uncle"));

            _processorEngine.Setup( p=> p.Process(It.IsAny<Stream>(), It.IsAny<RequestImageDescriptor>())).Returns(content);
            _fileNameParser.Setup( p=> p.Parse(It.IsAny<string>())).Returns(FileNameParser.ParseFileName($"{imageId}_Original_Original.png"));
            _sourceRepo.Setup( r=> r.Get(ImageService.GetOriginalImageDataFilePath(_rootId, segments, imageId))).ReturnsAsync(content);

            var result = await _svc.GetImage(_rootId, imageId.ToString(), segments);

            _processorEngine.Verify( p=> p.Process(It.IsAny<Stream>(), It.IsAny<RequestImageDescriptor>()), Times.Once);
            _cacheRepo.Verify( r=> r.Get(It.IsAny<string>()), Times.Once);
            _sourceRepo.Verify( r=> r.Get(ImageService.GetOriginalImageDataFilePath(_rootId, segments, imageId)), Times.Once);

            Assert.NotNull(result.Item1);
            Assert.NotNull(result.Item2);
        }

        [Fact]
        public async Task ImageService_GetImage_fails()
        {
            var segments = new List<string> { "bob" };
            var imageId = Guid.NewGuid();

            var content = new MemoryStream(UTF8Encoding.Default.GetBytes("bobs your uncle"));

            _processorEngine.Setup( p=> p.Process(It.IsAny<Stream>(), It.IsAny<RequestImageDescriptor>())).Returns(content);
            _fileNameParser.Setup( p=> p.Parse(It.IsAny<string>())).Returns(FileNameParser.ParseFileName($"{imageId}_Original_Original.png"));
            _cacheRepo.Setup( r=> r.Get(It.IsAny<string>())).ThrowsAsync(new FileNotFoundException());
            _sourceRepo.Setup( r=> r.Get(ImageService.GetOriginalImageDataFilePath(_rootId, segments, imageId))).ThrowsAsync(new FileNotFoundException());

            await Assert.ThrowsAsync<FileNotFoundException>( async ()=> await _svc.GetImage(_rootId, imageId.ToString(), segments));
        }

        [Fact]
        public async Task ImageService_DeleteImage_success()
        {
            var segments = new List<string> { "bob" };
            var imageId = Guid.NewGuid();

            var content = new MemoryStream(UTF8Encoding.Default.GetBytes("bobs your uncle"));

            _fileNameParser.Setup( p=> p.Parse(It.IsAny<string>())).Returns(FileNameParser.ParseFileName($"{imageId}_Original_Original.png"));

            await _svc.DeleteImage(_rootId, imageId.ToString(), segments);

            _cacheRepo.Verify( r=> r.DeleteFolder(It.IsAny<string>()), Times.Once);
            _sourceRepo.Verify( r=> r.Delete(ImageService.GetOriginalImageDataFilePath(_rootId, segments, imageId)), Times.Once);
        }

        [Fact]
        public async Task ImageService_CreateImage_success()
        {
            var segments = new List<string> { "bob" };
            var imageId = Guid.NewGuid();

            var imageFile = new Mock<IFormFile>();
            var images = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var imageStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ImageHub.UnitTests.Willow Logo.png");

            imageFile.Setup( f=> f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback( (Stream output, CancellationToken cancel)=> imageStream.CopyTo(output));
            _sourceRepo.Setup( r=> r.Add(It.IsAny<string>(), It.IsAny<Stream>())).ReturnsAsync(new ImageDescriptor { FileName = imageId.ToString(), FileExtension = ".png"});

            _fileNameParser.Setup( p=> p.Parse(It.IsAny<string>())).Returns(FileNameParser.ParseFileName($"{imageId}_Original_Original.png"));

            var result = await _svc.CreateImage(_rootId, segments, imageFile.Object);

            Assert.NotNull(result);

            _sourceRepo.Verify( r=> r.Add(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);

        }

        // "159d12d0-7491-490a-8c61-54a21e94924d/sites/5f549b74-c430-4fa1-9841-637ec6998651/logo/2501eaa3-24f3-42bc-af95-507ed9abf2a7_1_w300_h420.jpg";

        // 159d12d0-7491-490a-8c61-54a21e94924d
        // sites
        // 5f549b74-c430-4fa1-9841-637ec6998651
        // logo
        // 2501eaa3-24f3-42bc-af95-507ed9abf2a7_1_w300_h420.jpg
        [Fact]
        public void ImageService_GetCachedImageFilePathPrefix()
        {
            var segments = ImageService.NormalizePathSegments("sites", "5f549b74-c430-4fa1-9841-637ec6998651", "logo");

            var requestImageDescriptor = FileNameParser.ParseFileName("2501eaa3-24f3-42bc-af95-507ed9abf2a7_1_w300_h420.jpg");

            var prefix = ImageService.GetCachedImageFilePathPrefix(Guid.Parse("159d12d0-7491-490a-8c61-54a21e94924d"), segments, requestImageDescriptor.ImageId);

            Assert.NotNull(prefix);            
        }
    }
}
