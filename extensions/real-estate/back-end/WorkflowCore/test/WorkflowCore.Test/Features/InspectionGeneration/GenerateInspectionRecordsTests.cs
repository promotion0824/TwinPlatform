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
using WorkflowCore.Models;

namespace WorkflowCore.Test.Features.InspectionGeneration
{
    public partial class GenerateInspectionRecordsTests : BaseInMemoryTest
    {
        public GenerateInspectionRecordsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenInspections_GenerateInspectionRecords_OnlyValidInspectionsWhoseNextEffectiveDateAreBeforePresentTimeWillBeGenerated()
        {
            var utcNow = DateTime.UtcNow;
            var inspectionEntities = Fixture.Build<InspectionEntity>()
                                            .With(x => x.StartDate, utcNow.AddDays(-1))
                                            .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
                                            .With(x => x.Frequency, 1)
                                            .With(x => x.EndDate, (DateTime?)null)
                                            .With(x => x.IsArchived, false)
                                            .With(x => x.SiteId, siteId)
                                            .Without(i => i.FrequencyDaysOfWeekJson)
                                            .Without(x => x.Zone)
                                            .Without(x => x.Checks)
                                            .Without(x => x.LastRecord)
                                            .CreateMany(10)
                                            .ToList();
            inspectionEntities.ForEach(c=>c.Checks= Fixture.Build<CheckEntity>()
                .With(x => x.InspectionId, c.Id)
                .With(x => x.IsArchived, false)
                .Without(x => x.LastRecordId)
                .Without(x => x.LastRecord)
                .Without(x => x.LastSubmittedRecordId)
                .Without(x => x.LastSubmittedRecord)
                .CreateMany(2)
                .ToList());
          
            var archivedInspectionEntities = Fixture.Build<InspectionEntity>()
                                                    .With(x => x.StartDate, utcNow.AddDays(-1))
                                                    .With(x => x.EndDate, (DateTime?)null)
                                                    .With(x => x.SiteId, siteId)
                                                    .With(x => x.IsArchived, true)
                                                    .Without(i => i.FrequencyDaysOfWeekJson)
                                                    .Without(x => x.Zone)
                                                    .Without(x => x.Checks)
                                                    .Without(x => x.LastRecord)
                                                    .CreateMany(15)
                                                    .ToList();

            var futureInspectionEntities = Fixture.Build<InspectionEntity>()
                                                  .With(x => x.StartDate, utcNow.AddDays(1))
												  .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
												  .With(x => x.Frequency, 8)
											      .With(x => x.EndDate, (DateTime?)null)
												  .With(x => x.IsArchived, false)
                                                  .With(x => x.SiteId, siteId)
                                                  .Without(i => i.FrequencyDaysOfWeekJson)
                                                  .Without(x => x.Zone)
                                                  .Without(x => x.Checks)
                                                  .Without(x => x.LastRecord)
                                                  .CreateMany(20);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.AddRange(inspectionEntities);
                db.Inspections.AddRange(archivedInspectionEntities);
                db.Inspections.AddRange(futureInspectionEntities);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<GenerationResult>();

                result.Inspections.Should().HaveCount(inspectionEntities.Count);

                db = server.Assert().GetDbContext<WorkflowContext>();

                db.InspectionRecords.Should().HaveCount(inspectionEntities.Count);
                db.InspectionRecords.Select(x => x.InspectionId).Should().BeEquivalentTo(inspectionEntities.Select(x => x.Id));
            }
        }

