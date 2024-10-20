using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.StaticFiles;

using Xunit;
using Moq;
using PlatformPortalXL.Features.Twins;
using Willow.Api.Client;
using Willow.Common;

using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.Test.Infrastructure;
using Willow.Platform.Models;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class DigitalTwinApiServiceTests
    {
        private readonly Mock<IRestApi> _digitalTwinCoreApi;
        private readonly Mock<IBlobStore> _blobStore;
        private readonly Mock<IContentTypeProvider> _contentTypeProvider;

        public DigitalTwinApiServiceTests()
        {
            _digitalTwinCoreApi   = new Mock<IRestApi>();
            _blobStore            = new Mock<IBlobStore>();
            _contentTypeProvider  = new Mock<IContentTypeProvider>();
        }

        [Fact]
        public async Task DigitalTwinApiService_GetFile()
        {
            var svc = new DigitalTwinApiService(_digitalTwinCoreApi.Object, _blobStore.Object, _contentTypeProvider.Object);
            var siteId  = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var fileId  = Guid.Parse("b777c442-9dea-4628-bada-f4cc3effa2b3");

            _blobStore.Setup( api=> api.Get("b777c442-9dea-4628-bada-f4cc3effa2b3", It.IsAny<Stream>()) ).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes("Bobs your uncle"), 0, UTF8Encoding.Default.GetByteCount("Bobs your uncle")));
            _digitalTwinCoreApi.Setup(api => api.Get<List<DigitalTwinDocument>>($"sites/{siteId}/assets/{assetId}/documents", null)).ReturnsAsync(new List<DigitalTwinDocument> { new DigitalTwinDocument { Id = fileId, Name = "bob", Uri = new Uri("https://jimpocstorage1.blob.core.windows.net/test/b777c442-9dea-4628-bada-f4cc3effa2b3") } });

            var result = await svc.GetFileAsync(siteId, assetId, fileId);

            Assert.NotNull(result);
            Assert.Equal(UTF8Encoding.Default.GetByteCount("Bobs your uncle"), result.Content.Length);
        }

        [Fact]
        public async Task DigitalTwinApiService_GetDocumentStreamAsync()
        {
            var svc = new DigitalTwinApiService(_digitalTwinCoreApi.Object, _blobStore.Object, _contentTypeProvider.Object);
            var siteId  = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var documentId  = Guid.Parse("b777c442-9dea-4628-bada-f4cc3effa2b3");

            _blobStore.Setup( api=> api.Get("b777c442-9dea-4628-bada-f4cc3effa2b3", It.IsAny<Stream>()) ).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes("Bobs your uncle"), 0, UTF8Encoding.Default.GetByteCount("Bobs your uncle")));
            _digitalTwinCoreApi.Setup(api => api.Get<DigitalTwinBasicDocument>($"admin/sites/{siteId}/twins/{documentId}", null)).ReturnsAsync(new DigitalTwinBasicDocument { Id = documentId, Name = "bob", Url = new Uri("https://jimpocstorage1.blob.core.windows.net/test/b777c442-9dea-4628-bada-f4cc3effa2b3") });

            var result = await svc.GetDocumentStreamAsync(siteId, documentId.ToString());

            Assert.NotNull(result);
            Assert.Equal(UTF8Encoding.Default.GetByteCount("Bobs your uncle"), result.Content.Length);
        }

        [Theory]
        [AutoData]
        public async Task DigitalTwinApiService_Search(TwinSearchRequest request)
        {
            request.ModelId = null;
            request.SiteIds = Array.Empty<Guid>();

            var svc = new DigitalTwinApiService(_digitalTwinCoreApi.Object, _blobStore.Object, _contentTypeProvider.Object);

            await svc.Search(request);

            var parms = ParseQueryString(_digitalTwinCoreApi.Invocations[0].Arguments[0].ToString());
            parms.GetValues("siteIds").Should().BeNull();
            parms.Get("term").Should().Be(request.Term);
            parms.Get("queryId").Should().Be(request.QueryId);
            parms.Get("page").Should().Be(request.Page.ToString());
        }

        [Theory]
        [CustomAutoData]
        public async Task DigitalTwinApiService_Search_WithModelId(TwinSearchRequest request, Site[] sites)
        {
            request.SiteIds = sites.Select(x => x.Id).ToArray();

            var svc = new DigitalTwinApiService(_digitalTwinCoreApi.Object, _blobStore.Object, _contentTypeProvider.Object);
            await svc.Search(request);

            var parms = ParseQueryString(_digitalTwinCoreApi.Invocations[0].Arguments[0].ToString());
            parms.GetValues("siteIds").Should().Equal(sites.Select(s => s.Id.ToString()));
            parms.Get("modelId").Should().Be(request.ModelId);
            parms.Get("term").Should().Be(request.Term);
            parms.GetValues("fileTypes").Should().Equal(request.FileTypes);
            parms.Get("queryId").Should().Be(request.QueryId);
            parms.Get("isCapabilityOfModelId").Should().Be(request.IsCapabilityOfModelId);
        }

        [Theory]
        [AutoData]
        public async Task DigitalTwinApiService_Search_WithSiteId(TwinSearchRequest request)
        {
            request.ModelId = null;

            var svc = new DigitalTwinApiService(_digitalTwinCoreApi.Object, _blobStore.Object, _contentTypeProvider.Object);
            await svc.Search(request);

            var parms = ParseQueryString(_digitalTwinCoreApi.Invocations[0].Arguments[0].ToString());
            parms.GetValues("siteIds").Should().Equal(request.SiteIds.Select(id => id.ToString()));
            parms.Get("term").Should().Be(request.Term);
            parms.GetValues("fileTypes").Should().Equal(request.FileTypes);
            parms.Get("queryId").Should().Be(request.QueryId);
            parms.Get("isCapabilityOfModelId").Should().Be(request.IsCapabilityOfModelId);
        }

        private NameValueCollection ParseQueryString(string path)
        {
            // HttpUtility.ParseQueryString appears to expect its first character to be
            // a question mark, so if we have something like "search?key=val" we strip
            // out "search".
            return HttpUtility.ParseQueryString(path.Substring(path.IndexOf("?")));
        }
    }
}
