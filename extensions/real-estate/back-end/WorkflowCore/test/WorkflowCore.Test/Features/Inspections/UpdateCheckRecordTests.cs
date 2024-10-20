using AutoFixture;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Controllers.Responses;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Inspections
{
    public class UpdateCheckRecordTests : BaseInMemoryTest
    {
        public UpdateCheckRecordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(CheckType.Numeric, 1.23, "should not be saved", 1.23, null)]
        [InlineData(CheckType.Total, 7.89, "should not be saved", 7.89, null)]
        [InlineData(CheckType.List, 99.9, "itemA", null, "itemA")]
        public async Task CheckRecordExists_UpdateCheckRecord_CheckRecordIsUpdated(
            CheckType checkType,
            double? inputtedNumberValue, string inputtedStringValue,
            double? expectedNumberValue, string expectedStringValue)
        {
            var userId = Guid.NewGuid();
            var timeZoneId = "AUS Eastern Standard Time";
            var inspectionId = Guid.NewGuid();
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .Without(x => x.Checks)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.LastRecord)
                                          .Without(x => x.Zone)
                                          .Create();
            var inspectionRecordEntity = Fixture.Build<InspectionRecordEntity>()
                                            .With(x => x.Id, inspectionEntity.LastRecordId.Value)
                                            .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.Type, checkType)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Without(x => x.LastRecord)
                                     .Create();
            var checkRecordEntity = Fixture.Build<CheckRecordEntity>()
                                           .With(x => x.InspectionId, inspectionEntity.Id)
                                           .With(x => x.CheckId, checkEntity.Id)
                                           .With(x => x.InspectionRecordId, inspectionEntity.LastRecordId.Value)
                                           .With(x => x.Attachments, "[]")
                                           .Without(x => x.NumberValue)
                                           .Without(x => x.StringValue)
                                           .Create();
            var zoneEntity = Fixture.Build<ZoneEntity>()
                                .With(x => x.Id, inspectionEntity.ZoneId)
                                .With(x => x.SiteId, inspectionEntity.SiteId)
                                .Create();
            var request = Fixture.Build<SubmitCheckRecordRequest>()
                                 .With(x => x.NumberValue, inputtedNumberValue)
                                 .With(x => x.StringValue, inputtedStringValue)
                                 .With(x => x.SubmittedUserId, userId)
                                 .With(x => x.TimeZoneId, timeZoneId)
                                 .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.Add(zoneEntity);
                db.Checks.Add(checkEntity);
                db.Inspections.Add(inspectionEntity);
                db.InspectionRecords.Add(inspectionRecordEntity);
                db.CheckRecords.Add(checkRecordEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync(
                    $"sites/{inspectionEntity.SiteId}/inspections/{inspectionEntity.Id}/{inspectionEntity.LastRecordId.Value}/checkRecords/{checkRecordEntity.Id}",
                    request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.CheckRecords.First().NumberValue.Should().Be(expectedNumberValue);
                db.CheckRecords.First().StringValue.Should().Be(expectedStringValue);
                db.CheckRecords.First().Notes.Should().Be(request.Notes);
                db.Checks.First().LastSubmittedRecordId.Should().Be(checkRecordEntity.Id);
            }
        }

        [Theory]
        [InlineData(CheckType.Numeric, null, null, 1.0, null, null)]
        [InlineData(CheckType.Numeric, 10.0, 20.0, 15.0, null, null)]
        [InlineData(CheckType.Numeric, 1.1, null, 1.0, null, SubmitCheckRecordResponse.InsightType.Alert)]
        [InlineData(CheckType.Numeric, null, 2.9, 3.0, null, SubmitCheckRecordResponse.InsightType.Alert)]
        [InlineData(CheckType.Numeric, null, null, 2.0, "some notes", SubmitCheckRecordResponse.InsightType.Note)]
        [InlineData(CheckType.Numeric, null, 2.0, 99.0, "some notes", SubmitCheckRecordResponse.InsightType.Alert)]
        public async Task InputValueWillTriggerInsightGeneration_UpdateCheckRecord_InsightInformationIsReturned(
            CheckType checkType, double? checkMinValue, double? checkMaxValue,
            double? inputtedNumberValue, string inputtedNotes,
            SubmitCheckRecordResponse.InsightType? expectedInsightType)
        {
            var userId = Guid.NewGuid();
            var attachments = Fixture.CreateMany<AttachmentBase>(5).ToList();
            var timeZoneId = "AUS Eastern Standard Time";
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .Without(x => x.Checks)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.LastRecord)
                                          .Without(x => x.Zone)
                                          .Create();
            var inspectionRecordEntity = Fixture.Build<InspectionRecordEntity>()
                                            .With(x => x.Id, inspectionEntity.LastRecordId.Value)
                                            .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.Type, checkType)
                                     .With(x => x.MinValue, checkMinValue)
                                     .With(x => x.MaxValue, checkMaxValue)
                                     .With(x => x.CanGenerateInsight, true)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Without(x => x.LastRecord)
                                     .Create();
            var checkRecordEntity = Fixture.Build<CheckRecordEntity>()
                                           .With(x => x.InspectionId, inspectionEntity.Id)
                                           .With(x => x.CheckId, checkEntity.Id)
                                           .With(x => x.InspectionRecordId, inspectionEntity.LastRecordId.Value)
                                           .With(x => x.Attachments, "[]")
                                           .Without(x => x.NumberValue)
                                           .Without(x => x.StringValue)
                                           .Create();
            var zoneEntity = Fixture.Build<ZoneEntity>()
                .With(x => x.Id, inspectionEntity.ZoneId)
                .With(x => x.SiteId, inspectionEntity.SiteId)
                .Create();
            var request = new SubmitCheckRecordRequest
            {
                NumberValue = inputtedNumberValue,
                StringValue = null,
                Notes = inputtedNotes,
                SubmittedUserId = userId,
                TimeZoneId = timeZoneId,
                Attachments = attachments

            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.Add(zoneEntity);
                db.Inspections.Add(inspectionEntity);
                db.InspectionRecords.Add(inspectionRecordEntity);
                db.Checks.Add(checkEntity);
                db.CheckRecords.Add(checkRecordEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync(
                    $"sites/{inspectionEntity.SiteId}/inspections/{inspectionEntity.Id}/{inspectionEntity.LastRecordId.Value}/checkRecords/{checkRecordEntity.Id}",
                    request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SubmitCheckRecordResponse>();
                if (expectedInsightType.HasValue)
                {
                    result.RequiredInsight.Should().NotBeNull();
                    result.RequiredInsight.TwinId.Should().Be(inspectionEntity.TwinId);
					result.RequiredInsight.Priority.Should().Be(3);
                    result.RequiredInsight.Type.Should().Be(expectedInsightType);
                }
                else
                {
                    result.RequiredInsight.Should().BeNull();
                }
            }
        }

        [Theory]
        [InlineData(CheckType.Numeric, 1.1, null, 1.0, null)]
        [InlineData(CheckType.Numeric, null, 2.9, 3.0, null)]
        [InlineData(CheckType.Numeric, null, null, 2.0, "some notes")]
        [InlineData(CheckType.Numeric, null, 2.0, 99.0, "some notes")]
        public async Task InputValueWillTriggerInsightGenerationButCheckCannotGenerateInsight_UpdateCheckRecord_NoInsightInformationReturned(
            CheckType checkType, double? checkMinValue, double? checkMaxValue,
            double? inputtedNumberValue, string inputtedNotes)
        {
            var userId = Guid.NewGuid();
            var timeZoneId = "AUS Eastern Standard Time";
            var attachments = Fixture.CreateMany<AttachmentBase>(3).ToList();
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .Without(x => x.Checks)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.LastRecord)
                                          .Without(x => x.Zone)
                                          .Create();
            var inspectionRecordEntity = Fixture.Build<InspectionRecordEntity>()
                                            .With(x => x.Id, inspectionEntity.LastRecordId.Value)
                                            .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.Type, checkType)
                                     .With(x => x.MinValue, checkMinValue)
                                     .With(x => x.MaxValue, checkMaxValue)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Without(x => x.LastRecord)
                                     .With(x => x.CanGenerateInsight, false)
                                     .Create();
            var checkRecordEntity = Fixture.Build<CheckRecordEntity>()
                                           .With(x => x.InspectionId, inspectionEntity.Id)
                                           .With(x => x.CheckId, checkEntity.Id)
                                           .With(x => x.InspectionRecordId, inspectionEntity.LastRecordId.Value)
                                           .With(x => x.Attachments, "[]")
                                           .Without(x => x.NumberValue)
                                           .Without(x => x.StringValue)
                                           .Create();
            var zoneEntity = Fixture.Build<ZoneEntity>()
                .With(x => x.Id, inspectionEntity.ZoneId)
                .With(x => x.SiteId, inspectionEntity.SiteId)
                .Create();
            var request = new SubmitCheckRecordRequest
            {
                NumberValue = inputtedNumberValue,
                StringValue = null,
                Notes = inputtedNotes,
                SubmittedUserId = userId,
                TimeZoneId = timeZoneId,
                Attachments = attachments
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.Add(zoneEntity);
                db.Inspections.Add(inspectionEntity);
                db.InspectionRecords.Add(inspectionRecordEntity);
                db.Checks.Add(checkEntity);
                db.CheckRecords.Add(checkRecordEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync(
                    $"sites/{inspectionEntity.SiteId}/inspections/{inspectionEntity.Id}/{inspectionEntity.LastRecordId.Value}/checkRecords/{checkRecordEntity.Id}",
                    request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SubmitCheckRecordResponse>();
                result.RequiredInsight.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(CheckType.Numeric, "Yes", "Yes", CheckRecordStatus.NotRequired, CheckRecordStatus.Due)]
        [InlineData(CheckType.Total, "Yes", "Yes", CheckRecordStatus.Due, CheckRecordStatus.Due)]
        [InlineData(CheckType.Date, "No", "Yes", CheckRecordStatus.Due, CheckRecordStatus.NotRequired)]
        public async Task CheckRecordExists_UpdateCheckRecord_DependentRecordStatusIsUpdated(
            CheckType checkType,
            string inputtedStringValue, string dependentValue,
            CheckRecordStatus dependentRecordStatus, CheckRecordStatus expectedDependentRecordStatus)
        {
            var userId = Guid.NewGuid();
            var timeZoneId = "AUS Eastern Standard Time";
            var inspectionId = Guid.NewGuid();
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .Without(x => x.Checks)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.LastRecord)
                                          .Without(x => x.Zone)
                                          .Create();
            var inspectionRecordEntity = Fixture.Build<InspectionRecordEntity>()
                                            .With(x => x.Id, inspectionEntity.LastRecordId.Value)
                                            .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.Type, CheckType.List)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Without(x => x.LastRecord)
                                     .Create();
            var dependentCheckEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.Type, checkType)
                                     .With(x => x.DependencyId, checkEntity.Id)
                                     .With(x => x.DependencyValue, dependentValue)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Without(x => x.LastRecord)
                                     .Create();
            var checkRecordEntity = Fixture.Build<CheckRecordEntity>()
                                           .With(x => x.InspectionId, inspectionEntity.Id)
                                           .With(x => x.CheckId, checkEntity.Id)
                                           .With(x => x.InspectionRecordId, inspectionEntity.LastRecordId.Value)
                                           .With(x => x.Attachments, "[]")
                                           .Without(x => x.NumberValue)
                                           .Without(x => x.StringValue)
                                           .Create();
            var dependentCheckRecordEntity = Fixture.Build<CheckRecordEntity>()
                                           .With(x => x.InspectionId, inspectionEntity.Id)
                                           .With(x => x.CheckId, dependentCheckEntity.Id)
                                           .With(x => x.InspectionRecordId, inspectionEntity.LastRecordId.Value)
                                           .With(x => x.Attachments, "[]")
                                           .With(x => x.Status, dependentRecordStatus)
                                           .Without(x => x.NumberValue)
                                           .Without(x => x.StringValue)
                                           .Create();
            var zoneEntity = Fixture.Build<ZoneEntity>()
                                .With(x => x.Id, inspectionEntity.ZoneId)
                                .With(x => x.SiteId, inspectionEntity.SiteId)
                                .Create();
            var request = Fixture.Build<SubmitCheckRecordRequest>()
                                 .With(x => x.StringValue, inputtedStringValue)
                                 .With(x => x.SubmittedUserId, userId)
                                 .With(x => x.TimeZoneId, timeZoneId)
                                 .Without(x => x.Attachments)
                                 .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.Add(zoneEntity);
                db.Inspections.Add(inspectionEntity);
                db.InspectionRecords.Add(inspectionRecordEntity);
                db.Checks.Add(checkEntity);
                db.Checks.Add(dependentCheckEntity);
                db.CheckRecords.Add(checkRecordEntity);
                db.CheckRecords.Add(dependentCheckRecordEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync(
                    $"sites/{inspectionEntity.SiteId}/inspections/{inspectionEntity.Id}/{inspectionEntity.LastRecordId.Value}/checkRecords/{checkRecordEntity.Id}",
                    request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.CheckRecords.First(x => x.CheckId == dependentCheckEntity.Id).Status.Should().Be(expectedDependentRecordStatus);
            }
        }
    }
}
