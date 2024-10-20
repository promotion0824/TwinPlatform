using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;

namespace WorkflowCore.Test.Features.NotificationReceivers
{
    public class GetReceiversTests : BaseInMemoryTest
    {
        public GetReceiversTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetReceivers_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/notificationReceivers");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GivenSiteHasReceivers_GetReceivers_ReturnsSiteReceivers()
        {
            var siteId = Guid.NewGuid();
            var receiverEntities = Fixture.Build<NotificationReceiverEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .CreateMany(10);
            var expectedReceivers = NotificationReceiverDto.MapFromModels(NotificationReceiverEntity.MapToModels(receiverEntities));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.NotificationReceivers.AddRange(receiverEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/notificationReceivers");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<NotificationReceiverDto>>();
                result.Should().BeEquivalentTo(expectedReceivers);
            }
        }
    }
}
