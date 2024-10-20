using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.ConfigurationController
{
    public class GetConfigurationTests : BaseInMemoryTest
    {
        public GetConfigurationTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteExists_GetConfiguration_ReturnsConfiguration()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            SiteConfigurationDto expectedConfiguration = new SiteConfigurationDto {
                SiteCodeForModelId = "B121",
                AdtInstanceUri = "https://localhost/"
            };

            context.SiteSettings.Add(
                new SiteSettingEntity { 
                    SiteId = siteId, 
                    InstanceUri = "https://localhost", 
                    SiteCodeForModelId = expectedConfiguration.SiteCodeForModelId
                });
            context.SaveChanges();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"/sites/{siteId}/configuration");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<SiteConfigurationDto>();

            result.Should().BeEquivalentTo(expectedConfiguration);
        }

        [Fact]
        public async Task NoSiteExists_GetConfiguration_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"/sites/{siteId}/configuration");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
