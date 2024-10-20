using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.MarketPlace.AppCategories
{
    public class GetAppCategoriesTests : BaseInMemoryTest
    {
        public GetAppCategoriesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AuthNotProvided_GetAppCategories_ReturnsUnauthorized()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync($"appCategories");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task CategoriesExist_GetAppCategories_ReturnsUnauthorized()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(new [] { "Admin" }, null, string.Empty))
            {
                var existingCategories = Fixture.Build<AppCategory>().CreateMany(5);
                var arrangement = server.Arrange();
                arrangement.GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"categories")
                    .ReturnsJson(AppCategoryDto.MapFrom(existingCategories));

                var response = await client.GetAsync($"appCategories");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<AppCategoryDto>>();
                result.Should().HaveCount(5);
                result.Should().BeEquivalentTo(AppCategoryDto.MapFrom(existingCategories));
            }
        }
    }
}