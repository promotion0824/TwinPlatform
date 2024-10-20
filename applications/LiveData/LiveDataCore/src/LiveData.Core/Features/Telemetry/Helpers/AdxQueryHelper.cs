namespace Willow.LiveData.Core.Features.Telemetry.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class AdxQueryHelper : IAdxQueryHelper
{
    private static readonly IEnumerable<string> SkipIllegalValues = new[] { "NaN", "N/A", "n/a", "NA", "na", "NAN", "nan", "Infinity" };

    /// <inheritdoc/>
    public IAdxQuerySelector GetIdsForTwins(IEnumerable<string> twinIds, Guid? siteId)
    {
        if (siteId is null)
        {
            return (AdxQueryBuilder.Create()
                                   .Select(AdxConstants.ActiveTwinsFunction)
                                   .Where()
                                   .PropertyIn(AdxConstants.TwinsId, twinIds) as IAdxQuerySelector)!
               .Project(AdxConstants.TwinsId, TelemetryTable.ExternalId, TelemetryTable.TrendId);
        }

        if (twinIds is null)
        {
            return (AdxQueryBuilder.Create()
                                   .Select(AdxConstants.ActiveTwinsFunction)
                                   .Where()
                                   .PropertyEquals(AdxConstants.SiteId, siteId.ToString())
                        as IAdxQuerySelector)!
               .Project(AdxConstants.TwinsId, TelemetryTable.ExternalId, TelemetryTable.TrendId);
        }

        return (AdxQueryBuilder.Create()
                               .Select(AdxConstants.ActiveTwinsFunction)
                               .Where()
                               .PropertyIn(AdxConstants.TwinsId, twinIds)
                               .And()
                               .PropertyEquals(AdxConstants.SiteId, siteId.ToString())
                    as IAdxQuerySelector)!
           .Project(AdxConstants.TwinsId, TelemetryTable.ExternalId, TelemetryTable.TrendId);
    }

    /// <inheritdoc/>
    public IAdxQuerySelector GetTelemetryValues(DateTime start, DateTime end, IEnumerable<TwinDetails> twinDetails)
    {
        var twinDetailsList = twinDetails.ToList();
        var externalIds = twinDetailsList
                         .Where(x => !string.IsNullOrEmpty(x.ExternalId))
                         .Select(x => x.ExternalId)
                         .Distinct()
                         .ToList();
        var trendIds = twinDetailsList
                      .Where(x => !string.IsNullOrEmpty(x.TrendId.ToString()))
                      .Select(x => x.TrendId.ToString())
                      .Distinct()
                      .ToList();

        return AdxQueryBuilder.Create()
                              .Select(AdxConstants.TelemetryTable)
                              .Where()
                              .DateTimeBetween(TelemetryTable.SourceTimestamp, start, end)
                              .And()
                              .PropertyNotIn(TelemetryTable.TelemetryField, SkipIllegalValues)
                              .And()
                              .OpenGroupParentheses()
                              .PropertyIn(TelemetryTable.ExternalId, externalIds)
                              .Or()
                              .PropertyIn(TelemetryTable.TrendId, trendIds)
                              .CloseGroupParentheses()
                   as IAdxQuerySelector;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector RemoveDuplicates(IAdxQuerySelector querySelector)
    {
        return querySelector
              .Summarize()
              .ArgMax(TelemetryTable.EnqueuedTimestamp, "*")
              .By()
              .AddFields(TelemetryTable.ExternalId, TelemetryTable.TrendId, TelemetryTable.SourceTimestamp)
                   as IAdxQuerySelector;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector GetLatestValue(IAdxQuerySelector querySelector)
    {
        return querySelector
              .Summarize()
              .ArgMax(TelemetryTable.SourceTimestamp, "*")
              .By()
              .AddFields(TelemetryTable.ExternalId, TelemetryTable.TrendId)
                   as IAdxQuerySelector;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector SummarizeAnalogValues(IAdxQuerySelector querySelector, string interval)
    {
        return querySelector
              .Summarize()
              .Average(TelemetryTable.TelemetryField).Comma()
              .Maximum(TelemetryTable.TelemetryField).Comma()
              .Minimum(TelemetryTable.TelemetryField)
              .By()
              .Bin($"Timestamp={TelemetryTable.SourceTimestamp}", $"time({interval})").Comma()
              .AddFields(TelemetryTable.TrendId, TelemetryTable.ExternalId)
              .Order("Timestamp") as IAdxQuerySelector;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector SummarizeMultiStateValues(IAdxQuerySelector querySelector, string interval)
    {
        var summary = querySelector
            .Summarize()
            .StateCount(TelemetryTable.TelemetryField)
            .Comma()
            .Bin($"Timestamp={TelemetryTable.SourceTimestamp}", $"time({interval})").Comma()
            .AddFields(TelemetryTable.TrendId, TelemetryTable.ExternalId) as IAdxQuerySelector;
        var result = summary!.Summarize()
            .State(TelemetryTable.TelemetryField, "StateCountValue")
            .By()
            .AddFields("Timestamp", TelemetryTable.TrendId, TelemetryTable.ExternalId)
            .Order("Timestamp") as IAdxQuerySelector;

        return result;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector SummarizeBinaryValues(IAdxQuerySelector querySelector, string interval)
    {
        return querySelector
              .Summarize()
              .OnCount(TelemetryTable.TelemetryField).Comma()
              .OffCount(TelemetryTable.TelemetryField)
              .By()
              .Bin($"Timestamp={TelemetryTable.SourceTimestamp}", $"time({interval})").Comma()
              .AddFields(TelemetryTable.TrendId, TelemetryTable.ExternalId)
              .Order("Timestamp") as IAdxQuerySelector;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector SummarizeSumValues(IAdxQuerySelector querySelector, string interval)
    {
        return querySelector
              .Summarize()
              .Sum(TelemetryTable.TelemetryField)
              .By()
              .Bin($"Timestamp={TelemetryTable.SourceTimestamp}", $"time({interval})").Comma()
              .AddFields(TelemetryTable.TrendId, TelemetryTable.ExternalId)
              .Order("Timestamp") as IAdxQuerySelector;
    }
}
