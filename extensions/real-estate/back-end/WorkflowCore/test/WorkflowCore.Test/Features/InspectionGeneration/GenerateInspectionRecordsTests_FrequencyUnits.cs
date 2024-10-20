using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
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
using System.Collections.Generic;
using System.Collections;

namespace WorkflowCore.Test.Features.InspectionGeneration
{
    public partial class GenerateInspectionRecordsTests : BaseInMemoryTest
    {

		class InspectionGenerationTestData : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] { new DateTime(2022, 4, 20, 17, 30, 0, DateTimeKind.Utc),
											new DateTime(2022, 4, 16, 10, 25, 0, DateTimeKind.Utc),
											SchedulingUnit.Days,
											4};

				yield return new object[] { new DateTime(2022, 4, 20, 17, 30, 0, DateTimeKind.Utc),
											new DateTime(2022, 4, 6, 10, 25, 0, DateTimeKind.Utc),
											SchedulingUnit.Weeks,
											2};

				yield return new object[] { new DateTime(2020, 2, 29, 16, 0, 0, DateTimeKind.Utc),
											new DateTime(2019, 12, 31, 8, 0, 0, DateTimeKind.Utc),
											SchedulingUnit.Months,
											1};

				yield return new object[] { new DateTime(2020, 1, 1, 16, 0, 0, DateTimeKind.Utc),
											new DateTime(2010, 1, 1, 8, 0, 0, DateTimeKind.Utc),
											SchedulingUnit.Years,
											5};

				yield return new object[] {new DateTime(2021, 2, 28, 16, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 2, 29, 8, 0, 0, DateTimeKind.Utc),
											SchedulingUnit.Years,
											1};

			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}


		[Theory]
		[ClassData(typeof(InspectionGenerationTestData))]
		public async Task InspectionHasValidChecks_GenerateInspectionRecordsDaysFrequency_CheckRecordsAreGenerated(
			DateTime utcNow,
			DateTime startDate,
			SchedulingUnit frequencyUnit,
			int frequency
			)
        {
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.StartDate, startDate)
                                          .With(x => x.EndDate, (DateTime?)null)
                                          .With(x => x.FrequencyUnit, frequencyUnit)
                                          .With(x => x.Frequency, frequency)
                                          .With(X => X.SiteId, siteId)
                                          .With(X => X.IsArchived, false)
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
                db.Inspections.RemoveRange(db.Inspections.ToList());
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
    }
}
