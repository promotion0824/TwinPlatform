using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;

namespace WorkflowCore.Test.Features.Settings
{
    public class GetSiteExtensionsTests : BaseInMemoryTest
    {
        public GetSiteExtensionsTests(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task TokenIsNotGiven_GetSiteExtensions_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/settings");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task SiteExtensionsExists_GetSiteExtensions_ReturnThisSiteExtensions()
        {
            var siteExtensions = Fixture.Build<SiteExtensionEntity>().Create();
            
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.SiteExtensions.AddRange(siteExtensions);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteExtensions.SiteId}/settings");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteExtensionsDto>();
            }
        }

        [Fact]
        public async Task NoSiteExtensions_GetSiteExtensions_ReturnDefaultSiteExtensions()
        {
            var siteId = Guid.NewGuid();
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"sites/{siteId}/settings");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
