using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Inspections
{
    public class UpdateInspectionTests : BaseInMemoryTest
    {
        public UpdateInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InvalidFrequency_UpdateInspection_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInspectionRequest>()
                                    .With(i => i.Frequency, -1)
                                    .With(i => i.FrequencyUnit, SchedulingUnit.Hours)
                                    .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("Frequency", out _));
            }
        }

        [Fact]
        public async Task InvalidDependency_UpdateInspection_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var inspectionRequest = Fixture.Build<UpdateInspectionRequest>()
                                    .With(i => i.Frequency, 8)
                                    .With(i => i.FrequencyUnit, SchedulingUnit.Hours)
                                    .Without(i => i.Checks)
                                    .Create();
            var firstCheckRequest = Fixture.Build<CheckRequest>()
                                        .Without(c => c.Name)
                                        .Without(c => c.Type)
                                        .Without(c => c.TypeValue)
                                        .Without(c => c.DecimalPlaces)
                                        .Without(c => c.DependencyName)
                                        .Without(c => c.DependencyValue)
                                        .Create();

            var secondCheckRequest = Fixture.Build<CheckRequest>()
                                                .With(c => c.Type, CheckType.Numeric)
                                                .Without(c => c.DecimalPlaces)
                                                .Create();

            inspectionRequest.Checks = new List<CheckRequest> { firstCheckRequest, secondCheckRequest };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}", inspectionRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("DependencyName", out _));
            }
        }

        [Fact]
        public async Task ValidInput_UpdateInspection_ReturnsUpdatedInspection()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();

            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.SiteId, siteId)
                                            .With(i=> i.IsArchived, false)
                                            .With(i => i.StartDate, DateTime.Parse("2000-01-01", null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal))
                                            .Without(i => i.FrequencyDaysOfWeekJson)
                                            .Without(x => x.Zone)
                                            .Without(i => i.EndDate)
                                            .Without(i => i.Checks)
                                            .Without(i => i.LastRecord)
                                            .Create();

            var checkEntities = Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, inspectionEntity.Id)
                                        .CreateMany(3).ToList();

            var inspectionRequest = Fixture.Build<UpdateInspectionRequest>()
                                    .With(i => i.Frequency, 8)
                                    .With(i => i.FrequencyUnit, SchedulingUnit.Hours)
                                    .Without(i => i.Checks)
                                    .Create();

            var firstCheckRequest = Fixture.Build<CheckRequest>()
                                                .Without(c => c.Id)
                                                .With(c => c.Type, CheckType.List)
                                                .With(c => c.TypeValue, "Yes|No|Auto")
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .Create();

            var secondCheckRequest = Fixture.Build<CheckRequest>()
                                                .With(c => c.Id, checkEntities[0].Id)
                                                .With(c => c.Type, CheckType.Numeric)
                                                .With(c => c.DecimalPlaces, 2)
                                                .With(c => c.DependencyName, firstCheckRequest.Name)
                                                .With(c => c.DependencyValue, "No")
                                                .Create();
            inspectionRequest.Checks = new List<CheckRequest> { firstCheckRequest, secondCheckRequest };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.Checks.AddRange(checkEntities);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}", inspectionRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();
                db.Inspections.Should().HaveCount(1);
                var updatedInspection = db.Inspections.First();
                updatedInspection.Id.Should().Be(result.Id);
                updatedInspection.SiteId.Should().Be(siteId);
                updatedInspection.Name.Should().Be(inspectionRequest.Name);
                updatedInspection.AssignedWorkgroupId.Should().Be(inspectionRequest.AssignedWorkgroupId);
                updatedInspection.Frequency.Should().Be(inspectionRequest.Frequency);
                updatedInspection.FrequencyUnit.Should().Be(inspectionRequest.FrequencyUnit);
                updatedInspection.StartDate.Should().Be(inspectionRequest.StartDate);
                updatedInspection.EndDate.Should().Be(inspectionRequest.EndDate);
                db.Checks.Should().HaveCount(4);
                var addedCheck = db.Checks.First(c => !checkEntities.Select(x => x.Id).Contains(c.Id));
                addedCheck.Id.Should().Be(result.Checks.First().Id);
                addedCheck.InspectionId.Should().Be(inspectionId);
                addedCheck.SortOrder.Should().Be(1);
                addedCheck.Name.Should().Be(firstCheckRequest.Name);
                addedCheck.Type.Should().Be(firstCheckRequest.Type);
                addedCheck.TypeValue.Should().Be(firstCheckRequest.TypeValue);
                addedCheck.DecimalPlaces.Should().Be(0);
                addedCheck.MinValue.Should().Be(firstCheckRequest.MinValue);
                addedCheck.MaxValue.Should().Be(firstCheckRequest.MaxValue);
                addedCheck.DependencyId.Should().BeNull();
                addedCheck.DependencyValue.Should().Be(firstCheckRequest.DependencyValue);
                addedCheck.PauseStartDate.Should().Be(firstCheckRequest.PauseStartDate);
                addedCheck.PauseEndDate.Should().Be(firstCheckRequest.PauseEndDate);
                addedCheck.CanGenerateInsight.Should().Be(firstCheckRequest.CanGenerateInsight);
                var updatedCheck = db.Checks.First(c => c.Id == secondCheckRequest.Id);
                updatedCheck.Id.Should().Be(result.Checks.Skip(1).First().Id);
                updatedCheck.InspectionId.Should().Be(inspectionId);
                updatedCheck.SortOrder.Should().Be(2);
                updatedCheck.Name.Should().Be(secondCheckRequest.Name);
                updatedCheck.TypeValue.Should().Be(secondCheckRequest.TypeValue);
                updatedCheck.DecimalPlaces.Should().Be(2);
                updatedCheck.MinValue.Should().Be(secondCheckRequest.MinValue);
                updatedCheck.MaxValue.Should().Be(secondCheckRequest.MaxValue);
                updatedCheck.DependencyId.Should().Be(addedCheck.Id);
                updatedCheck.DependencyValue.Should().Be(secondCheckRequest.DependencyValue);
                updatedCheck.PauseStartDate.Should().Be(secondCheckRequest.PauseStartDate);
                updatedCheck.PauseEndDate.Should().Be(secondCheckRequest.PauseEndDate);
                updatedCheck.CanGenerateInsight.Should().Be(secondCheckRequest.CanGenerateInsight);
                var archivedChecks = db.Checks.Where(c => c.IsArchived).ToList();
                archivedChecks.Should().HaveCount(2);
                archivedChecks.Select(c => c.Id).Should().BeEquivalentTo(checkEntities.Where(c => c.Id != updatedCheck.Id).Select(c => c.Id));
                var expectedInspection = InspectionEntity.MapToModel(updatedInspection); ;
                expectedInspection.Checks = CheckEntity.MapToModels(db.Checks.Where(c => !c.IsArchived).OrderBy(c => c.SortOrder).ToList());
                var expectedInspectionDto = InspectionDto.MapFromModel(expectedInspection);
                result.Should().BeEquivalentTo(expectedInspectionDto);
            }
        }
    }
}
