using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Azure.Storage.Blobs.Models;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Controllers;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using DocsController = DigitalTwinCore.Controllers.DocumentsController;

using Willow.Api.AzureStorage;
using Willow.Common;

namespace DigitalTwinCore.Test.Controllers.DocumentsController
{
    public class PostDocumentTests : BaseInMemoryTest
    {
        private MethodInfo _method;
        private Attribute _attribute;
        private ParameterInfo _parameter;

        private readonly Mock<IContentTypeProvider> _contentTypeProviderMock = new Mock<IContentTypeProvider>();
        private readonly Mock<IBlobStore> _blobStore = new Mock<IBlobStore>();
        private readonly Mock<IHashCreator> _hashCreatorMock = new Mock<IHashCreator>();
        private readonly Mock<IDigitalTwinServiceProvider> _digitalTwinServiceFactoryMock = new Mock<IDigitalTwinServiceProvider>();
        private readonly Mock<IDigitalTwinService> _digitalTwinServiceMock = new Mock<IDigitalTwinService>();
        private readonly Mock<IGuidWrapper> _guidWrapperMock = new Mock<IGuidWrapper>();
        private readonly BlobStorageConfig _blobStorageConfig = new BlobStorageConfig
        {
            AccountKey = "123456789",
            AccountName = "ThatsAmazing",
            ContainerName = "IHaveTheSameCombinationOnMyLuggage",
        };
        private readonly ServerFixtureConfiguration _serverFixtureConfiguration = ServerFixtureConfigurations.InMemoryDb;
        private readonly MultipartFormDataContent _dataContent = new MultipartFormDataContent();

        private static readonly Guid _siteId = Guid.NewGuid();
        private string _existingDocumentId = string.Empty;
        private string _dtdlModelType = string.Empty;
        private string _fileMimeType = "TROGDOR";
        private HttpResponseMessage _result;
        private static Guid _createdGuid = Guid.NewGuid();

        private string _fileName = "SNARF.json";
        private Stream _fileContent = new MemoryStream();
        private string _fileContentType = "StrongBad";
        private byte[] _md5Hash = new byte[0];
        private string _endpointUrl = $"/sites/{_siteId}/documents";
        private readonly Response<BlobContentInfo> _uploadResponse = new TestResponse<BlobContentInfo>();
        private readonly TwinWithRelationships _twinWithRelationship = new TwinWithRelationships
        {
            Id = _createdGuid.ToString(),
            Metadata = new TwinMetadata
            {
                ModelId = WillowInc.DocumentModelId,
                WriteableProperties = new Dictionary<string, TwinPropertyMetadata>()
            },
        };



        public PostDocumentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("Category", "Unit-Signature")]
        public void ShouldAcceptOnlyHttpPostRequestsOnTheCorrectUrlPath()
        {
            GivenControllerHasAttribute<DocsController, ApiControllerAttribute>();
            GivenControllerHasAttribute<DocsController, RouteAttribute>();
            ThenRouteAttributeHasTemplateValueOf("sites/{siteId}/[controller]");

            GivenTypeHasMethod<DocsController>(nameof(DocsController.PostAsync));
            ThenPostAsyncMethodContains<HttpPostAttribute>();
            ThenHttpPostPathIs(null);
        }

        [Fact, Trait("Category", "Unit-Signature")]
        public void ShouldAcceptOnlyAuthorizedRequests()
        {
            GivenTypeHasMethod<DocsController>(nameof(DocsController.PostAsync));

            ThenPostAsyncMethodContains<AuthorizeAttribute>();
        }

        [Fact]
        public void ShouldAcceptValidASiteIdAsInput()
        {
            GivenTypeHasMethod<DocsController>(nameof(DocsController.PostAsync));

            ThenMethodContainsAnInputParameterOf<Guid>("siteId");
            ThenParameterHasAttribute<FromRouteAttribute>();
        }

