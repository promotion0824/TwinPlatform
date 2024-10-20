using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Features.DirectoryCore.Dtos;
using DigitalTwinCore.Features.SiteAdmin;
using DigitalTwinCore.Services.AdtApi;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Features.SiteAdmin
{
    public class SiteAdminTests : BaseInMemoryTest
    {
        public SiteAdminTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [AutoData]
        public async Task PostSite_Should_Fail_When_Site_Not_Exists(NewAdtSiteRequest request)
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            server
                .Arrange()
                .GetHttpHandler(ApiServiceNames.DirectoryCore)
                .HttpHandler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);

            using var client = server.CreateClient(null);
            var response = await client.PostAsJsonAsync("admin/sites", request);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        [Theory]
        [AutoData]
        public async Task PostSite_Should_Fail_When_Site_Is_Already_Setup(NewAdtSiteRequest request, Site site)
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            serverArrangement.GetHttpHandler(ApiServiceNames.DirectoryCore).HttpHandler.SetupAnyRequest().ReturnsJsonUsingNewtonsoft(site);
            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
            context.SiteSettings.Add(new SiteSettingEntity
            {
                AdxDatabase = request.AdxDatabase,
                InstanceUri = request.InstanceUri.AbsoluteUri,
                SiteCodeForModelId = request.SiteCode,
                SiteId = request.SiteId
            });
            await context.SaveChangesAsync();
            using var client = server.CreateClient(null);
            var response = await client.PostAsJsonAsync("admin/sites", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
        [Theory]
        [AutoData]
        public async Task PostSite_Should_Save_SiteSettings(NewAdtSiteRequest request, Site site)
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            
            var serverArrangement = server.Arrange();
            serverArrangement.GetHttpHandler(ApiServiceNames.DirectoryCore).HttpHandler.SetupAnyRequest().ReturnsJsonUsingNewtonsoft(site);
            
            using var client = server.CreateClient(null);
            
            var response = await client.PostAsJsonAsync("admin/sites", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.ToString().Should().EndWith($"admin/sites/{request.SiteId}");
            
            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
            var siteSettings = await context.SiteSettings.FindAsync(request.SiteId);
            siteSettings.Should().NotBeNull();
            siteSettings.AdxDatabase.Should().Be(request.AdxDatabase);
            siteSettings.InstanceUri.Should().Be(request.InstanceUri.AbsoluteUri);
            siteSettings.SiteCodeForModelId.Should().Be(request.SiteCode);

            var result = await response.Content.ReadFromJsonAsync<SiteAdtSettings>();
            var siteAdtSettings = SiteAdtSettings.CreateInstance(request.SiteId, siteSettings);

            result.Should().BeEquivalentTo(siteAdtSettings);
        }

        [Theory]
        [AutoData]
        public async Task GetSite_Should_Return_Site(SiteSettingEntity site, Uri siteUri)
        {
            site.InstanceUri = siteUri.AbsoluteUri;

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            await context.SiteSettings.AddAsync(site);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            
            var siteAdtSettings = await client.GetFromJsonAsync<SiteAdtSettings>($"admin/sites/{site.SiteId}");
            siteAdtSettings.Should().NotBeNull();
            siteAdtSettings.SiteId.Should().Be(site.SiteId);
            siteAdtSettings.AdxDatabase.Should().Be(site.AdxDatabase);
            siteAdtSettings.SiteCodeForModelId.Should().Be(site.SiteCodeForModelId);
        }

        [Theory]
        [AutoData]
        public async Task GetSite_Should_Return_404_When_Site_Not_Exists(Guid siteId)
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            using var client = server.CreateClient(null);

            var result = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetFromJsonAsync<SiteAdtSettings>($"admin/sites/{siteId}"));

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
