using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Xunit;
using Moq;
using Newtonsoft.Json;

using Willow.Common;

using ImageHub.Models;
using ImageHub.Services;

namespace ImageHub.UnitTests
{
    public class ImageRepositoryTests
    {
        private readonly Mock<IBlobStore> _blobStore;
        private readonly ImageRepository    _repo;

        public ImageRepositoryTests()
        {
            _blobStore = new Mock<IBlobStore>();
            _repo = new ImageRepository(_blobStore.Object);
        }

        [Fact]
        public async Task ImageRepository_Get_success()
        {
            _blobStore.Setup( s=> s.Get("bob", It.IsAny<Stream>())).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes("wilma")));

            var result = await _repo.Get("bob");

            Assert.NotNull(result);
        }
    }
}