        [Fact]
        public async Task InspectionHasValidChecks_GenerateInspectionRecords_CheckRecordsAreGenerated()
        {
            var utcNow = DateTime.UtcNow;
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.StartDate, utcNow.AddDays(-1))
                                          .With(x => x.EndDate, (DateTime?)null)
                                          .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
                                          .With(x => x.Frequency, 1)
                                          .With(x => x.SiteId, siteId)
                                          .With(x => x.IsArchived, false)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.Checks)
                                          .Without(x => x.LastRecord)
                                          .Create();
            var checkEntities = Fixture.Build<CheckEntity>()
                                       .With(x => x.InspectionId, inspectionEntity.Id)
                                       .With(x => x.IsArchived, false)
                                       .Without(x => x.LastRecordId)
                                       .Without(x => x.LastRecord)
                                       .Without(x => x.LastSubmittedRecordId)
                                       .Without(x => x.LastSubmittedRecord)
                                       .CreateMany()
                                       .ToList();
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();
                db.Checks.AddRange(checkEntities);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<GenerationResult>();
                result.Inspections.Should().HaveCount(1);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.CheckRecords.Should().HaveCount(checkEntities.Count);
                db.CheckRecords.Select(x => x.CheckId).Should().BeEquivalentTo(checkEntities.Select(x => x.Id));
            }
        }

        [Fact]
        public async Task InspectionHasValidCheck_GenerateInspectionRecords_InspectionAndCheckAreUpdated()
        {
            var utcNow = DateTime.UtcNow;
            var nextEffectiveDate = utcNow.AddHours(-2);
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.StartDate, utcNow.AddDays(-1))
                                          .With(x => x.EndDate, (DateTime?)null)
                                          .With(x => x.IsArchived, false)
                                           .With(x => x.SiteId, siteId)
                                         .With(x => x.Frequency, 1)
                                         .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
                                         .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.Checks)
                                          .Without(x => x.LastRecordId)
                                          .Without(x => x.LastRecord)
                                          .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.IsArchived, false)
                                     .Without(x => x.LastRecordId)
                                     .Without(x => x.LastRecord)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Create();
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();
                db.Checks.Add(checkEntity);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                var updatedInspectionEntity = db.Inspections.First();
                var inspectionRecord = db.InspectionRecords.First();
                updatedInspectionEntity.LastRecordId.Should().Be(inspectionRecord.Id);
                var updatedCheckEntity = db.Checks.First();
                var checkRecord = db.CheckRecords.First();
                updatedCheckEntity.LastRecordId.Should().Be(checkRecord.Id);
            }
        }

        [Fact]
        public async Task InspectionMissedManyGenerationCalls_GenerateInspectionRecordsForManyTimes_AllMissedRecordsWillBeGenerated()
        {
            var startDate = new DateTime(2021, 1, 3,  0, 0, 0, DateTimeKind.Unspecified);
            var now       = new DateTime(2021, 3, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var utcNow    = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);

            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.StartDate, startDate)
                                          .With(x => x.EndDate, (DateTime?)null)
                                          .With(X => X.SiteId, siteId)
                                          .With(x => x.Frequency, 1)
                                          .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
                                          .With(x => x.IsArchived, false)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.Checks)
                                          .Without(x => x.LastRecordId)
                                          .Without(x => x.LastRecord)
                                          .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.IsArchived, false)
                                     .Without(x => x.LastRecordId)
                                     .Without(x => x.LastRecord)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Create();
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();
                db.Checks.Add(checkEntity);
                db.SaveChanges();

                bool continueGenerating = true;
                while (continueGenerating)
                {
                    var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());
                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var result = await response.Content.ReadAsAsync<GenerationResult>();
                    continueGenerating = result.Inspections.Count > 0;
                }

                db = server.Assert().GetDbContext<WorkflowContext>();
                db.InspectionRecords.Should().HaveCount(1);
                db.CheckRecords.Should().HaveCount(1);
            }
        }




		[Fact]
		public async Task InspectionGeneration_GenerateInspectionRecordInspectionRecordAlreadyGenerated_NoInspectionShouldBeGenerated()
		{
			var startDate = new DateTime(2022, 12, 5, 1, 45, 0, DateTimeKind.Utc);
			var utcNow = new DateTime(2022, 12, 6, 1, 50, 0, DateTimeKind.Utc);

			var inspectionEntity = Fixture.Build<InspectionEntity>()
										  .With(x => x.StartDate, startDate)
										  .With(x => x.EndDate, (DateTime?)null)
										  .With(X => X.SiteId, siteId)
										  .With(x => x.Frequency, 8)
										  .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
										  .With(x => x.IsArchived, false)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.Checks)
										  .Without(x => x.LastRecordId)
										  .Without(x => x.LastRecord)
										  .Create();
			var checkEntity = Fixture.Build<CheckEntity>()
									 .With(x => x.InspectionId, inspectionEntity.Id)
									 .With(x => x.IsArchived, false)
									 .Without(x => x.LastRecordId)
									 .Without(x => x.LastRecord)
									 .Without(x => x.LastSubmittedRecordId)
									 .Without(x => x.LastSubmittedRecord)
									 .Create();
			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				SetupSite(server);

				server.Arrange().SetCurrentDateTime(utcNow);
				var db = server.Arrange().CreateDbContext<WorkflowContext>();
				db.Inspections.Add(inspectionEntity);
				db.SaveChanges();
				db.Checks.Add(checkEntity);
				db.SaveChanges();


				var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<GenerationResult>();

				db = server.Assert().GetDbContext<WorkflowContext>();
				db.InspectionRecords.Should().HaveCount(1);

				utcNow = new DateTime(2022, 12, 5, 2, 0, 0, DateTimeKind.Utc);
				server.Arrange().SetCurrentDateTime(utcNow);

				response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				result = await response.Content.ReadAsAsync<GenerationResult>();

				db = server.Assert().GetDbContext<WorkflowContext>();
				db.InspectionRecords.Should().HaveCount(1);
			}
		}
	}
}
