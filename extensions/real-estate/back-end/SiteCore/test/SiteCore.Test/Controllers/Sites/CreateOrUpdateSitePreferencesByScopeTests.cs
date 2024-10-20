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

namespace SiteCore.Test.Controllers.Sites
{
    public class CreateOrUpdateSitePreferencesByScopeTests : BaseInMemoryTest
    {
        public CreateOrUpdateSitePreferencesByScopeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenEmptyInput_CreateOrUpdateSitePreferencesByScope_ReturnsNoContent()
        {
            var scopeId = "scope-dtid";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"scopes/{scopeId}/preferences", new { });
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task GivenProfle_CreateOrUpdateSitePreferencesByScope_ReturnsNoContent()
        {
            var scopeId = "scope-dtid";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var request = new { TimeMachine = new {} };

                var response = await client.PutAsJsonAsync($"scopes/{scopeId}/preferences", request);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var dbContext = server.Assert().GetDbContext<SiteDbContext>();
                dbContext.SitePreferences.Should().HaveCount(1);
                var sitePreference = dbContext.SitePreferences.First();
                sitePreference.ScopeId.Should().Be(scopeId);
                sitePreference.SiteId.Should().Be(Guid.Empty);
            }
        }
    }
}
