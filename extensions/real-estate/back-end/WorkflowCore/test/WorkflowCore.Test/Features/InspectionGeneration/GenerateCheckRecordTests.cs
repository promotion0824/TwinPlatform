using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net;
using WorkflowCore.Entities;
using System;
using System.Net.Http;
using System.Linq;

using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Models;

namespace WorkflowCore.Test.Features.InspectionGeneration
{
    public partial class GenerateCheckRecordTests : BaseInMemoryTest
    {
        private Guid _siteId1       = Guid.NewGuid();
        private Guid _inspectionId1 = Guid.NewGuid();

        public GenerateCheckRecordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("",                    "",                    CheckRecordStatus.Completed,   CheckRecordStatus.Due)]
        [InlineData("2020-01-01T00:00:00", "",                    CheckRecordStatus.Completed,   CheckRecordStatus.NotRequired)]
        [InlineData("2020-01-01T00:00:00", "2040-01-01T00:00:00", CheckRecordStatus.Completed,   CheckRecordStatus.NotRequired)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Missed,      CheckRecordStatus.Overdue)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Due,         CheckRecordStatus.Overdue)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Overdue,     CheckRecordStatus.Overdue)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Completed,   CheckRecordStatus.Due)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.NotRequired, CheckRecordStatus.Due)]
        public async Task GenerateCheckRecord_success(string pauseStartDate, string pauseEndDate, CheckRecordStatus lastRecordStatus, CheckRecordStatus expectedStatus)
		{
			var utcNow  = DateTime.UtcNow;
            var checkId = Guid.NewGuid();
            var lastCheckRecordId = Guid.NewGuid();
            var request = new GenerateCheckRecordRequest
            {
                InspectionId       = _inspectionId1,
                InspectionRecordId = Guid.NewGuid(),
                CheckId            = checkId,
                SiteId             = _siteId1,
                EffectiveDate      = utcNow,
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(new InspectionEntity
                {
                    Id = _inspectionId1
                });
                db.Checks.Add(new CheckEntity
                {
                    Id             = checkId,
                    InspectionId   = _inspectionId1,
                    LastRecordId   = lastCheckRecordId,
                    TypeValue = Guid.NewGuid().ToString(),
                    PauseStartDate = string.IsNullOrWhiteSpace(pauseStartDate) ? null : DateTime.Parse(pauseStartDate),
                    PauseEndDate   = string.IsNullOrWhiteSpace(pauseEndDate) ? null : DateTime.Parse(pauseEndDate)
                });
                db.CheckRecords.Add(new CheckRecordEntity
                {
                    Id           = lastCheckRecordId,
                    CheckId      = checkId,
                    InspectionId = _inspectionId1,
                    Status       = lastRecordStatus
                });
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("scheduledinspection/generate/check", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<GenerateCheckRecordDto>();

                db = server.Assert().GetDbContext<WorkflowContext>();

                db.CheckRecords.Should().HaveCount(2);
                var check = db.Checks.First(c => c.Id == checkId);
                db.Checks.Select(x => x.LastRecordId).First().Should().Be(result.Id);
                db.CheckRecords.Where( r=> r.Id == lastCheckRecordId).Select(x => x.Status).First().Should().Be(result.Status == CheckRecordStatus.Overdue ? CheckRecordStatus.Missed : lastRecordStatus );
                db.CheckRecords.First(c => c.Id == result.Id).TypeValue.Should().Be(check.TypeValue);
                Assert.Equal(lastCheckRecordId, result.LastRecordId);
                Assert.Equal(expectedStatus, result.Status);
            }
        }

        private void SetupSite(ServerFixture server)
        { 
            server.Arrange().GetSiteApi()
                .SetupRequestSequence(HttpMethod.Get, $"sites/{_siteId1}")
                .ReturnsJson(new Site { Id = _siteId1, CustomerId = Guid.NewGuid(), TimezoneId = "Pacific Standard Time", Features = new SiteFeatures { IsInspectionEnabled = true } } );
        }
    }
}
