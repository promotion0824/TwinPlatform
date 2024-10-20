using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.DataQuality.Model.Capability;
using Willow.Rules.Logging;
using Willow.Rules.Model;

namespace Willow.Rules.Services;

/// <summary>
/// Service to send data to the Willow Data Quality Api
/// </summary>
public interface IDataQualityService
{
	/// <summary>
	/// Queues all the time series using a channel buffered writer
	/// </summary>
	Task SendCapabilityStatusUpdate(IEnumerable<TimeSeries> buffers, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the reader channel (used by the background service)
	/// </summary>
	ChannelReader<DataQualityServiceDto> Reader { get; }

	/// <summary>
	/// Performs the actual send operation
	/// </summary>
	Task SendCapabilityStatusUpdate(IEnumerable<DataQualityServiceDto> status, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service to send data to the Willow Data Quality Api
/// </summary>
public class DataQualityService : IDataQualityService
{
	public const int MaxQueueCapacity = 100000;
	private readonly Channel<DataQualityServiceDto> messageQueue;
	private readonly ILogger<DataQualityService> logger;
	private readonly IADTApiService adtApiService;

	/// <summary>
	/// Creates a new <see cref="DataQualityService" />
	/// </summary>
	public DataQualityService(IADTApiService adtApiService, ILogger<DataQualityService> logger)
	{
		this.adtApiService = adtApiService ?? throw new ArgumentNullException(nameof(adtApiService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

		this.messageQueue = Channel.CreateBounded<DataQualityServiceDto>(new BoundedChannelOptions(MaxQueueCapacity)
		{
			//Increased performance
			SingleReader = true
		});
	}

	public ChannelReader<DataQualityServiceDto> Reader => messageQueue.Reader;
	public ChannelWriter<DataQualityServiceDto> Writer => messageQueue.Writer;

	public async Task SendCapabilityStatusUpdate(IEnumerable<TimeSeries> buffers, CancellationToken cancellationToken = default)
	{
		var timedLogger = logger.Throttle(TimeSpan.FromSeconds(5));

		foreach (var timeseries in buffers)
		{
			Guid.TryParse(timeseries.Id, out Guid trendId);

			if (trendId != Guid.Empty || (!string.IsNullOrEmpty(timeseries.ExternalId)))
			{
				await Writer.WriteAsync(new DataQualityServiceDto()
				{
					TwinId = timeseries.DtId,
					TrendId = trendId != Guid.Empty ? trendId : null,
					ConnectorId = timeseries.ConnectorId,
					ExternalId = timeseries.ExternalId,
					IsOffline = timeseries.IsOffline,
					IsPeriodOutOfRange = timeseries.IsPeriodOutOfRange,
					IsStuck = timeseries.IsStuck,
					IsValueOutOfRange = timeseries.IsValueOutOfRange,
					ReportedDateTime = timeseries.LastSeen
				}, cancellationToken);
			}
			else
			{
				timedLogger.LogWarning("Could not convert timeseries id to guid for Id {timeseriesId}", timeseries.Id);
			}
		}
	}

	public async Task SendCapabilityStatusUpdate(IEnumerable<DataQualityServiceDto> status, CancellationToken cancellationToken = default)
	{
		try
		{
			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(20));

			var request = status.Select(v =>
			{
				var status = new CapabilityStatusDto()
				{
					ConnectorId = v.ConnectorId,
					ExternalId = v.ExternalId,
					ReportedDateTime = v.ReportedDateTime.DateTime,
					TrendId = v.TrendId,
					TwinId = v.TwinId,
					Status = new List<StatusType>()
				};

				if (v.IsStuck)
				{
					status.Status.Add(StatusType.IsStuck);
				}

				if (v.IsOffline)
				{
					status.Status.Add(StatusType.IsOffline);
				}

				if (v.IsPeriodOutOfRange)
				{
					status.Status.Add(StatusType.IsPeriodOutOfRange);
				}

				if (v.IsValueOutOfRange)
				{
					status.Status.Add(StatusType.IsValueOutOfRange);
				}

				//no error? add all is OK
				if (status.Status.Count == 0)
				{
					status.Status.Add(StatusType.Ok);
				}

				return status;
			}).ToArray();

			if (adtApiService.IsConfiguredCorrectly)
			{
				//disabled for now. Response Status 200 seems to be an error??
				//await adtApiService.CreateStatusAsync(request, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to SendCapabilityStatusUpdate");
			throw;
		}
	}
}

/// <summary>
/// Dto for sending data to Data Quality Api
/// </summary>
public class DataQualityServiceDto
{
	/// <summary>
	/// The twin Id
	/// </summary>
	public string? TwinId { get; init; }

	/// <summary>
	/// The twin's trendid
	/// </summary>
	public Guid? TrendId { get; init; }

	/// <summary>
	/// The twin's connector id
	/// </summary>
	public string? ConnectorId { get; init; }

	/// <summary>
	/// The twin's external id
	/// </summary>
	public string? ExternalId { get; init; }

	/// <summary>
	/// The value is above the maximum or below the minimum for this type
	/// </summary>
	public bool IsValueOutOfRange { get; init; }

	/// <summary>
	/// The period is too far from the expected interval
	/// </summary>
	public bool IsPeriodOutOfRange { get; init; }

	/// <summary>
	/// The sensor has been stuck on the same value for a long time
	/// </summary>
	public bool IsStuck { get; init; }

	/// <summary>
	/// The sensor appears to be offline, no data has been received for > 2 x expected period
	/// </summary>
	public bool IsOffline { get; init; }

	/// <summary>
	/// The date and time of the last point processed
	/// </summary>
	public DateTimeOffset ReportedDateTime { get; set; }
}
