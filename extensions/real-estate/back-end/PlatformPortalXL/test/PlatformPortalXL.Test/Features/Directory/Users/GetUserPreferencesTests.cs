using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users;

public class GetUserPreferencesTests : BaseInMemoryTest
{
    public GetUserPreferencesTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UnauthorizedUser_GetUserPreferences_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync("me/preferences");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }

    [Fact]
    public async Task ValidInput_GetUserPreferences_ReturnsUserPreferences()
    {
       
        var user = Fixture.Create<User>();

        var userPreferences = new UserPreferencesResponse
        {
            Language = "en",
            TimeZone = Guid.NewGuid()
        };
        var jsonResponse = JsonSerializer.Serialize(userPreferences);

        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, user.Id))
        {
            var handler = server.Arrange().GetDirectoryApi();
            handler
                .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                .ReturnsJson(user);
            handler
                .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/users/{user.Id}/preferences")
                .ReturnsJson(userPreferences);

            var response = await client.GetAsync("me/preferences");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UserPreferencesResponse>();
            result.Should().BeEquivalentTo(userPreferences);
        }
    }

    record UserPreferencesResponse
    {
        public string Language { get; init; }
        public Guid TimeZone { get; init; }
    }
}

