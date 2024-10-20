using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Moq.Contrib.HttpClient;
using Microsoft.IdentityModel.Tokens;
using PlatformPortalXL.Features.Twins;
using static PlatformPortalXL.Features.Twins.TwinSearchResponse;
using PlatformPortalXL.Helpers;
using Willow.ExceptionHandling;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class UploadSiteModulesTests : BaseInMemoryTest
    {
        public UploadSiteModulesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteExists_Upload3DModule_ReturnsNoContent()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var byteContent = Encoding.UTF8.GetBytes("file content");
            var fileName = "abc.rvt";

            var expectedFile = new DependencyServiceHttpHandlerExtensions.ExpectedFile { FileName = fileName, Content = byteContent };

            var user = Guid.Parse("01bb51cc-816b-4a9e-96cd-c108d688a8cc");

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClientWithPermissionOnSite(user, Permissions.ViewSites, siteId);
            server.Arrange().SetCurrentDateTime(utcNow);

            var bucketName = $"willow-site-{siteId}";
            var uniqueFileName = Path.GetFileNameWithoutExtension(fileName) + "_" + utcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName);

            var moduleRequest = new CreateUpdateModule3DRequest
            {
                Modules3D = new List<Module3DInfo>
                    {
                        new Module3DInfo
                        {
                            ModuleName = fileName,
                            Url = Base64UrlEncoder.Encode($"urn:adsk.objects:os.object:{bucketName}/{uniqueFileName}")
                        },
                    }
            };

            server.Arrange().GetSiteApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/module", moduleRequest)
                .ReturnsResponse(HttpStatusCode.NoContent);

            server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Get, $"search?siteIds={siteId}&modelId=dtmi:com:willowinc:Building;1&page=1")
                .ReturnsJson(new TwinSearchResponse() 
                {
                    Twins = new SearchTwin[]
                    {
                        new SearchTwin () { SiteId = siteId, Id = Guid.NewGuid().ToString()}
                    }
                });

            // needed by for the call to siteService.MapToTwinId
            server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Get, $"search?siteIds={siteId}&modelId=dtmi%3acom%3awillowinc%3aBuilding%3b1&page=1")
                .ReturnsJson(new TwinSearchResponse()
                {
                    Twins = new SearchTwin[]
                    {
                        new SearchTwin () { SiteId = siteId, Id = Guid.NewGuid().ToString()}
                    }
                });

            var dataContent = new MultipartFormDataContent();
            var fileContent1 = new ByteArrayContent(byteContent)
            {
                Headers = { ContentLength = byteContent.Length }
            };

            dataContent.Add(fileContent1, "file", fileName);

            var response = await client.PostAsync($"sites/{siteId}/module", dataContent);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task SiteExists_Upload3DModule_ReturnsUnprocessableEntity()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var byteContent = Encoding.UTF8.GetBytes("file content");
            var fileName = "abc.jpg";

            var expectedResponse = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Unprocessable entity",
                Data = new { Errors = new[] { new { Name = fileName, Message = "Invalid data: unknown format" } } }
            };

            var expectedFile = new DependencyServiceHttpHandlerExtensions.ExpectedFile {FileName = fileName, Content = byteContent};

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);

            server.Arrange().SetCurrentDateTime(utcNow);
            var uniqueFileName1= Path.GetFileNameWithoutExtension(fileName) + "_" + utcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName);
            var bucketName = $"willow-site-{siteId}";

            var moduleRequest = new CreateUpdateModule3DRequest
            {
                Modules3D = new List<Module3DInfo>
                    {
                        new Module3DInfo
                        {
                            ModuleName = fileName,
                            Url = Base64UrlEncoder.Encode($"urn:adsk.objects:os.object:{bucketName}/{uniqueFileName1}")
                        }
                    }
            };

            server.Arrange().GetSiteApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/module", moduleRequest)
                .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                { Content = new StringContent(JsonSerializerHelper.Serialize(expectedResponse), Encoding.UTF8, "application/problem+json") }));

            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(byteContent)
            {
                Headers = { ContentLength = byteContent.Length }
            };
            dataContent.Add(fileContent, "file", fileName);

            var response = await client.PostAsync($"sites/{siteId}/module", dataContent);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var result = await response.Content.ReadAsAsync<ValidationError>();
            result.Items.Should().HaveCount(1);
            result.Items[0].Name.Should().Be("files");
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_Upload3DModule_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(new byte[0])
                {
                    Headers = { ContentLength = 0 }
                };
                dataContent.Add(fileContent, "files", "abc.jpg");
                var response = await client.PostAsync($"sites/{siteId}/module", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task SiteExists_DeleteModule_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var moduleId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/module")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{siteId}/module");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteModule_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/module");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ModuleExists_GetModule_ReturnsModule()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var byteContent = Encoding.UTF8.GetBytes("file content");
            var fileName = "abc.rvt";

            var expectedResponse = new LayerGroupModuleDto
            {
                Name = fileName
            };

            var expectedFile = new DependencyServiceHttpHandlerExtensions.ExpectedFile { FileName = fileName, Content = byteContent };

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);

            server.Arrange().SetCurrentDateTime(utcNow);
            var uniqueFileName1 = Path.GetFileNameWithoutExtension(fileName) + "_" + utcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName);
            var bucketName = $"willow-site-{siteId}";

            server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Get, $"sites/{siteId}/module")
                .ReturnsJson(expectedResponse);

            var response = await client.GetAsync($"sites/{siteId}/module");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<LayerGroupModuleDto>();
            result.Name.Should().Be(fileName);
        }

        [Fact]
        public async Task ModuleDoesNotExist_GetModule_ReturnsNoContent()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);

            server.Arrange().SetCurrentDateTime(utcNow);

            server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Get, $"sites/{siteId}/module")
                .ReturnsResponse(HttpStatusCode.NoContent);

            var response = await client.GetAsync($"sites/{siteId}/module");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
