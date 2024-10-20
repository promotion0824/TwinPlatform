using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.Model.Async;
using Willow.ServiceBus;
using Willow.Storage.Blobs;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Services
{
    public class AsyncJobMock : AsyncJob
    {
        public AsyncJobMock(string jobId) : base(jobId)
        {
        }
    }

    public class TopicMock : Topic
    { }

    public class AsyncServiceTests
    {
        private const string _folder = "dummyFolder";
        private readonly AsyncService<AsyncJobMock> _asyncService;
        private readonly Mock<IMessageSender> _messageSenderMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _configurationSectionMock;
        private readonly Mock<ILogger<AsyncService<AsyncJobMock>>> _loggerMock;


        public AsyncServiceTests()
        {
            _messageSenderMock = new Mock<IMessageSender>();
            _blobServiceMock = new Mock<IBlobService>();
            _configurationMock = new Mock<IConfiguration>();
            _configurationSectionMock = new Mock<IConfigurationSection>();
            _loggerMock = new Mock<ILogger<AsyncService<AsyncJobMock>>>();

            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(_configurationSectionMock.Object);
            _asyncService = new AsyncService<AsyncJobMock>(_messageSenderMock.Object, _blobServiceMock.Object, _configurationMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task TriggerAsyncProcess_ShouldPublishEvent()
        {
            var jobId = Guid.NewGuid().ToString();
            var asyncJob = new AsyncJobMock(jobId);
            await _asyncService.TriggerAsyncProcess(asyncJob, new TopicMock());

            _messageSenderMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AsyncJobMock>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindAsyncJobs_ShouldReturnJob()
        {
            var jobId = Guid.NewGuid().ToString();
            var asyncJob = new AsyncJobMock(jobId);
            var foundNames = new List<string> { "blobpath" };

            _blobServiceMock.Setup(x => x.GetBlobNameByTags(It.IsAny<string>(), It.IsAny<IEnumerable<(string, string, string)>>())).ReturnsAsync(foundNames);
            _blobServiceMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(SerializeToStream(asyncJob));

            var jobs = await _asyncService.FindAsyncJobs(_folder, jobId, AsyncJobStatus.Done, "test@mail.com", DateTime.Now, DateTime.Now.AddDays(1));

            _blobServiceMock.Verify(x => x.GetBlobNameByTags(It.IsAny<string>(), It.IsAny<IEnumerable<(string, string, string)>>()), Times.Once);
            _blobServiceMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(foundNames.Count));
            Assert.Equal(foundNames.Count, jobs.Count());
            Assert.Single(jobs);
            Assert.Equal(jobId, jobs.Single().JobId);
        }

        [Fact]
        public async Task FindAsyncJobs_NoneFound_ShouldReturnEmpty()
        {
            var jobId = Guid.NewGuid().ToString();

            _blobServiceMock.Setup(x => x.GetBlobNameByTags(It.IsAny<string>(), It.IsAny<IEnumerable<(string, string, string)>>())).ReturnsAsync(Enumerable.Empty<string>());

            var jobs = await _asyncService.FindAsyncJobs(_folder, jobId, AsyncJobStatus.Done, "test@mail.com", DateTime.Now, DateTime.Now.AddDays(1));

            _blobServiceMock.Verify(x => x.GetBlobNameByTags(It.IsAny<string>(), It.IsAny<IEnumerable<(string, string, string)>>()), Times.Once);
            _blobServiceMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.Empty(jobs);
        }

        public MemoryStream SerializeToStream(object o)
        {
            var stream = new MemoryStream();
            stream.FromString(JsonSerializer.Serialize(o));
            return stream;
        }

        [Fact]
        public async Task StoreRequest_ShouldUploadRequest()
        {
            var jobId = Guid.NewGuid().ToString();
            var asyncJob = new AsyncJobMock(jobId);
            await _asyncService.StoreRequest(_folder, jobId, asyncJob);

            _blobServiceMock.Verify(x => x.UploadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task StoreAsyncJob_ShouldUploadRequestAndSetTags()
        {
            var jobId = Guid.NewGuid().ToString();
            var asyncJob = new AsyncJobMock(jobId) { CreateTime = DateTime.Now, UserId = "test@email.com" };
            await _asyncService.StoreAsyncJob(_folder, asyncJob);

            _blobServiceMock.Verify(x => x.UploadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            _blobServiceMock.Verify(x => x.MergeTags(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetRequest_WithPath_ReturnsObject()
        {
            var jobId = Guid.NewGuid().ToString();
            _blobServiceMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(SerializeToStream(new AsyncJobMock(jobId)));

            var job = await _asyncService.GetRequest<AsyncJobMock>("path");

            Assert.NotNull(job);
            Assert.Equal(jobId, job.JobId);
            _blobServiceMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetLatestAsyncJob_NoBlobs_ReturnsNullJob()
        {
            var jobId = Guid.NewGuid().ToString();
            _blobServiceMock.Setup(x => x.GetBlobItems(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Enumerable.Empty<BlobItem>());

            var job = await _asyncService.GetLatestAsyncJob(_folder, AsyncJobStatus.Done);

            Assert.Null(job);
            _blobServiceMock.Verify(x => x.GetBlobItems(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
