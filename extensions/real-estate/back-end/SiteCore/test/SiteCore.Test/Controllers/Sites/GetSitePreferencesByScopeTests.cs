using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System;
using SiteCore.Entities;
using System.Net.Http.Json;
using Castle.Core.Resource;
using System.Linq;
using SiteCore.Dto;
using System.Net.Http;
using SiteCore.Domain;
using System.Security.Policy;
using System.Text.Json;

namespace SiteCore.Test.Controllers.Sites
{
    public class GetSitePreferencesByScopeTests : BaseInMemoryTest
    {
        public GetSitePreferencesByScopeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task NoSitePreferences_GetSitePreferencesByScope_ReturnsEmptyResult()
        {
            var scopeId = "scope-dtid";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"scopes/{scopeId}/preferences");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task SitePreferencesExist_CreateOrUpdateSitePreferencesByScope_ReturnsSitePreferences()
        {
            var scopeId = "scope-dtid";
            var sitePreference = Fixture.Build<SitePreferencesEntity>()
                .Without(x => x.ModuleGroups)
                .With(x => x.ScopeId, scopeId)
                .With(x => x.TimeMachine, "{\"favorites\":[]}")
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.SitePreferences.Add(sitePreference);
                db.SaveChanges();

                var response = await client.GetAsync($"scopes/{scopeId}/preferences");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SitePreferences>();
                JsonSerializer.Serialize(result.TimeMachine).Should().Be("{\"favorites\":[]}");
            }
        }
    }
}
