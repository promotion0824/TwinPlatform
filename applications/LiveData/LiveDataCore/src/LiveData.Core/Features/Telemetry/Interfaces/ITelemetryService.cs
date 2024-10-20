namespace Willow.LiveData.Core.Features.Telemetry.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
using Willow.LiveData.Core.Features.Telemetry.DTOs;

internal interface ITelemetryService
{
    Task<IEnumerable<T>> GetTelemetryDataByTwinIdAsync<T>(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string type,
        string twinId,
        TimeSpan? selectedInterval)
        where T : TelemetrySummaryData;

    Task<Dictionary<string, IEnumerable<T>>> GetTelemetryDataByTwinIdAsync<T>(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string type,
        IEnumerable<string> twinIds,
        TimeSpan? selectedInterval)
        where T : TelemetrySummaryData;

    Task<Dictionary<string, IEnumerable<TelemetrySummaryData>>> GetTelemetryDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string[] twinIds,
        string[] types,
        TimeSpan? selectedInterval);

    Task<TelemetryRawResult> GetTelemetryRawDataByTwinIdAsync(
        Guid? clientId,
        string twinId,
        DateTime start,
        DateTime end,
        string continuationToken,
        int pageSize);

    [Obsolete]
    Task<IEnumerable<TelemetryRawResultMultiple>> GetTelemetryRawDataBySiteIdAsync(
        Guid? clientId,
        Guid siteId,
        DateTime start,
        DateTime end,
        IEnumerable<string> twinIds);

    [Obsolete]
    Task<IEnumerable<TelemetryRawData>> GetLastTelemetryRawDataBySiteIdAsync(
        Guid? clientId,
        Guid siteId,
        IEnumerable<string> twinIds = null);

    Task<IEnumerable<TelemetryRawResultMultiple>> GetTelemetryRawDataAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        IEnumerable<string> twinIds);

    Task<IEnumerable<TelemetryRawData>> GetLastTelemetryRawDataAsync(
        Guid? clientId,
        IEnumerable<string> twinIds = null);

    Task<GetTelemetryResult> GetTelemetryAsync(
        Guid? clientId,
        IEnumerable<string> twinIds,
        DateTime start,
        DateTime end,
        int pageSize,
        string continuationToken);

    Task<IEnumerable<TelemetryData>> GetLastTelemetryAsync(IEnumerable<string> twinIds);
}
