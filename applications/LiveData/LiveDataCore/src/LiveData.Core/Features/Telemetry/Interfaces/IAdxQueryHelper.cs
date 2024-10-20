namespace Willow.LiveData.Core.Features.Telemetry.Interfaces;

using System;
using System.Collections.Generic;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Helpers;

internal interface IAdxQueryHelper
{
    IAdxQuerySelector GetIdsForTwins(IEnumerable<string> twinIds, Guid? siteId);

    IAdxQuerySelector GetTelemetryValues(DateTime start, DateTime end, IEnumerable<TwinDetails> twinDetails);

    IAdxQuerySelector RemoveDuplicates(IAdxQuerySelector querySelector);

    IAdxQuerySelector GetLatestValue(IAdxQuerySelector querySelector);

    IAdxQuerySelector SummarizeAnalogValues(IAdxQuerySelector querySelector, string interval);

    IAdxQuerySelector SummarizeMultiStateValues(IAdxQuerySelector querySelector, string interval);

    IAdxQuerySelector SummarizeBinaryValues(IAdxQuerySelector querySelector, string interval);

    IAdxQuerySelector SummarizeSumValues(IAdxQuerySelector querySelector, string interval);
}
