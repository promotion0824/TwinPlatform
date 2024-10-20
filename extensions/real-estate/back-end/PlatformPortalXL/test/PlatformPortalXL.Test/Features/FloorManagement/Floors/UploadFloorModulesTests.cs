using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Microsoft.IdentityModel.Tokens;
using PlatformPortalXL.Features.Twins;
using static PlatformPortalXL.Features.Twins.TwinSearchResponse;
using PlatformPortalXL.Helpers;
using Willow.ExceptionHandling;

namespace PlatformPortalXL.Test.Features.FloorManagement.Floors
{
    public class UploadFloorModulesTests : BaseInMemoryTest
    {
        public UploadFloorModulesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorExists_Upload2DModule_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var expectedFloor = Fixture.Build<Floor>()
                .Create();
            var byteContent = Encoding.UTF8.GetBytes("file content");
            var fileName = "abc.jpg";
            var fileName2 = "abcd.jpg";

            var expectedFiles = new[]
            {
                new DependencyServiceHttpHandlerExtensions.ExpectedFile {FileName = fileName, Content = byteContent},
                new DependencyServiceHttpHandlerExtensions.ExpectedFile {FileName = fileName2, Content = byteContent}
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedFiles(HttpMethod.Post, $"sites/{siteId}/floors/{expectedFloor.Id}/2dmodules", "files", expectedFiles)
                    .ReturnsJson(expectedFloor);

                var dataContent = new MultipartFormDataContent();
                var fileContent1 = new ByteArrayContent(byteContent)
                {
                    Headers = { ContentLength = byteContent.Length }
                };
                var fileContent2 = new ByteArrayContent(byteContent)
                {
                    Headers = { ContentLength = byteContent.Length }
                };
                dataContent.Add(fileContent1, "files", fileName);
                dataContent.Add(fileContent2, "files", fileName2);

                var response = await client.PostAsync($"sites/{siteId}/floors/{expectedFloor.Id}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(expectedFloor));
            }
        }

        [Fact]
        public async Task FloorExists_Upload3DModule_ReturnsUpdatedFloor()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var expectedFloor = Fixture.Build<Floor>()
                .Create();
            var byteContent = Encoding.UTF8.GetBytes("file content");
            var fileName1 = "abc.rvt";
            var fileName2 = "abcd.rvt";

            var expectedFiles = new[]
            {
                new DependencyServiceHttpHandlerExtensions.ExpectedFile {FileName = fileName1, Content = byteContent},
                new DependencyServiceHttpHandlerExtensions.ExpectedFile {FileName = fileName2, Content = byteContent}
            };

