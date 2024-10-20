using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class CreateOrUpdateUserTimeSeriesPreferencesTests : BaseInMemoryTest
    {
        public CreateOrUpdateUserTimeSeriesPreferencesTests(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task GivenValidInput_UpdatePreferences_ReturnsNoContent()
        {
            var user = Fixture.Create<User>();
            var request = new
            {
                state = new {},
                recentAssets = new { },
                exportedCsvs = new { },
                favorites = new { },
            };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler
                    .SetupRequest(HttpMethod.Put, $"customers/{user.CustomerId}/users/{user.Id}/preferences/timeSeries")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"me/preferences/timeSeries", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
