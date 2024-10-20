using System;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net;
using WorkflowCore.Entities;
using System.Linq;
using System.Collections.Generic;

namespace WorkflowCore.Test.Features.Inspections
{
    public class UpdateInspectionSortOrderTests : BaseInMemoryTest
    {
        public UpdateInspectionSortOrderTests(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task InspectionsExist_UpdateInspectionSortOrder_ReturnNoContent()
        {
            var siteId = Guid.NewGuid();
            var zoneId = Guid.NewGuid();
            var inspectionEntities = Fixture.Build<InspectionEntity>()
                .With(x => x.SiteId, siteId)
                .With(x => x.ZoneId, zoneId)
                .Without(i => i.FrequencyDaysOfWeekJson)
                .Without(x => x.Checks)
                .Without(x => x.Zone)
                .CreateMany(3)
                .ToList();
            var request = new UpdateInspectionSortOrderRequest()
            {
                InspectionIds = new List<Guid>
                {
                    inspectionEntities[0].Id,
                    inspectionEntities[1].Id,
                    inspectionEntities[2].Id
                }
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.AddRange(inspectionEntities);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/zones/{zoneId}/inspections/sortOrder", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Inspections.First().SortOrder.Should().Be(0);
                db.Inspections.Last().SortOrder.Should().Be(2);
            }
        }
    }
}
