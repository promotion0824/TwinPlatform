using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using InsightCore.Models;
using Newtonsoft.Json;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights;

public class GetInsightDiagnosticTest : BaseInMemoryTest
{
    public GetInsightDiagnosticTest(ITestOutputHelper output) : base(output)
    {
    }
    [Fact]
    public async Task GetInsightDiagnostic_InsightNotExist_ReturnsNotFound()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var response = await client.GetAsync($"insights/{Guid.NewGuid()}/occurrences/diagnostics?start={DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy")}&end={DateTime.Now.ToString("MM/dd/yyyy")}&interval={00.00:10:00}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetInsightDiagnostic_InsightHasNoDependencies_ReturnsEmpty()
    {
        var siteId = Guid.NewGuid();
        var insightId = Guid.NewGuid();
        var expectedInsightEntity = Fixture.Build<InsightEntity>()
            .With(x => x.SiteId, siteId)
            .With(x => x.Id, insightId)
            .Without(x=> x.PointsJson)
            .Without(x => x.ImpactScores)
            .Without(x=>x.Dependencies)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            await db.Insights.AddAsync(expectedInsightEntity);
            db.SaveChanges();

            var expectedResponse = new List<InsightDiagnosticDto>();

            var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy")}&end={DateTime.Now.ToString("MM/dd/yyyy")}&interval={00.00:10:00}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<InsightDiagnosticDto>>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }


    [Fact]
    public async Task GetInsightDiagnostic_InsightHasDependencies_ReturnsResponse()
    {
        var siteId = Guid.NewGuid();
        var insightId = Guid.NewGuid();
        var startDate = DateTime.Parse("2023-10-19 12:00:00 AM");
        var endDate = DateTime.Parse("2023-11-03 12:00:00 AM");
        var expectedData = GetExpectedData(insightId, siteId, startDate, endDate, "00.10:00:00");
        var expectedInsightEntityAndDependent = expectedData.entities;
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            await db.Dependencies.AddRangeAsync(expectedInsightEntityAndDependent.Where(c => c.Dependencies != null)
                .SelectMany(c => c.Dependencies));
            await db.Insights.AddRangeAsync(expectedInsightEntityAndDependent);
            db.SaveChanges();

            var expectedResponse = expectedData.response;

            var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={startDate.ToString("MM/dd/yyyy")}&end={endDate.ToString("MM/dd/yyyy")}&interval=00.10:00:00");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<InsightDiagnosticDto>>();
            foreach (var resultItem in result)
            {
                var expectedItem = expectedResponse.FirstOrDefault(c => c.Id == resultItem.Id);
                expectedItem.Should().NotBeNull();
                resultItem.OccurrenceLiveData.TimeSeriesData.Count.Should().Be(expectedItem.OccurrenceLiveData.TimeSeriesData.Count);
            }

        }
    }

    [Theory]
    [InlineData("2023-10-03 12:00:00 AM", "00.12:00:00")]
    [InlineData("2023-10-30 12:01:00 AM", "")]
    [InlineData("2023-09-03 12:00:00 AM", "00.24:00:00")]
    public async Task GetInsightDiagnostic_InsightHasDependencies_InvalidEndDateOrInterval_ReturnsResponse(string endDateString,string interval)
    {
        var siteId = Guid.NewGuid();
        var insightId = Guid.NewGuid();
        var startDate = DateTime.Parse("2023-10-30 12:00:00 AM");
        var endDate = DateTime.Parse(endDateString);
        var expectedData = GetExpectedData(insightId, siteId, startDate, endDate, interval);
        var expectedInsightEntityAndDependent = expectedData.entities;
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            await db.Dependencies.AddRangeAsync(expectedInsightEntityAndDependent.Where(c => c.Dependencies != null)
                .SelectMany(c => c.Dependencies));
            await db.Insights.AddRangeAsync(expectedInsightEntityAndDependent);
            db.SaveChanges();

            var expectedResponse = expectedData.response;

            var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={startDate.ToString("MM/dd/yyyy")}&end={endDate.ToString("MM/dd/yyyy")}&interval={interval}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<InsightDiagnosticDto>>();
            var json = JsonConvert.SerializeObject(result);
            foreach (var resultItem in result)
            {
                var expectedItem = expectedResponse.FirstOrDefault(c => c.Id == resultItem.Id);
                expectedItem.Should().NotBeNull();
                resultItem.OccurrenceLiveData.TimeSeriesData.Count.Should().Be(expectedItem.OccurrenceLiveData.TimeSeriesData.Count);
            }

        }
    }

    [Theory]
    [InlineData("2023-10-03 12:00:00 AM", "")]
    [InlineData("", "2023-10-30 12:00:00 AM")]
    [InlineData("", "")]
    public async Task GetInsightDiagnostic_InsightHasDependencies_NullEndDateOrStartDate_ReturnsBadRequest(string startDateString, string endDateString)
    {
        var siteId = Guid.NewGuid();
        var insightId = Guid.NewGuid();
        DateTime? startDate = startDateString==""?null: DateTime.Parse(startDateString);
        DateTime? endDate = endDateString == "" ? null : DateTime.Parse(endDateString);
       
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var url = $"insights/{insightId}/occurrences/diagnostics?interval=0.10:00:00";
            url += startDate.HasValue ? $"&start={startDate.Value.ToString("MM/dd/yyyy")}" : "";
            url+= endDate.HasValue ? $"&start={endDate.Value.ToString("MM/dd/yyyy")}" : "";
            var response = await client.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
          
        }
    }
    private (List<InsightDiagnosticDto> response,List<InsightEntity> entities) GetExpectedData(Guid insightId, Guid siteId,DateTime start,DateTime end,string requestedInterval)
    {
        var parent = Fixture.Build<InsightEntity>()
            .With(x => x.SiteId, siteId)
            .With(x => x.Id, insightId)
            .Without(x => x.PointsJson)
            .Without(x => x.ImpactScores)
            .Without(x => x.Dependencies)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .Create();

        parent.InsightOccurrences = new List<InsightOccurrenceEntity>
        {
            new() { Started = start.AddDays(-20), Ended = start.AddDays(-10), IsFaulted = false },
            new() { Started = end.AddDays(20), Ended = end.AddDays(30), IsFaulted = true },
            new() { Started = start.AddDays(5), Ended = start.AddDays(10), IsFaulted = true },
            new() { Started = start.AddDays(-5), Ended = start.AddDays(3), IsFaulted = true },
            new() { Started = end.AddDays(-5), Ended = end.AddDays(3), IsFaulted = true },
            new() { Started = start.AddDays(11), Ended = start.AddDays(13), IsFaulted = false },
            new() { Started = DateTime.Parse("2023-01-05 12:00:00 AM"), Ended = DateTime.Parse("2023-01-06 12:00:00 AM"), IsFaulted = false }
        };
        var parentDependent = Fixture.Build<InsightEntity>()
            .With(x => x.SiteId, siteId)
            .Without(x => x.PointsJson)
            .Without(x => x.ImpactScores)
            .Without(x => x.Dependencies)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs).CreateMany(3).ToList();
        parent.Dependencies = new List<DependencyEntity>();
        foreach (var dependent in parentDependent)
        {
            parent.Dependencies.Add(Fixture.Build<DependencyEntity>().With(x=>x.FromInsightId,parent.Id)
                .With(x=>x.ToInsightId,dependent.Id).Without(c=>c.FromInsight).Without(c=>c.ToInsight).Create());
        }
        parentDependent[0].InsightOccurrences = new List<InsightOccurrenceEntity>
        {
            new() { Started = start.AddDays(5), Ended = start.AddDays(6), IsFaulted = true },
            new() { Started = start.AddDays(7), Ended = start.AddDays(10), IsFaulted = false },
            new() { Started = start.AddDays(-5), Ended = start.AddDays(1), IsFaulted = true }
        };

        parentDependent[1].InsightOccurrences = new List<InsightOccurrenceEntity>
        {
            new() { Started = start.AddDays(5), Ended = start.AddDays(10), IsFaulted = false },
            new() { Started = start.AddDays(-5), Ended = start.AddDays(3), IsFaulted = true },
            new() { Started = end.AddDays(-5), Ended = end.AddDays(3), IsFaulted = false },
            new() { Started = start.AddDays(11), Ended = start.AddDays(13), IsFaulted = false },
            new() { Started = DateTime.Parse("2023-01-05 12:00:00 AM"), Ended = DateTime.Parse("2023-01-06 12:00:00 AM"), IsFaulted = false }
        };
        parentDependent[2].InsightOccurrences = new List<InsightOccurrenceEntity>
        {
            new() { Started = start.AddDays(5), Ended = start.AddDays(10), IsFaulted = false },
            new() { Started = start.AddDays(-5), Ended = start.AddDays(3), IsFaulted = false },
            new() { Started = end.AddDays(-5), Ended = end.AddDays(3), IsFaulted = false },
            new() { Started = start.AddDays(11), Ended = start.AddDays(13), IsFaulted = false },
            new() { Started = DateTime.Parse("2023-01-05 12:00:00 AM"), Ended = DateTime.Parse("2023-01-06 12:00:00 AM"), IsFaulted = false }
        };
        var dependentDependent = Fixture.Build<InsightEntity>()
            .With(x => x.SiteId, siteId)
            .Without(x => x.PointsJson)
            .Without(x => x.ImpactScores)
            .Without(x => x.Dependencies)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs).Create();
        dependentDependent.InsightOccurrences = new List<InsightOccurrenceEntity>
        {
            new() { Started = start.AddDays(5), Ended = start.AddDays(6), IsFaulted = false },
            new() { Started = start.AddDays(-5), Ended = start.AddDays(1), IsFaulted = false },
            new() { Started = end.AddDays(-5), Ended = end.AddDays(3), IsFaulted = true },
            new() { Started = start.AddDays(11), Ended = start.AddDays(13), IsFaulted = false },
            new() { Started = DateTime.Parse("2023-01-05 12:00:00 AM"), Ended = DateTime.Parse("2023-01-06 12:00:00 AM"), IsFaulted = false }
        };
        parentDependent[1].Dependencies = new List<DependencyEntity>()
        {
            Fixture.Build<DependencyEntity>().With(x => x.FromInsightId, parentDependent[1].Id)
                .With(x => x.ToInsightId, dependentDependent.Id).Without(c => c.FromInsight).Without(c=>c.ToInsight).Create()
        };

        var entities= new List<InsightEntity>
        {
            parent,
            parentDependent[0],
            parentDependent[1],
            parentDependent[2],
            dependentDependent
        };
        var interval = string.IsNullOrWhiteSpace(requestedInterval) ||
                       !TimeSpan.TryParse(requestedInterval, CultureInfo.InvariantCulture, out var parsedInterval)
            ? (end > start ? (end - start).TotalMinutes : 1)
            : parsedInterval.TotalMinutes;

        var response = new List<InsightDiagnosticDto>();
        var parentModel= InsightEntity.MapTo(parent);
        foreach (var insightEntity in entities.Where(c => c.Id != insightId))
        {
            var insight = InsightEntity.MapTo(insightEntity);

            if (insight.Id == dependentDependent.Id)
            {
                var parentDependentModel = InsightEntity.MapTo(parentDependent[1]);
                response.Add(InsightDiagnosticDto.MapFrom(insight, parentDependentModel, parentDependentModel.InsightOccurrences.ToList(),
                    start, end, interval));
            }
            else
            {
                response.Add(InsightDiagnosticDto.MapFrom(insight, parentModel, parentModel.InsightOccurrences.ToList(),
                    start, end, interval));
            }
         
        }

        return new (response,entities);
    }
    private static List<TimeSeriesBinaryData> GenerateDiagnosticTimeSeries(
        List<InsightOccurrence> insightOccurrences, DateTime start, DateTime end, TimeSpan? interval)
    {

        var result = new List<TimeSeriesBinaryData>();
        // Filter out irrelevant occurrences
        var filteredOccurrences = insightOccurrences
            .Where(c => c.Started < end && c.Ended > start)
            .OrderBy(c => c.Started)
            .ToList();

        foreach (var occurrence in filteredOccurrences)
        {
            var occurrenceStart = occurrence.Started <= start ? start : occurrence.Started;
            var occurrenceEnd = occurrence.Ended >= end ? end : occurrence.Ended;
            // Calculate interval duration in minutes
            var intervalInMinutes = interval?.TotalMinutes ?? (occurrenceEnd - occurrenceStart).TotalMinutes;
            // Calculate the number of breakdowns within the occurrence
            var numOfBreakdown = interval != null ? (int)Math.Ceiling((occurrenceEnd - occurrenceStart).TotalMinutes / interval.Value.TotalMinutes) : 1;

            for (var i = 0; i < numOfBreakdown; i++)
            {
                result.Add(new TimeSeriesBinaryData
                {
                    Start = occurrenceStart.AddMinutes(i * intervalInMinutes),
                    End = occurrenceStart.AddMinutes((i + 1) * intervalInMinutes),
                    IsFaulty = occurrence.IsFaulted
                });
            }
        }

        return result;
    }
}
