using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Batch;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class GetSitesPagedTests : BaseInMemoryTest
    {
        public GetSitesPagedTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserCanViewSites_GetSites_ReturnsThoseSites()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>()
                .With(x => x.CustomerId, customerId)
                .Create();
            var expectedSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures())
                .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
                .CreateMany()
                .ToList();
            var batchedExpectedSites = new BatchDto<Site>()
            {
                After = 1,
                Before = 0,
                Items = expectedSites.ToArray(),
                Total = 2
            };
                       
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler
                    .SetupRequest(HttpMethod.Post, $"users/{user.Id}/sites/paged")
                    .ReturnsJson(batchedExpectedSites);

                var expectedSiteDetailDtos = SiteMiniDto.Map(expectedSites, server.Assert().GetImageUrlHelper());

                var response = await client.PostAsJsonAsync("/v2/me/sites", new BatchRequestDto());

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SiteMiniDto>>();

                result.After.Should().Be(1);
                result.Before.Should().Be(0);
                result.Items.Should().BeEquivalentTo(expectedSiteDetailDtos);
                result.Total.Should().Be(2);
            }
        }
    }
}
