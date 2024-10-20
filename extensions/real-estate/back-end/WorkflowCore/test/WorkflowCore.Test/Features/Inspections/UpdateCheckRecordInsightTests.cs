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
using Willow.Infrastructure;

namespace WorkflowCore.Test.Features.Inspections
{
    public class UpdateCheckRecordInsightTests : BaseInMemoryTest
    {
        public UpdateCheckRecordInsightTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CheckRecordExists_UpdateCheckRecordInsight_InsightIdIsUpdated()
        {
            var siteId = Guid.NewGuid();
            var checkRecordEntity = Fixture.Build<CheckRecordEntity>().With(x => x.Attachments, "[]").Create();
            var request = Fixture.Create<UpdateCheckRecordInsightRequest>();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.CheckRecords.Add(checkRecordEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync(
                    $"sites/{siteId}/inspections/{checkRecordEntity.InspectionId}/lastRecord/checkRecords/{checkRecordEntity.Id}/insight",
                    request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.CheckRecords.First().InsightId.Should().Be(request.InsightId);
            }
        }

        [Fact]
        public async Task CheckRecordDoesNotExist_UpdateCheckRecordInsight_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync(
                    $"sites/{siteId}/inspections/{Guid.NewGuid()}/lastRecord/checkRecords/{Guid.NewGuid()}/insight",
                    new UpdateCheckRecordInsightRequest());

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
