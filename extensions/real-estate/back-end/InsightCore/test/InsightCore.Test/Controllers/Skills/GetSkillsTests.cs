using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights
{

    public class GetSkillsTests : BaseInMemoryTest
    {
        public GetSkillsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SkillsExist_ReturnsThoseSkills()
        {

            var insightEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5)
                .ToList();
 

            var expectedSkills = insightEntities.Select(x => new SkillDto()
            {
                Id = x.RuleId,
                Category = x.Type,
                Name = x.RuleName
            }).DistinctBy(c =>c.Id);


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(insightEntities);

                db.SaveChanges();

                var request = new BatchRequestDto();

                var response = await client.PostAsJsonAsync($"skills", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
                result.Items.Should().BeEquivalentTo(expectedSkills);
            }
        }

        [Fact]
        public async Task SkillsExist_FilterByName_ReturnsThoseSkills()
        {
            var expectedId = "skill1";
            var insightEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .With(i => i.RuleName, expectedId)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2)
                .ToList();
            insightEntities.AddRange(Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .With(i => i.RuleName,$"test_{expectedId} test")
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2)
                .ToList());
            var extraEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .With(i => i.RuleName, "skill2")
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5)
                .ToList();

            var expectedSkills = insightEntities.Select(x => new SkillDto()
            {
                Id = x.RuleId,
                Category = x.Type,
                Name = x.RuleName
            }).DistinctBy(c =>  c.Id);


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(insightEntities);
                await db.Insights.AddRangeAsync(extraEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "RuleName",
                            Operator = FilterOperators.Contains,
                            Value = expectedId
                        }
                    }
                };


                var response = await client.PostAsJsonAsync($"skills", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
                result.Items.Should().BeEquivalentTo(expectedSkills);
            }
        }

        [Fact]
        public async Task SkillsExist_FilterByCategory_ReturnsThoseSkills()
        {
            var category =InsightType.Energy;
            var insightEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .With(i => i.Type, category)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(1)
                .ToList();
            insightEntities.AddRange(Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(10)
                .ToList());
          

            var expectedSkills = insightEntities.Where(c=>c.Type==category).Select(x => new SkillDto()
            {
                Id = x.RuleId,
                Category = x.Type,
                Name = x.RuleName
            }).DistinctBy(c => new { c.Id, c.Category, c.Name });


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(insightEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "Type",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = category
                        }
                    }
                };


                var response = await client.PostAsJsonAsync($"skills", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
                result.Items.Should().BeEquivalentTo(expectedSkills);
            }
        }
        [Fact]
        public async Task SkillsExist_FilterById_ReturnsThoseSkills()
        {
            var expectedId = "skill1";
            var insightEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .With(i=>i.RuleId, expectedId)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5)
                .ToList();

            var extraEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .With(i => i.RuleId, "skill2")
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5)
                .ToList();

            var expectedSkills = insightEntities.Select(x => new SkillDto()
            {
                Id = x.RuleId,
                Category = x.Type,
                Name = x.RuleName
            }).DistinctBy(c =>c.Id);


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(insightEntities);
                await db.Insights.AddRangeAsync(extraEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "RuleId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = expectedId
                        }
                    }
                };


                var response = await client.PostAsJsonAsync($"skills", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
                result.Items.Should().BeEquivalentTo(expectedSkills);
            }
        }

        [Fact]
        public async Task SkillsExist_GetPaginatedResponse_ReturnsThoseSkills()
        {
            var insightEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(10)
                .ToList();

            var expectedSkills = insightEntities.Skip(3).Take(3).Select(x => new SkillDto()
            {
                Id = x.RuleId,
                Category = x.Type,
                Name = x.RuleName
            }).DistinctBy(c =>  c.Id);


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(insightEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                  Page = 2,
                  PageSize = 3
                };


                var response = await client.PostAsJsonAsync($"skills", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
                result.Items.Should().BeEquivalentTo(expectedSkills);
            }
        }

        [Fact]
        public async Task SkillsExist_RuleIdNull_ReturnsThoseSkillsWithDefaultValue()
        {
            var insightEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(10)
                .ToList();
            insightEntities.AddRange(Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .Without(i => i.RuleId)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));
            var expectedSkills = insightEntities.Select(x => new SkillDto()
            {
                Id = x.RuleId,
                Category = x.Type,
                Name = x.RuleName
            }).DistinctBy(c => c.Id).ToList();

            expectedSkills.Where(x => x.Id == null).ToList().ForEach(c =>
            {
                c.Id = "inspection_note_";
                c.Name = "Inspection Note";

            });
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(insightEntities);
                db.SaveChanges();

                var request = new BatchRequestDto();

                var response = await client.PostAsJsonAsync($"skills", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
                result.Items.Should().BeEquivalentTo(expectedSkills);
            }
        }

        [Fact]
        public async Task SkillsExist_SameRuleIdWithMultipleCategory_ReturnsThoseSkillsWithFirstCategory()
        {
            var ruleId = "ruleId";
            var insightEntities = Fixture.Build<InsightEntity>()
                .With(i => i.Type, InsightType.Alert)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(10)
                .ToList();
            insightEntities.AddRange(Fixture.Build<InsightEntity>()
                .With(i => i.RuleId,ruleId)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));
            var expectedSkills = insightEntities.Select(x => new SkillDto()
            {
                Id = x.RuleId,
                Category = x.Type,
                Name = x.RuleName
            }).DistinctBy(c => c.Id).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(insightEntities);
                db.SaveChanges();

                var request = new BatchRequestDto();

                var response = await client.PostAsJsonAsync($"skills", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
                result.Items.Should().BeEquivalentTo(expectedSkills);
            }
        }
    }
}
