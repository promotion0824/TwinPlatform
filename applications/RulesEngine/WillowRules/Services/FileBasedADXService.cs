using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.Rules.Model;

namespace Willow.Rules.Services;

/// <summary>
/// Reads data from a csv file that was exported from ADX
/// </summary>
/// <remarks>
/// Supports 3 columns on the ADX Telemetry table: SourceTimestamp, TrendId, ScalarValue
/// Example ADX Query:
/// Telemetry | limit 100 | project TrendId, SourceTimestamp, ScalarValue
/// Then Export to csv
/// </remarks>
public class FileBasedADXService : IADXService
{
	private readonly string filePath;
	private readonly ILogger<FileBasedADXService> logger;
	private readonly DateTime? endTime;

	public FileBasedADXService(string filePath, ILogger<FileBasedADXService> logger, DateTime? endTime = null)
	{
		this.filePath = filePath;
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.endTime = endTime;
	}

	/// <summary>
	/// This class represents one row of CSV data. Column names are assumed in the first row
	/// </summary>
	private class ADXLine
	{
		public DateTime SourceTimestamp { get; init; }
		public DateTime EnqueuedTimestamp { get; init; }
		public double ScalarValue { get; init; }
		public string? TrendId { get; init; }
		public string? ConnectorId { get; set; }
		public string? ExternalId { get; set; }
		public string? Properties { get; set; }
	}

	public (Task producer, ChannelReader<RawData> reader) RunRawQueryPaged(DateTime earliest, DateTime latestStop, IEnumerable<IdFilter>? idFilters = null, IEnumerable<string>? ruleIds = null, CancellationToken cancellationToken = default)
	{
		var channel = Channel.CreateBounded<RawData>(100);

		var producer = Task.Run(async () =>
		{
			try
			{
				using (var reader = new StreamReader(filePath))
				{
					var config = new CsvConfiguration(CultureInfo.InvariantCulture)
					{
						HeaderValidated = null,
						MissingFieldFound = null,
						BadDataFound = null
					};

					using (var csv = new CsvReader(reader, config))
					{
						foreach (var item in csv.GetRecords<ADXLine>())
						{
							if (idFilters is not null && idFilters.Count() > 0)
							{
								var remove = true;
								foreach (var filter in idFilters)
								{
									if (!string.IsNullOrEmpty(item.TrendId) && item.TrendId == filter.trendId)
									{
										remove = false;
									}
									else if (!string.IsNullOrEmpty(item.ExternalId) && item.ExternalId == filter.externalId)
									{
										remove = false;
									}
								}

								if (remove)
								{
									continue;
								}
							}

							// CSV reader reads as local time, but we want UTC
							var sourceTimestamp = DateTime.SpecifyKind(item.SourceTimestamp.ToUniversalTime(), DateTimeKind.Utc);
							var enqueuedTimestamp = DateTime.SpecifyKind(item.EnqueuedTimestamp.ToUniversalTime(), DateTimeKind.Utc);
							if (sourceTimestamp < endTime.GetValueOrDefault(DateTime.MaxValue)
							&& sourceTimestamp > earliest && sourceTimestamp < latestStop)
							{
								//logger.LogInformation("Simulated ADX: {pointId} {timestamp} {value}", item.TrendId, item.SourceTimestamp, item.ScalarValue);
								await channel.Writer.WriteAsync(new RawData()
								{
									PointEntityId = item.TrendId,
									SourceTimestamp = sourceTimestamp,
									EnqueuedTimestamp = enqueuedTimestamp,
									Value = item.ScalarValue,
									ConnectorId = item.ConnectorId,
									ExternalId = item.ExternalId,
									TextValue = item.Properties
								}, cancellationToken);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "File read failed");
			}
			finally
			{
				channel.Writer.Complete();
			}

		}, cancellationToken);

		return (producer, channel.Reader);
	}

	public Task<(bool hasTwinChanges, bool hasRelationshipChanges)> HasADTChanges(DateTime startDate)
	{
		throw new NotImplementedException();
	}
}
