using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class UpdateUserPreferencesTests : BaseInMemoryTest
    {
        public UpdateUserPreferencesTests(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task GivenValidInput_UpdatePreferences_ReturnsNoContent()
        {
            var request = new { profile = new {} };
            var user = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler
                    .SetupRequest(HttpMethod.Put, $"customers/{user.CustomerId}/users/{user.Id}/preferences")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"me/preferences", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task GivenLanguageOnlyInput_UpdatePreferences_ReturnsNoContent()
        {
            var request = new { language = "en" };
            var user = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler
                    .SetupRequest(HttpMethod.Put, $"customers/{user.CustomerId}/users/{user.Id}/preferences")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync("me/preferences", request);
                
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
