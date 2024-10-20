using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class UpdateSiteLogoTests : BaseInMemoryTest
    {
        public UpdateSiteLogoTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteExists_UpdateSiteLogo_ReturnsThatSite()
        {
            var site = Fixture.Create<Site>();
            var logoImageBinary = Fixture.CreateMany<byte>(10).ToArray();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{site.Id}/logo")
                    .ReturnsJson(site);

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(logoImageBinary)
                {
                    Headers = { ContentLength = logoImageBinary.Length }
                };
                dataContent.Add(fileContent, "logoImage", "abc.jpg");
                var response = await client.PutAsync($"sites/{site.Id}/logo", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateSiteLogo_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var logoImageBinary = Fixture.CreateMany<byte>(10).ToArray();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(logoImageBinary)
                {
                    Headers = { ContentLength = logoImageBinary.Length }
                };
                dataContent.Add(fileContent, "logoImage", "abc.jpg");
                var response = await client.PutAsync($"sites/{siteId}/logo", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}