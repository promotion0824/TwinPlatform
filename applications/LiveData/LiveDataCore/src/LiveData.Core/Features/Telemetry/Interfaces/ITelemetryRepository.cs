namespace Willow.LiveData.Core.Features.Telemetry.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model;
using Willow.LiveData.Core.Features.Telemetry.DTOs;

internal interface ITelemetryRepository
{
    Task<IReadOnlyCollection<TelemetryAnalogResponseData>> GetTelemetryAnalogDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId);

    Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>>
        GetTelemetryAnalogDataByTwinIdAsync(Guid? clientId,
            DateTime start,
            DateTime end,
            string interval,
            IReadOnlyCollection<string> twinIds);

    Task<IReadOnlyCollection<TelemetryMultiStateResponseData>> GetTelemetryMultiStateDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId);

    Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>>
        GetTelemetryMultiStateDataByTwinIdAsync(Guid? clientId,
            DateTime start,
            DateTime end,
            string interval,
            IReadOnlyCollection<string> twinIds);

    Task<IReadOnlyCollection<TelemetryBinaryResponseData>> GetTelemetryBinaryDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId);

    Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>>
        GetTelemetryBinaryDataByTwinIdAsync(Guid? clientId,
            DateTime start,
            DateTime end,
            string interval,
            IReadOnlyCollection<string> twinIds);

    Task<IReadOnlyCollection<TelemetrySumResponseData>> GetTelemetrySumDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        string twinId);

    Task<IReadOnlyCollection<(string Id, TelemetrySummaryData TelemetrySummaryData)>> GetTelemetrySumDataByTwinIdAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string interval,
        IReadOnlyCollection<string> twinIds);

    Task<IReadOnlyCollection<TelemetryRawData>> GetTelemetryRawDataBySiteIdAsync(Guid? clientId,
        Guid siteId,
        DateTime start,
        DateTime end,
        IEnumerable<string> pointIds);

    Task<IReadOnlyCollection<TelemetryRawData>> GetLastTelemetryRawDataBySiteIdAsync(
        Guid? clientId,
        Guid siteId,
        IEnumerable<string> twinIds = null);

    Task<IReadOnlyCollection<TelemetryRawData>> GetTelemetryRawDataAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        IEnumerable<string> pointIds);

    Task<IReadOnlyCollection<TelemetryRawData>> GetLastTelemetryRawDataAsync(
        Guid? clientId,
        IEnumerable<string> twinIds = null);

    Task<PagedTelemetry> GetTelemetryAsync(
        Guid? clientId,
        DateTime start,
        DateTime end,
        int pageSize,
        string continuationToken,
        IEnumerable<string> twinIds = null,
        int lastRowNumber = 0);

    Task<IReadOnlyCollection<Telemetry>> GetLastTelemetryAsync(IEnumerable<string> twinIds);
}
