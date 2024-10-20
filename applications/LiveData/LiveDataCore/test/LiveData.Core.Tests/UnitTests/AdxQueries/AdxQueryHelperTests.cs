namespace Willow.LiveData.Core.Tests.UnitTests.AdxQueries;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Helpers;

internal class AdxQueryHelperTests
{
    private static readonly string RemoveDuplicateQuery = "| summarize " + Environment.NewLine +
"arg_max(EnqueuedTimestamp, *) by ExternalId, TrendId, SourceTimestamp";

    private static readonly string LatestQuery = "| summarize " + Environment.NewLine +
"arg_max(SourceTimestamp, *) by ExternalId, TrendId";

    private static readonly string GetTwinsQuery = "| where " + Environment.NewLine +
"Id in ";

    private const string GetTelemetryDateTimeQuery = @"SourceTimestamp between ";
    private const string GetTelemetryExternalIdQuery = @"ExternalId in ";
    private const string GetTelemetryTrendIdQuery = @"TrendId in ";
    private static readonly string AnalogSummaryQuery = "| summarize " + Environment.NewLine +
"Average = avg(todouble(ScalarValue)), Maximum = max(todouble(ScalarValue)), Minimum = min(todouble(ScalarValue))";

    private static readonly string BinarySummaryQuery = "| summarize " + Environment.NewLine +
"OnCount = countif(tobool(ScalarValue) == true), OffCount = countif(tobool(ScalarValue) == false)";

    private static readonly string SumSummaryQuery = "| summarize " + Environment.NewLine +
"Sum = sum(todouble(ScalarValue))";

    private readonly IAdxQuerySelector query = AdxQueryBuilder.Create()
                                                               .Select(AdxConstants.TelemetryTable) as IAdxQuerySelector;

    private readonly AdxQueryHelper adxQueryHelper = new();

    [TestCase]
    public void RemoveDuplicates_Returns_Latest_By_EnqueuedTimestamp()
    {
        var duplicateFilter = adxQueryHelper.RemoveDuplicates(query).GetQuery();

        duplicateFilter.Should().NotBeNull();
        duplicateFilter.Should().Contain(RemoveDuplicateQuery);
    }

    [TestCase]
    public void GetLatestValue_Returns_Latest_By_SourceTimestamp()
    {
        var latestFilter = adxQueryHelper.GetLatestValue(query).GetQuery();

        latestFilter.Should().NotBeNull();
        latestFilter.Should().Contain(LatestQuery);
    }

    [Test]
    [TestCaseSource(nameof(TelemetryTestCases))]
    public void GetTelemetryValues_Returns_Correct_Filters(DateTime start, DateTime end, List<TwinDetails> twinDetailsList)
    {
        var telemetryFilter = adxQueryHelper.GetTelemetryValues(start, end, twinDetailsList).GetQuery();
        var externalIds = twinDetailsList
                         .Where(x => !string.IsNullOrEmpty(x.ExternalId))
                         .Select(x => x.ExternalId)
                         .Distinct();
        var trendIds = twinDetailsList
                      .Where(x => !string.IsNullOrEmpty(x.TrendId.ToString()))
                      .Select(x => x.TrendId)
                      .Distinct();

        var expectedDateTimeFilter = GetTelemetryDateTimeQuery + $"(datetime({start:s}) .. datetime({end:s}))";
        var expectedExternalIdFilter = GetTelemetryExternalIdQuery + $"({string.Join(',', externalIds.Select(v => $"\"{v}\""))})";
        var expectedTrendIdFilter = GetTelemetryTrendIdQuery + $"({string.Join(',', trendIds.Select(v => $"\"{v}\""))})";

        telemetryFilter.Should().NotBeNull();
        telemetryFilter.Should().Contain(expectedDateTimeFilter);
        telemetryFilter.Should().Contain(expectedExternalIdFilter);
        telemetryFilter.Should().Contain(expectedTrendIdFilter);
    }

    [Test]
    [TestCaseSource(nameof(TwinsTestCases))]
    public void GetIdsForTwins_Returns_Given_Ids(List<string> twinIds, Guid? siteId)
    {
        var twinsFilter = adxQueryHelper.GetIdsForTwins(twinIds, siteId).GetQuery();

        var expectedTwinIdString = GetTwinsQuery + $"({string.Join(',', twinIds.Select(v => $"\"{v}\""))})";
        var expectedSiteIdString = $"SiteId == \"{siteId}\"";
        twinsFilter.Should().NotBeNull();
        twinsFilter.Should().Contain(expectedTwinIdString);
        if (siteId is null)
        {
            twinsFilter.Should().NotContain(expectedSiteIdString);
        }
        else
        {
            twinsFilter.Should().Contain(expectedSiteIdString);
        }
    }

    [TestCase("5 minutes")]
    public void SummarizeAnalogValues_Returns_Analog_Summary(string interval)
    {
        var analogFilter = adxQueryHelper.SummarizeAnalogValues(query, interval).GetQuery();

        analogFilter.Should().NotBeNull();
        analogFilter.Should().Contain(AnalogSummaryQuery);
    }

    [TestCase("5 minutes")]
    public void SummarizeBinaryValues_Returns_Binary_Summary(string interval)
    {
        var binaryFilter = adxQueryHelper.SummarizeBinaryValues(query, interval).GetQuery();

        binaryFilter.Should().NotBeNull();
        binaryFilter.Should().Contain(BinarySummaryQuery);
    }

    [TestCase("5 minutes")]
    public void SummarizeSumValues_Returns_Sum_Values(string interval)
    {
        var sumFilter = adxQueryHelper.SummarizeSumValues(query, interval).GetQuery();

        sumFilter.Should().NotBeNull();
        sumFilter.Should().Contain(SumSummaryQuery);
    }

    private static IEnumerable<TestCaseData> TwinsTestCases()
    {
        yield return new TestCaseData(new List<string> { "Test-Twin1", "Test-Twin2" }, null);
        yield return new TestCaseData(new List<string> { "Test-Twin3", "Test-Twin4" }, Guid.NewGuid());
    }

    private static IEnumerable<TestCaseData> TelemetryTestCases()
    {
        yield return new TestCaseData(DateTime.UtcNow - TimeSpan.FromHours(1), DateTime.UtcNow, GenerateTwinDetails());
    }

    private static List<TwinDetails> GenerateTwinDetails()
    {
        var result = new List<TwinDetails>
        {
            new("Test-Twin1", "Test-ExternalId", Guid.Empty),
            new("Test-Twin2", null, Guid.NewGuid()),
        };

        return result;
    }
}