        [Fact(Skip = "Testing v3")]
        public async Task ShouldWriteFilesToBlobStorage()
        {
            GivenAContentType();
            GivenABlobStorageConfiguration();
            GivenAHashCreator();
            GivenANewTwinId();
            GivenFileSuccessfullyUploadedToBlobClient();
            GivenTwinUploaded();
            GivenAJsonIsInTheRequest();

            GivenMockedInterfacesInjected();
            await GivenAFileInTheRequest();

            await WhenPostAsyncIsCalled();

            ThenFileWasUploadedToBlobStorage();
        }

        [Fact(Skip = "Testing v3")]
        public async Task ShouldReturnTwinsAdded()
        {
            GivenAContentType();
            GivenABlobStorageConfiguration();
            GivenAHashCreator();
            GivenANewTwinId();
            GivenFileSuccessfullyUploadedToBlobClient();
            GivenTwinUploaded();
            GivenAJsonIsInTheRequest();

            GivenMockedInterfacesInjected();
            await GivenAFileInTheRequest();

            await WhenPostAsyncIsCalled();

            await ThenTwinWithRelationshipWasReturned();
        }

        private void GivenAContentType()
        {
            _contentTypeProviderMock.Setup(ctp => ctp.TryGetContentType(It.IsAny<string>(), out _fileMimeType)).Returns(true);
        }

        private void GivenANewTwinId()
        {
            _guidWrapperMock.Setup(gw => gw.NewGuid()).Returns(_createdGuid);
        }

        private void GivenTwinUploaded()
        {            
            _digitalTwinServiceMock.Setup(dts => dts.AddOrUpdateTwinAsync(It.IsAny<Twin>(), true, null)).ReturnsAsync(_twinWithRelationship);
            _digitalTwinServiceFactoryMock.Setup(dtsf => dtsf.GetForSiteAsync(It.IsAny<Guid>())).ReturnsAsync(_digitalTwinServiceMock.Object);
        }

        private void GivenABlobStorageConfiguration()
        {
            _serverFixtureConfiguration.InjectedConfigurations = new[]
            {
                new MemoryConfigurationSource
                {
                    InitialData = new []
                    {
                        new KeyValuePair<string, string>("Azure:BlobStorage:AccountName", _blobStorageConfig.AccountName),
                        new KeyValuePair<string, string>("Azure:BlobStorage:AccountKey", _blobStorageConfig.AccountKey),
                        new KeyValuePair<string, string>("Azure:BlobStorage:ContainerName", _blobStorageConfig.ContainerName),
                    }
                }
            };
        }

        private void GivenAHashCreator()
        {
            _hashCreatorMock.Setup(hc => hc.Create(_fileContent)).Returns(_md5Hash);
        }

        private void GivenFileSuccessfullyUploadedToBlobClient()
        {
        }

        private void GivenControllerHasAttribute<TController, TAttribute>() where TAttribute : Attribute
        {
            _attribute = typeof(TController).GetCustomAttribute<TAttribute>();
            _attribute.Should().NotBeNull();
        }

        private void GivenTypeHasMethod<T>(string methodName)
        {
            _method = typeof(T).GetMethod(methodName);
            _method.Should().NotBeNull();
        }

