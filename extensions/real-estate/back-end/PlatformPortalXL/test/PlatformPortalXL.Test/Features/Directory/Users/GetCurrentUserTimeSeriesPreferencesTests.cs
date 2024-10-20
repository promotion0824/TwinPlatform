using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class GetCurrentUserTimeSeriesPreferencesTests : BaseInMemoryTest
    {
        public GetCurrentUserTimeSeriesPreferencesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CurrentUserExists_GetCurrentUserTimeSeriesPreferences_ReturnsTimeSeriesPreferences()
        {
            var user = Fixture.Create<User>();
            var timeSeries = new
            {
                state = new { },
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
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/users/{user.Id}/preferences/timeSeries")
                    .ReturnsJson(timeSeries);

                var response = await client.GetAsync($"me/preferences/timeSeries");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerUserTimeSeriesDto>();
                result.Should().NotBeNull();
            }
        }

    }
}