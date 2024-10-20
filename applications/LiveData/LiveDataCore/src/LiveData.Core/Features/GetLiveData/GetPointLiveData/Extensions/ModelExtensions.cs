namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model;
using Willow.LiveData.Core.Features.Telemetry.DTOs;

/// <summary>
/// Extensions methods for Models.
/// </summary>
internal static class ModelExtensions
{
    /// <summary>
    /// Map Model to DTO.
    /// </summary>
    /// <param name="telemetry">Telemetry.</param>
    /// <returns>TelemetryData.</returns>
    private static TelemetryData MapTo(this Telemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        return new TelemetryData()
        {
            Altitude = telemetry.Altitude,
            Latitude = telemetry.Latitude,
            Longitude = telemetry.Longitude,
            ConnectorId = telemetry.ConnectorId,
            TwinId = telemetry.DtId,
            EnqueuedTimestamp = telemetry.EnqueuedTimestamp,
            ExternalId = telemetry.ExternalId,
            Properties = telemetry.Properties,
            ScalarValue = telemetry.ScalarValue,
            SourceTimestamp = telemetry.SourceTimestamp,
            TrendId = telemetry.TrendId,
        };
    }

    /// <summary>
    /// Map Model List to Dto List.
    /// </summary>
    /// <param name="telemetry">A list of Telemetry.</param>
    /// <returns>A list of TelemetryData.</returns>
    public static List<TelemetryData> MapTo(this IEnumerable<Telemetry> telemetry)
    {
        return telemetry.Select(x => x.MapTo()).ToList();
    }

    /// <summary>
    /// Map ADX Telemetry to TelemetryRawData.
    /// </summary>
    /// <param name="telemetry">An enumerable of Telemetry.</param>
    /// <returns>A list of TelemetryRawData.</returns>
    public static List<TelemetryRawData> MapToTelemetryRaw(this IEnumerable<Telemetry> telemetry)
    {
        return telemetry.Select(x => x.MapToTelemetryRaw()).ToList();
    }

    /// <summary>
    /// Map TwinId from ExternalId or TrendId.
    /// </summary>
    /// <typeparam name="T">Derived from Telemetry Base ID Data.</typeparam>
    /// <param name="telemetry">An enumerable of Telemetry Base ID Data.</param>
    /// <param name="twinsList">A list of Twin Details.</param>
    /// <returns>List of telemetry with TwinId populated.</returns>
    public static IEnumerable<T> MapTwinId<T>(
        this IEnumerable<TelemetryBaseIdData> telemetry,
        List<TwinDetails> twinsList)
        where T : TelemetryBaseIdData
    {
        var result = new List<T>();
        foreach (var telemetryBaseData in telemetry)
        {
            var item = (T)telemetryBaseData;
            item.Id = string.IsNullOrEmpty(item.TrendId)
                          ? twinsList.Where(x => x.ExternalId == item.ExternalId)
                                     .Select(x => x.Id)
                                     .SingleOrDefault()
                          : twinsList.Where(x => x.TrendId.ToString() == item.TrendId)
                                     .Select(x => x.Id)
                                     .SingleOrDefault();
            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Map Model to DTO.
    /// </summary>
    /// <param name="telemetry">Telemetry.</param>
    /// <returns>TelemetryRawData.</returns>
    private static TelemetryRawData MapToTelemetryRaw(this Telemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        return new TelemetryRawData
        {
            Value = telemetry.ScalarValue,
            Timestamp = telemetry.SourceTimestamp,
        };
    }
}