            var user = Guid.Parse("01bb51cc-816b-4a9e-96cd-c108d688a8cc");

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(user, Permissions.ManageFloors, siteId))
            {
                server.Arrange().SetCurrentDateTime(utcNow);

                var bucketName = $"willow-site-{siteId}";
                var uniqueFileName1 = Path.GetFileNameWithoutExtension(fileName1) + "_" + utcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName1);
                var uniqueFileName2 = Path.GetFileNameWithoutExtension(fileName2) + "_" + utcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName2);

                var moduleRequest = new CreateUpdateModule3DRequest
                {
                    Modules3D = new List<Module3DInfo>
                    {
                        new Module3DInfo
                        {
                            ModuleName = fileName1,
                            Url = Base64UrlEncoder.Encode($"urn:adsk.objects:os.object:{bucketName}/{uniqueFileName1}")
                        },
                        new Module3DInfo
                        {
                            ModuleName = fileName2,
                            Url = Base64UrlEncoder.Encode($"urn:adsk.objects:os.object:{bucketName}/{uniqueFileName2}")
                        },
                    }
                };

                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/floors/{expectedFloor.Id}/3dmodules", moduleRequest)
                    .ReturnsJson(expectedFloor);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"search?siteIds={siteId}&modelId=dtmi:com:willowinc:Level;1&page=1")
                    .ReturnsJson(new TwinSearchResponse()
                    {
                        Twins = new SearchTwin[]
                        {
                            new SearchTwin () { SiteId = siteId, Id = Guid.NewGuid().ToString(), FloorId = expectedFloor.Id }
                        }
                    });

                // needed by for the call to floorService.MapToTwinId
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"search?siteIds={siteId}&modelId=dtmi%3acom%3awillowinc%3aLevel%3b1&page=1")
                    .ReturnsJson(new TwinSearchResponse()
                    {
                        Twins = new SearchTwin[]
                        {
                            new SearchTwin () { SiteId = siteId, Id = Guid.NewGuid().ToString(), FloorId = expectedFloor.Id }
                        }
                    });

                var dataContent = new MultipartFormDataContent();
                var fileContent1 = new ByteArrayContent(byteContent)
                {
                    Headers = { ContentLength = byteContent.Length }
                };
                var fileContent2 = new ByteArrayContent(byteContent)
                {
                    Headers = { ContentLength = byteContent.Length }
                };
                dataContent.Add(fileContent1, "files", fileName1);
                dataContent.Add(fileContent2, "files", fileName2);

                var response = await client.PostAsync($"sites/{siteId}/floors/{expectedFloor.Id}/3dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(expectedFloor));
            }
        }

        [Fact]
        public async Task FloorExists_Upload2DModule_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var expectedFloor = Fixture.Build<Floor>()
                .Create();
            var byteContent = Encoding.UTF8.GetBytes("file content");
            var fileName = "abc.jpg";

            var expectedResponse = new ErrorResponse
            {
                StatusCode = (int) HttpStatusCode.BadRequest,
                Message = "Unprocessable entity",
                Data = new { Errors = new [] { new { Name = fileName, Message = "Invalid data: unknown format" } } }
            };

            var expectedFiles = new[]
                {new DependencyServiceHttpHandlerExtensions.ExpectedFile {FileName = fileName, Content = byteContent}};

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedFiles(HttpMethod.Post,
                        $"sites/{siteId}/floors/{expectedFloor.Id}/2dmodules", "files", expectedFiles)
                    .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {Content = new StringContent(JsonSerializerHelper.Serialize(expectedResponse), Encoding.UTF8, "application/problem+json")}));

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(byteContent)
                {
                    Headers = { ContentLength = byteContent.Length }
                };
                dataContent.Add(fileContent, "files", fileName);

                var response = await client.PostAsync($"sites/{siteId}/floors/{expectedFloor.Id}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Name.Should().Be("files");
            }
        }

        [Fact]
        public async Task FloorExists_Upload3DModule_ReturnsUnprocessableEntity()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var expectedFloor = Fixture.Build<Floor>()
                .Create();
            var byteContent = Encoding.UTF8.GetBytes("file content");
            var fileName = "abc.rvt";

            var expectedResponse = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Unprocessable entity",
                Data = new { Errors = new[] { new { Name = fileName, Message = "Invalid data: unknown format" } } }
            };

            var expectedFiles = new[]
                {new DependencyServiceHttpHandlerExtensions.ExpectedFile {FileName = fileName, Content = byteContent}};

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().SetCurrentDateTime(utcNow);
                var uniqueFileName1 = Path.GetFileNameWithoutExtension(fileName) + "_" + utcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName);
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
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/floors/{expectedFloor.Id}/3dmodules", moduleRequest)
                    .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    { Content = new StringContent(JsonSerializerHelper.Serialize(expectedResponse), Encoding.UTF8, "application/problem+json") }));

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(byteContent)
                {
                    Headers = { ContentLength = byteContent.Length }
                };
                dataContent.Add(fileContent, "files", fileName);

                var response = await client.PostAsync($"sites/{siteId}/floors/{expectedFloor.Id}/3dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Name.Should().Be("files");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_Upload2DModule_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(new byte[0])
                {
                    Headers = { ContentLength = 0 }
                };
                dataContent.Add(fileContent, "files", "abc.jpg");

                var response = await client.PostAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_Upload3DModule_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(new byte[0])
                {
                    Headers = { ContentLength = 0 }
                };
                dataContent.Add(fileContent, "files", "abc.jpg");
                var response = await client.PostAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/3dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
