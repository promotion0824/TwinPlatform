using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using WorkflowCore.Controllers.Request;
using FluentAssertions;
using System.Net;
using Willow.Infrastructure;
using WorkflowCore.Models;
using WorkflowCore.Entities;
using System.Net.Http;
using WorkflowCore.Dto;
using System.Linq;

namespace WorkflowCore.Test.Features.Inspections
{
    public class CreateInspectionTests : BaseInMemoryTest
    {
        public CreateInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InvalidFrequency_CreateInspection_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateInspectionRequest>()
                                    .With(i => i.Frequency, -1)
                                    .With(i => i.FrequencyUnit, SchedulingUnit.Hours)
                                    .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("Frequency", out _));
            }
        }

        [Fact]
        public async Task InvalidDependency_CreateInspection_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var inspectionRequest = Fixture.Build<CreateInspectionRequest>()
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

            var thirdCheckRequest = Fixture.Build<CheckRequest>()
                                    .With(c => c.Type, CheckType.Date)
                                    .Without(c => c.DecimalPlaces)
                                    .Without(c => c.DependencyName)
                                    .Without(c => c.DependencyValue)
                                    .Create();

            inspectionRequest.Checks = new List<CheckRequest> { firstCheckRequest, secondCheckRequest, thirdCheckRequest };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections", inspectionRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("DependencyName", out _));
            }
        }

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("TwinId_MN")]
		public async Task ValidInput_CreateInspection_ReturnsCreatedInspection(string requestedTwinId)
        {
            var siteId = Guid.NewGuid();
            var inspectionRequest = Fixture.Build<CreateInspectionRequest>()
                                    .With(i => i.Frequency, 8)
                                    .With(i=>i.TwinId,requestedTwinId)
                                    .With(i => i.FrequencyUnit, SchedulingUnit.Hours)
                                    .Without(i => i.Checks)
                                    .Create();

            var firstCheckRequest = Fixture.Build<CheckRequest>()
                                                .With(c => c.Type, CheckType.List)
                                                .With(c => c.TypeValue, "Yes|No|Auto")
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .Create();

            var secondCheckRequest = Fixture.Build<CheckRequest>()
                                                .With(c => c.Type, CheckType.Numeric)
                                                .With(c => c.DecimalPlaces, 2)
                                                .With(c => c.DependencyName, firstCheckRequest.Name)
                                                .With(c => c.DependencyValue, "No")
                                                .Create();

            var thirdCheckRequest = Fixture.Build<CheckRequest>()
                                                .With(c => c.Type, CheckType.Date)
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .Create();

            inspectionRequest.Checks = new List<CheckRequest> { firstCheckRequest, secondCheckRequest, thirdCheckRequest };
            var assetTwinIds = new List<TwinIdDto>()
            {
	            new TwinIdDto
	            {
		            Id ="TwinId-Ms-2",
		            UniqueId = inspectionRequest.AssetId.ToString()
				}
            };
			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
	            server.Arrange().GetDigitalTwinApi()
		            .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={inspectionRequest.AssetId}")
		            .ReturnsJson(assetTwinIds);
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections", inspectionRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Inspections.Should().HaveCount(1);
                db.Checks.Should().HaveCount(3);
                var createdInspection = db.Inspections.First();
                createdInspection.Id.Should().Be(result.Id);
                createdInspection.SiteId.Should().Be(siteId);
                createdInspection.Name.Should().Be(inspectionRequest.Name);
                createdInspection.FloorCode.Should().Be(inspectionRequest.FloorCode);
                createdInspection.ZoneId.Should().Be(inspectionRequest.ZoneId);
                createdInspection.AssetId.Should().Be(inspectionRequest.AssetId);
                createdInspection.TwinId.Should().Be( string.IsNullOrWhiteSpace(requestedTwinId)?  assetTwinIds.First().Id:requestedTwinId);
                createdInspection.AssignedWorkgroupId.Should().Be(inspectionRequest.AssignedWorkgroupId);
                createdInspection.Frequency.Should().Be(inspectionRequest.Frequency);
                createdInspection.FrequencyUnit.Should().Be(inspectionRequest.FrequencyUnit);
                createdInspection.StartDate.Should().Be(inspectionRequest.StartDate);
                createdInspection.EndDate.Should().Be(inspectionRequest.EndDate);
                var firstCreatedCheck = db.Checks.First();
                firstCreatedCheck.Id.Should().Be(result.Checks.First().Id);
                firstCreatedCheck.InspectionId.Should().Be(result.Id);
                firstCreatedCheck.SortOrder.Should().Be(1);
                firstCreatedCheck.Name.Should().Be(firstCheckRequest.Name);
                firstCreatedCheck.Type.Should().Be(firstCheckRequest.Type);
                firstCreatedCheck.DecimalPlaces.Should().Be(0);
                firstCreatedCheck.MinValue.Should().Be(firstCheckRequest.MinValue);
                firstCreatedCheck.MaxValue.Should().Be(firstCheckRequest.MaxValue);
                firstCreatedCheck.DependencyId.Should().BeNull();
                firstCreatedCheck.DependencyValue.Should().Be(firstCheckRequest.DependencyValue);
                firstCreatedCheck.PauseStartDate.Should().Be(firstCheckRequest.PauseStartDate);
                firstCreatedCheck.PauseEndDate.Should().Be(firstCheckRequest.PauseEndDate);
                firstCreatedCheck.CanGenerateInsight.Should().Be(firstCheckRequest.CanGenerateInsight);
                var secondCreatedCheck = db.Checks.Skip(1).First();
                secondCreatedCheck.Id.Should().Be(result.Checks.Skip(1).First().Id);
                secondCreatedCheck.InspectionId.Should().Be(result.Id);
                secondCreatedCheck.SortOrder.Should().Be(2);
                secondCreatedCheck.Name.Should().Be(secondCheckRequest.Name);
                secondCreatedCheck.Type.Should().Be(secondCheckRequest.Type);
                secondCreatedCheck.DecimalPlaces.Should().Be(2);
                secondCreatedCheck.MinValue.Should().Be(secondCheckRequest.MinValue);
                secondCreatedCheck.MaxValue.Should().Be(secondCheckRequest.MaxValue);
                secondCreatedCheck.DependencyId.Should().Be(firstCreatedCheck.Id);
                secondCreatedCheck.DependencyValue.Should().Be(secondCheckRequest.DependencyValue);
                secondCreatedCheck.PauseStartDate.Should().Be(secondCheckRequest.PauseStartDate);
                secondCreatedCheck.PauseEndDate.Should().Be(secondCheckRequest.PauseEndDate);
                secondCreatedCheck.CanGenerateInsight.Should().Be(secondCheckRequest.CanGenerateInsight);
                var thirdCreatedCheck = db.Checks.Last();
                thirdCreatedCheck.Id.Should().Be(result.Checks.Last().Id);
                thirdCreatedCheck.InspectionId.Should().Be(result.Id);
                thirdCreatedCheck.SortOrder.Should().Be(3);
                thirdCreatedCheck.Name.Should().Be(thirdCheckRequest.Name);
                thirdCreatedCheck.Type.Should().Be(thirdCheckRequest.Type);
                var expectedInspection = InspectionEntity.MapToModel(createdInspection); ;
                expectedInspection.Checks = CheckEntity.MapToModels(db.Checks.ToList());
                var expectedInspectionDto = InspectionDto.MapFromModel(expectedInspection);
                result.Should().BeEquivalentTo(expectedInspectionDto);
            }
        }
    }
}
