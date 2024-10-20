using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net;
using WorkflowCore.Entities;
using System;
using AutoFixture;
using System.Net.Http;
using WorkflowCore.Services;
using System.Linq;

using Newtonsoft.Json;

using Willow.Calendar;

using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Models;

namespace WorkflowCore.Test.Features.InspectionGeneration
{
    public partial class GenerateInspectionRecordForInspectionTests : BaseInMemoryTest
    {
        private Guid _siteId1       = Guid.NewGuid();
        private Guid _inspectionId1 = Guid.NewGuid();

        public GenerateInspectionRecordForInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GenerateInspectionRecordForInspection_success()
        {
            var utcNow = DateTime.UtcNow;
            var request = new GenerateInspectionRecordRequest
            {
                InspectionId  = _inspectionId1,
                SiteId        = _siteId1,
                HitTime       = utcNow,
                SiteNow       = utcNow.InTimeZone("Pacific Standard Time"),
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
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("scheduledinspection/generate", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InspectionRecordDto>();

                db = server.Assert().GetDbContext<WorkflowContext>();

                db.InspectionRecords.Should().HaveCount(1);
                db.InspectionRecords.Select(x => x.InspectionId).First().Should().Be(_inspectionId1);
                db.Inspections.Select(x => x.LastRecordId).First().Should().Be(result.Id);

                Assert.Equal(_inspectionId1, result.InspectionId);
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