        private void GivenAJsonIsInTheRequest()
        {
            var jsonString = JsonConvert.SerializeObject(new { ModelId = "dtmi:com:willowinc:Document;1", CustomProperties = new { description = "The description", name = "the name" } });

            _dataContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(jsonString)), "data");
        }

        private async Task GivenAFileInTheRequest()
        {
            await using var ms = new MemoryStream();
            await _fileContent.CopyToAsync(ms);
            var fileContentBytes = ms.ToArray();

            _dataContent.Add(
                new ByteArrayContent(fileContentBytes) { Headers = { ContentLength = fileContentBytes.Length } },
                "formFiles",
                _fileName);
        }

        private void GivenMockedInterfacesInjected()
        {
            _serverFixtureConfiguration.InjectedScopedServices = new[]
            {
                ServiceDescriptor.Describe(typeof(IContentTypeProvider), provider => _contentTypeProviderMock.Object, ServiceLifetime.Scoped),
                ServiceDescriptor.Describe(typeof(IBlobStore), provider => _blobStore.Object, ServiceLifetime.Scoped),
                ServiceDescriptor.Describe(typeof(IHashCreator), provider => _hashCreatorMock.Object, ServiceLifetime.Scoped),
                ServiceDescriptor.Describe(typeof(IGuidWrapper), provider => _guidWrapperMock.Object, ServiceLifetime.Singleton),
            };
        }


        private async Task WhenPostAsyncIsCalled()
        {
            using var server = CreateServerFixture(_serverFixtureConfiguration);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = _siteId, InstanceUri = "https://localhost", SiteCodeForModelId = "B121" });
            context.SaveChanges();

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(_siteId) as TestDigitalTwinService;

            var setup = new AdtSetupHelper(dts);
            setup.SetupModels();
            dts.Reload();

            using var client = server.CreateClient(null, Guid.NewGuid());

            _result = await client.PostAsync(_endpointUrl, _dataContent);

            _result.StatusCode.Should().Be(HttpStatusCode.Created);
            _result.Headers.Location.Should().Be(_endpointUrl);
        }
        private void ThenFileWasUploadedToBlobStorage()
        {
            _blobStore.Verify(bc => bc.Put(
                _fileName,
                It.IsAny<Stream>(),
                It.IsAny<object>()),
                Times.Once);
        }

        private void ThenRouteAttributeHasTemplateValueOf(string expectedTemplateValue)
        {
            ((RouteAttribute)_attribute).Template.Should().Be(expectedTemplateValue);
        }

        private void ThenPostAsyncMethodContains<T>() where T : Attribute
        {
            _attribute = _method.GetCustomAttribute<T>();
            _attribute.Should().NotBeNull();
        }

        private void ThenHttpPostPathIs(string expectedPath)
        {
            ((HttpPostAttribute)_attribute).Template.Should().Be(expectedPath);
        }

        private void ThenMethodContainsAnInputParameterOf<TParamType>(string expectedParameterName)
        {
            _parameter = _method.GetParameters().SingleOrDefault(pi => pi.Name == expectedParameterName && pi.ParameterType == typeof(TParamType));
            _parameter.Should().NotBeNull();
        }

        private void ThenParameterHasAttribute<TAttribute>() where TAttribute : Attribute
        {
            _attribute = _parameter.GetCustomAttribute<TAttribute>();
            _attribute.Should().NotBeNull();
        }

        private async Task ThenTwinWithRelationshipWasReturned()
        {
            var contentString = await _result.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<List<TwinWithRelationships>>(contentString).First();
            content.Metadata.Should().BeEquivalentTo(_twinWithRelationship.Metadata);
            content.Id.Should().BeEquivalentTo(_twinWithRelationship.Id);
        }

        private void ThenTwinWasCreatedInAdt()
        {
            _digitalTwinServiceMock.Verify(dts => 
                    dts.AddOrUpdateTwinAsync(It.Is<Twin>(t => 
                        t.Id == _createdGuid.ToString() 
                        && t.Metadata.ModelId == WillowInc.DocumentModelId ), true, null),
                Times.Once);
        }
    }

    internal class TestResponse<T> : Response<T>
    {
        public override Response GetRawResponse()
        {
            return new FakeResponse();
        }

        public override T Value { get; }
    }

    internal class FakeResponse : Response
    {
        public override void Dispose()
        {
        }

        protected override bool TryGetHeader(string name, out string? value)
        {
            value = String.Empty;
            return true;
        }

        protected override bool TryGetHeaderValues(string name, out IEnumerable<string>? values)
        {
            values = new[] { String.Empty };
            return true;
        }

        protected override bool ContainsHeader(string name)
        {
            return true;
        }

        protected override IEnumerable<HttpHeader> EnumerateHeaders()
        {
            return new List<HttpHeader>();
        }

        public override int Status { get; } = (int)HttpStatusCode.Created;
        public override string ReasonPhrase { get; }
        public override Stream? ContentStream { get; set; }
        public override string ClientRequestId { get; set; }
    }
}
