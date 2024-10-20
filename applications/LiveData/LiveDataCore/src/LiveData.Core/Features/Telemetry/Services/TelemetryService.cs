namespace Willow.LiveData.Core.Features.Telemetry.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Extensions;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class TelemetryService : ITelemetryService
{
    private readonly ITelemetryRepository telemetryRepository;
    private readonly IDateTimeIntervalService dateTimeIntervalService;
    private readonly IAdxContinuationTokenProvider<string, int> continuationTokenProvider;

    public TelemetryService(ITelemetryRepository telemetryRepository, IDateTimeIntervalService dateTimeIntervalService, IAdxContinuationTokenProvider<string, int> continuationTokenProvider)
    {
        this.telemetryRepository = telemetryRepository;
        this.dateTimeIntervalService = dateTimeIntervalService;
        this.continuationTokenProvider = continuationTokenProvider;
    }

    public async Task<IEnumerable<T>> GetTelemetryDataByTwinIdAsync<T>(
        Guid? clientId,
        DateTime start,
        DateTime end,
        string type,
        string twinId,
        TimeSpan? selectedInterval)
        where T : TelemetrySummaryData
    {
        var interval = dateTimeIntervalService.GetDateTimeInterval(start, end, selectedInterval);

        IReadOnlyCollection<TelemetrySummaryData> items = type switch
        {
            Constants.Analog =>
                await telemetryRepository.GetTelemetryAnalogDataByTwinIdAsync(clientId,
                                                                               start,
                                                                               end,
                                                                               interval.Name,
                                                                               twinId),
            Constants.Binary =>
                await telemetryRepository.GetTelemetryBinaryDataByTwinIdAsync(clientId,
                                                                               start,
                                                                               end,
                                                                               interval.Name,
                                                                               twinId),
            Constants.MultiState =>
                await telemetryRepository.GetTelemetryMultiStateDataByTwinIdAsync(clientId,
                    start,
                    end,
                    interval.Name,
                    twinId),
            Constants.Sum =>
                await telemetryRepository.GetTelemetrySumDataByTwinIdAsync(clientId,
                                                                            start,
                                                                            end,
                                                                            interval.Name,
                                                                            twinId),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Telemetry Data Type {type} is not available."),
        };

        return items as IEnumerable<T>;
    }

    public Task<Dictionary<string, IEnumerable<T>>> GetTelemetryDataByTwinIdAsync<T>(
        Guid? clientId, DateTime start, DateTime end, string type, IEnumerable<string> twinIds, TimeSpan? selectedInterval)
        where T : TelemetrySummaryData
    {
        var interval = dateTimeIntervalService.GetDateTimeInterval(start, end, selectedInterval);

        var getTimeSeriesData = type switch
        {
            Constants.Analog =>
                telemetryRepository.GetTelemetryAnalogDataByTwinIdAsync(clientId,
                                                                         start,
                                                                         end,
                                                                         interval.Name,
                                                                         twinIds.ToList()),
            Constants.Binary =>
                telemetryRepository.GetTelemetryBinaryDataByTwinIdAsync(clientId,
                                                                         start,
                                                                         end,
                                                                         interval.Name,
                                                                         twinIds.ToList()),
            Constants.MultiState =>
                telemetryRepository.GetTelemetryMultiStateDataByTwinIdAsync(clientId,
                    start,
                    end,
                    interval.Name,
                    twinIds.ToList()),

            Constants.Sum =>
                telemetryRepository.GetTelemetrySumDataByTwinIdAsync(clientId,
                                                                      start,
                                                                      end,
                                                                      interval.Name,
                                                                      twinIds.ToList()),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Telemetry Data Type {type} is not available."),
        };

        return GetTelemetryDataByPointEntityIdInternalAsync<T>();

        async Task<Dictionary<string, IEnumerable<TPointType>>> GetTelemetryDataByPointEntityIdInternalAsync<TPointType>()
            where TPointType : TelemetrySummaryData
        {
            var items = await getTimeSeriesData;

            return items?.GroupBy(x => x.Id, x => x.TelemetrySummaryData as TPointType)
                         .ToDictionary(x => x.Key, x => x.AsEnumerable());
        }
    }

    public Task<Dictionary<string, IEnumerable<TelemetrySummaryData>>> GetTelemetryDataByTwinIdAsync(Guid? clientId,
                                                                                                     DateTime start,
                                                                                                     DateTime end,
                                                                                                     string[] twinIds,
                                                                                                     string[] types,
                                                                                                     TimeSpan? selectedInterval)
    {
        var interval = dateTimeIntervalService.GetDateTimeInterval(start, end, selectedInterval);
        var analogPointEntityIds = new List<string>(twinIds.Length);
        var binaryPointEntityIds = new List<string>(twinIds.Length);
        var sumPointEntityIds = new List<string>(twinIds.Length);
        var multiStateEntityIds = new List<string>(twinIds.Length);
        for (var i = 0; i < twinIds.Length; i++)
        {
            if (types?[i] == null)
            {
                throw new BadRequestException("Point type cannot be empty");
            }

            var type = types[i].ToLower();
            switch (type)
            {
                case Constants.Analog:
                    analogPointEntityIds.Add(twinIds[i]);
                    break;
                case Constants.Binary:
                    binaryPointEntityIds.Add(twinIds[i]);
                    break;
                case Constants.MultiState:
                    multiStateEntityIds.Add(twinIds[i]);
                    break;
                case Constants.Sum:
                    sumPointEntityIds.Add(twinIds[i]);
                    break;
                default:
                    throw new BadRequestException($"Point Type {type} is not supported.");
            }
        }

        return GetTelemetryDataByTwinIdInternalAsync();

        async Task<Dictionary<string, IEnumerable<TelemetrySummaryData>>> GetTelemetryDataByTwinIdInternalAsync()
        {
            var items = new List<(string Id, TelemetrySummaryData Data)>();
            var result = new Dictionary<string, IEnumerable<TelemetrySummaryData>>();
            if (analogPointEntityIds.Any())
            {
                items.AddRange(await telemetryRepository.GetTelemetryAnalogDataByTwinIdAsync(clientId,
                                                                                              start,
                                                                                              end,
                                                                                              interval.Name,
                                                                                              analogPointEntityIds));
            }

            if (binaryPointEntityIds.Any())
            {
                items.AddRange(await telemetryRepository.GetTelemetryBinaryDataByTwinIdAsync(clientId,
                                                                                              start,
                                                                                              end,
                                                                                              interval.Name,
                                                                                              binaryPointEntityIds));
            }

            if (sumPointEntityIds.Any())
            {
                items.AddRange(await telemetryRepository.GetTelemetrySumDataByTwinIdAsync(clientId,
                                                                                           start,
                                                                                           end,
                                                                                           interval.Name,
                                                                                           sumPointEntityIds));
            }

            if (multiStateEntityIds.Any())
            {
                items.AddRange(await telemetryRepository.GetTelemetryMultiStateDataByTwinIdAsync(clientId,
                    start,
                    end,
                    interval.Name,
                    multiStateEntityIds));
            }

            foreach (var groupedItem in items.GroupBy(x => x.Id, x => x.Data))
            {
                result[groupedItem.Key] = groupedItem.ToList();
            }

            return result;
        }
    }

    public async Task<TelemetryRawResult> GetTelemetryRawDataByTwinIdAsync(Guid? clientId,
                                                                           string twinId,
                                                                           DateTime start,
                                                                           DateTime end,
                                                                           string continuationToken,
                                                                           int pageSize)
    {
        var (storedQueryNameResult, rowNumber) = continuationTokenProvider.ParseToken(continuationToken);

        var pagedTelemetry = await telemetryRepository.GetTelemetryAsync(clientId, start, end, pageSize, storedQueryNameResult, new[] { twinId }, rowNumber);

        if (pagedTelemetry is null)
        {
            return null;
        }

        var lastRowNumber = pagedTelemetry.Telemetry.MaxBy(x => x.RowNumber)?.RowNumber;
        continuationToken = continuationTokenProvider.GetToken(pagedTelemetry.ContinuationToken, lastRowNumber ?? 0);

        return new TelemetryRawResult
        {
            ContinuationToken = pagedTelemetry.TotalRowsCount > lastRowNumber ? continuationToken : string.Empty,
            Data = pagedTelemetry.Telemetry?.ToList().MapToTelemetryRaw(),
        };
    }

    [Obsolete]
    public async Task<IEnumerable<TelemetryRawResultMultiple>> GetTelemetryRawDataBySiteIdAsync(Guid? clientId,
                                                                                                Guid siteId,
                                                                                                DateTime start,
                                                                                                DateTime end,
                                                                                                IEnumerable<string> twinIds)
    {
        var items = await telemetryRepository.GetTelemetryRawDataBySiteIdAsync(
                                                                                   clientId,
                                                                                   siteId,
                                                                                   start,
                                                                                   end,
                                                                                   twinIds.ToList());

        var grouped = items?.GroupBy(q => q.Id);

        return grouped?.Select(group => new TelemetryRawResultMultiple
        {
            TwinId = group.Key,
            Data = group.Select(q => new TelemetryRawData
            {
                Value = q.Value,
                Timestamp = q.Timestamp,
            })
                                       .ToList(),
        })
                      .ToList();
    }

    [Obsolete]
    public async Task<IEnumerable<TelemetryRawData>> GetLastTelemetryRawDataBySiteIdAsync(Guid? clientId, Guid siteId, IEnumerable<string> twinIds = null)
    {
        return await telemetryRepository.GetLastTelemetryRawDataBySiteIdAsync(clientId, siteId, twinIds);
    }

    public async Task<IEnumerable<TelemetryRawResultMultiple>> GetTelemetryRawDataAsync(Guid? clientId, DateTime start, DateTime end, IEnumerable<string> twinIds)
    {
        var items = await telemetryRepository.GetTelemetryRawDataAsync(
            clientId,
            start,
            end,
            twinIds);

        var grouped = items?.GroupBy(q => q.Id);

        return grouped?.Select(group => new TelemetryRawResultMultiple
        {
            TwinId = group.Key,
            Data = group.Select(q => new TelemetryRawData
            {
                Value = q.Value,
                Timestamp = q.Timestamp,
            })
                    .ToList(),
        })
            .ToList();
    }

    public async Task<IEnumerable<TelemetryRawData>> GetLastTelemetryRawDataAsync(Guid? clientId, IEnumerable<string> twinIds = null)
    {
        return await telemetryRepository.GetLastTelemetryRawDataAsync(clientId, twinIds);
    }

    public async Task<GetTelemetryResult> GetTelemetryAsync(Guid? clientId,
                                                            IEnumerable<string> twinIds,
                                                            DateTime start,
                                                            DateTime end,
                                                            int pageSize,
                                                            string continuationToken)
    {
        var (storedQueryNameResult, rowNumber) = continuationTokenProvider.ParseToken(continuationToken);

        var pagedTelemetry = await telemetryRepository.GetTelemetryAsync(clientId, start, end, pageSize, storedQueryNameResult, twinIds, rowNumber);

        if (pagedTelemetry is null)
        {
            return null;
        }

        var lastRowNumber = pagedTelemetry.Telemetry.MaxBy(x => x.RowNumber)?.RowNumber;
        continuationToken = continuationTokenProvider.GetToken(pagedTelemetry.ContinuationToken, lastRowNumber ?? 0);

        return new GetTelemetryResult
        {
            ContinuationToken = pagedTelemetry.TotalRowsCount > lastRowNumber ? continuationToken : string.Empty,
            Data = pagedTelemetry.Telemetry?.ToList().MapTo(),
        };
    }

    public async Task<IEnumerable<TelemetryData>> GetLastTelemetryAsync(IEnumerable<string> twinIds)
    {
        var telemetry = await telemetryRepository.GetLastTelemetryAsync(twinIds);

        return telemetry.MapTo();
    }
}
