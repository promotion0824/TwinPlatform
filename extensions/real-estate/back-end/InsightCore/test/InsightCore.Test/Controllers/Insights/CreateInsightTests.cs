using AutoFixture;
using FluentAssertions;
using InsightCore.Controllers.Requests;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using InsightCore.Test.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Notifications.Models;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights
{
    public class CreateInsightTests : BaseInMemoryTest
    {
        public CreateInsightTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(OldInsightStatus.Open)]
        [InlineData(OldInsightStatus.InProgress)]
        [InlineData(OldInsightStatus.Acknowledged)]
        [InlineData(OldInsightStatus.Closed)]
        public async Task RequestProvided_CreateInsight_InsightAreCreatedAndStatusIsNew(OldInsightStatus oldStatus)
        {
            var siteId = Guid.NewGuid();

            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .With(i => i.Status, oldStatus)
                                 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();

                var createdInsight = InsightEntity.MapTo(db.Insights.Single(c => c.Id == result.Id));
                createdInsight.Points.Should().BeEquivalentTo(request.Points);

                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");

                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.StatusLog.Where(c => c.InsightId == result.Id).Count().Should().Be(1);
                TestContainer.NotificationService.Verify(s => s.SendNotificationAsync(It.IsAny<NotificationMessage>()));
            }
        }

        [Fact]
        public async Task RequestProvided_CreateInsight_TwinIdHasValue_InsightAreCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Without(i => i.Points)
                                 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.TwinName.Should().Be(twinSimpleResponse.Name);
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.StatusLog.Count(c => c.InsightId == result.Id).Should().Be(1);
            }
        }
        [Fact]
        public async Task RequestProvided_CreateInsight_LookUpMissingTwinDetail_InsightAreCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();

            var expectedTwinId = "TwinId-Ms-3";
            var request = Fixture.Build<CreateInsightRequest>()
                .With(c => c.TwinId, expectedTwinId)
                .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                .Without(i=>i.PrimaryModelId)
                .Without(i => i.Points)
                .Create();

            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, expectedTwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {

                server.Arrange().GetDigitalTwinApi()
                        .SetupRequest(HttpMethod.Post, $"sites/Assets/names")
                        .ReturnsJson(new List<TwinSimpleDto>{ twinSimpleResponse});

                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(expectedTwinId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.PrimaryModelId.Should().Be(twinSimpleResponse.ModelId);
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.StatusLog.Count(c => c.InsightId == result.Id).Should().Be(1);
            }
        }
        [Fact]
        public async Task RequestProvidedAndInsightNumberExist_CreateInsight_InsightAreCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();

            var siteCode = Guid.NewGuid().ToString().Substring(0, 5);
            var existingSequenceNumber = 999;
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, siteCode)
                                 .Without(i => i.Points)
                                 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();
                db.InsightNextNumbers.Add(new InsightNextNumberEntity() { Prefix = siteCode, NextNumber = existingSequenceNumber });
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-{existingSequenceNumber}");
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
            }
        }

        [Theory]
        [InlineData(SourceType.Inspection)]
        [InlineData(SourceType.App)]
        public async Task RequestProvided_UniqueInsightExists_StatusIsOpenAndStateIsActive_InsightUpdatedAndReturnUpdatedInsight(SourceType source)
        {
            var siteId = Guid.NewGuid();
            var twinId = Guid.NewGuid().ToString();
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SourceType, source)
                                 .With(c => c.TwinId, twinId)
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Without(i => i.Points)
                                 .Create();
            var existingInsight = Fixture.Build<InsightEntity>()
                                            .With(i => i.SiteId, siteId)
                                            .With(i => i.Name, request.Name)
                                            .With(c => c.TwinId, twinId)
                                            .With(i => i.Description, request.Description.ToUpperInvariant())
                                            .With(i => i.Status, InsightStatus.Open)
                                            .With(i => i.State, InsightState.Active)
                                            .Without(i => i.PointsJson)
                                            .Without(i => i.ImpactScores)
                                            .Without(x => x.InsightOccurrences)
                                            .Without(x => x.StatusLogs)
                                            .Create();

            var expectedImpactScoreEntity = new ImpactScoreEntity()
            {
                Id = Guid.NewGuid(),
                InsightId = existingInsight.Id,
                Name = "cost",
                FieldId = "cost_id",
                Value = 14.45,
                Unit = "$"
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                await db.Insights.AddAsync(existingInsight);
                await db.ImpactScores.AddAsync(expectedImpactScoreEntity);
                await db.SaveChangesAsync();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.OccurrenceCount.Should().Be(request.OccurrenceCount);
                result.OccurredDate.Should().BeMoreThan(utcNow.TimeOfDay);
                result.ImpactScores.Should().BeEquivalentTo(new List<ImpactScore> { ImpactScoreEntity.MapTo(expectedImpactScoreEntity) });
                result.TwinId.Should().Be(existingInsight.TwinId);
                result.EquipmentId.Should().Be(existingInsight.EquipmentId);
            }
        }

        [Fact]
        public async Task RequestProvidedWithSourceInspection_InsightExistsButDifferentDescription_StatusIsOpenAndStateIsActive_InsightCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SourceType, SourceType.Inspection)
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Without(i => i.Points)
                                 .Create();

            var existingInsight = Fixture.Build<InsightEntity>()
                                            .With(i => i.SiteId, siteId)
                                            .With(i => i.Name, request.Name)
                                            .With(i => i.TwinId, request.TwinId)
                                            .With(i => i.Status, InsightStatus.Open)
                                            .With(i => i.State, InsightState.Active)
                                            .Without(i => i.PointsJson)
                                            .Without(i => i.ImpactScores)
                                            .Without(x => x.InsightOccurrences)
                                            .Without(x => x.StatusLogs)
                                            .Create();

            request.ImpactScores.ForEach(x => x.RuleId = existingInsight.RuleId);

            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
            }
        }

        [Fact]
        public async Task RequestProvided_UniqueInsightExists_StatusIsNotOpenAndStateIsActive_InsightCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Without(i => i.Points)
                                 .Create();
            var existingInsight = Fixture.Build<InsightEntity>()
                                            .With(i => i.Name, request.Name)
                                            .With(i => i.TwinId, request.TwinId)
                                            .With(i => i.Status, InsightStatus.Ignored)
                                            .With(i => i.State, InsightState.Active)
                                            .With(i => i.PointsJson)
                                            .Without(i => i.ImpactScores)
                                            .Without(x => x.InsightOccurrences)
                                            .Without(x => x.StatusLogs)
                                            .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.StatusLog.Count(c => c.InsightId == result.Id).Should().Be(1);
            }
        }

        [Fact]
        public async Task RequestProvided_UniqueInsightExists_StatusIsOpenAndStateIsNotActive_InsightCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();

            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Without(i => i.Points)
                                 .Create();
            var existingInsight = Fixture.Build<InsightEntity>()
                                            .With(i => i.Name, request.Name)
                                            .With(i => i.TwinId, request.TwinId)
                                            .With(i => i.Status, InsightStatus.Open)
                                            .Without(i => i.PointsJson)
                                            .With(i => i.State, InsightState.Archived)
                                            .Without(i => i.ImpactScores)
                                            .Without(x => x.InsightOccurrences)
                                            .Without(x => x.StatusLogs)
                                            .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.StatusLog.Count(c => c.InsightId == result.Id).Should().Be(1);
            }
        }


        [Theory]
        [InlineData(InsightType.Alert)]
        [InlineData(InsightType.Calibration)]
        [InlineData(InsightType.Comfort)]
        [InlineData(InsightType.Commissioning)]
        [InlineData(InsightType.DataQuality)]
        [InlineData(InsightType.Diagnostic)]
        [InlineData(InsightType.EdgeDevice)]
        [InlineData(InsightType.Energy)]
        [InlineData(InsightType.EnergyKpi)]
        [InlineData(InsightType.Fault)]
        [InlineData(InsightType.GoldenStandard)]
        [InlineData(InsightType.Note)]
        public async Task RequestProvidedWithSourceInspection_RuleIdAndNameAreEmpty_CreatedInsightAndSetTheRuleIdAndNameIfTypeIsAlert(InsightType type)
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SourceType, SourceType.Inspection)
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .With(i=>i.Type, type)
                                 .Without(i=>i.RuleName)
                                 .Without(i=>i.RuleId)
                                 .Without(i => i.Points)
                                 .Create();

           
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.RuleId.Should().Be(type==InsightType.Alert?  "inspection-value-out-of-range-":null);
                result.RuleName.Should().Be(type == InsightType.Alert ? "Inspection Value Out of Range":null);

            }
        }

        [Fact]
        public async Task RequestProvided_CreateInsight_LocationHasValue_InsightAreCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Without(i => i.Points)
                                 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.TwinName.Should().Be(twinSimpleResponse.Name);
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.StatusLog.Count(c => c.InsightId == result.Id).Should().Be(1);
                db.InsightLocations.Where(c => c.InsightId == result.Id).Select(c=>c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);
            }
        }

        [Fact]
        public async Task RequestProvided_CreateInsight_TagIsNull_InsightAreCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Without(i => i.Points)
                                 .Without(i=>i.Tags)
                                 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.Status);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.InsightOccurrences);
                    config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
                    return config;
                });
                result.Status.Should().Be(OldInsightStatus.Open);
                result.LastStatus.Should().Be(InsightStatus.New);
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.TwinName.Should().Be(twinSimpleResponse.Name);
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.StatusLog.Count(c => c.InsightId == result.Id).Should().Be(1);
                db.InsightLocations.Where(c => c.InsightId == result.Id).Select(c => c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);
            }
        }
    }
}
