using Azure.Core;
using Azure.Identity;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Data.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Logging;
using Willow.Rules.Model;

namespace Willow.Rules.Services;

/// <summary>
/// An interface for reading ADX trend data
/// </summary>
public interface IADXService
{
	/// <summary>
	/// Queries ADX Telemetry for the given time range
	/// </summary>
	(Task producer, ChannelReader<RawData> reader) RunRawQueryPaged(DateTime earliest, DateTime latestStop, IEnumerable<IdFilter>? idFilters = null, IEnumerable<string>? ruleIds = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get twin ids changed after a given date
	/// </summary>
	Task<(bool hasTwinChanges, bool hasRelationshipChanges)> HasADTChanges(DateTime startDate);
}

/// <summary>
/// A filter object for ADX
/// </summary>
public record class IdFilter(string trendId, string? externalId = null, string? connectorId = null)
{
	internal string Format()
	{
		var filters = new List<string>();

		if (!string.IsNullOrEmpty(trendId))
		{
			filters.Add($"(TrendId == '{trendId}')");
		}

		if (!string.IsNullOrEmpty(connectorId) && !string.IsNullOrEmpty(externalId))
		{
			filters.Add($"(ConnectorId == '{connectorId}' and ExternalId == '{externalId}')");
		}
		else if (!string.IsNullOrEmpty(externalId))  // Mapped missing connector Id
		{
			filters.Add($"(ExternalId == '{externalId}')");
		}

		return string.Join(" or ", filters);
	}

	/// <summary>
	/// Checks whether the filter has a trendid or externalid
	/// </summary>
	public bool IsValid()
	{
		if(string.IsNullOrEmpty(trendId) && string.IsNullOrEmpty(externalId))
		{
			return false;
		}

		return true;
	}
};

/// <summary>
/// A service for querying ADX
/// </summary>
public class ADXService : IADXService
{
	private readonly Func<ICslQueryProvider> clientFactory;
	private readonly string databaseName;
	private readonly ILogger logger;
	private readonly ILogger throttledLogger;
	private readonly HealthCheckADX healthCheckADX;

	/// <summary>
	/// Creates a new <see cref="ADXService"/>
	/// </summary>
	public ADXService(IOptions<CustomerOptions> customerOptions,
		DefaultAzureCredential credential,
		HealthCheckADX healthCheckADX,
		ILogger logger)
	{
		if (customerOptions is null) throw new ArgumentNullException(nameof(customerOptions));
		this.healthCheckADX = healthCheckADX ?? throw new ArgumentNullException(nameof(healthCheckADX));
		this.databaseName = customerOptions.Value.ADX?.DatabaseName ?? throw new ArgumentNullException(nameof(AdxOption.DatabaseName));
		string connectionString = customerOptions.Value.ADX?.Uri ?? throw new ArgumentNullException(nameof(AdxOption.Uri));
		this.clientFactory = () => GetADXConnection(connectionString, logger, credential);
		this.logger = logger;
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(30));
	}

	private static DateTimeOffset lastTokenExpires = DateTimeOffset.Now;

	private static ICslQueryProvider GetADXConnection(string url, ILogger logger, DefaultAzureCredential credential)
	{
		logger.LogInformation("Attempting to get ADX Connection for {url}", url);

		var uri = new Uri(url);
		var authority = uri.GetLeftPart(UriPartial.Authority);

		var scopes = new TokenRequestContext(scopes: new string[] { authority.TrimEnd('/') + "/.default" });

		var kcsb = new KustoConnectionStringBuilder(url)
			.WithAadTokenProviderAuthentication(async () =>
			{
				var resp = await credential.GetTokenAsync(scopes);
				var remaining = resp.ExpiresOn - DateTimeOffset.Now;
				if (resp.ExpiresOn != lastTokenExpires || remaining < TimeSpan.FromMinutes(5))
				{
					lastTokenExpires = resp.ExpiresOn;
					logger.LogInformation("ADX Token obtained, good until {expiresOn} ({remaining})", resp.ExpiresOn, remaining);
				}
				var token = resp.Token;
				return token;
			});

		var client = KustoClientFactory.CreateCslQueryProvider(kcsb);
		logger.LogInformation("ADX client for '{url}' created", url);
		return client;
	}

	private AsyncRetryPolicy retryPolicy(Action renewer) => Policy
		.Handle<Kusto.Data.Exceptions.KustoRequestException>(kre => kre.ErrorReason == "Unauthorized")
		.Or<Azure.Identity.AuthenticationFailedException>()  // Seems to happen after 3 hours
		.Or<Kusto.Data.Exceptions.KustoClientException>()  // A bit generic but sometimes ADX forcibly closes the connection
		.Or<System.Net.Sockets.SocketException>()  // A bit generic but sometimes ADX forcibly closes the connection
		.Or<System.IO.IOException>(ioe => ioe.Message.Contains("Received an unexpected EOF"))  // laptop went to sleep
		.WaitAndRetryAsync(3, retryAttempt =>
		{
			logger.LogWarning("Renewing auth token for ADX");
			renewer();
			return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
		});

	//The bigger the page size the less chatty to ADX
	const int limit = 200000;

	//Precision is important otherwise the reader may get stuck
	//in the top while(any && !finished) loop because ADX returned the last item that is slightly bigger (1-2 millisecond precision) in its timestamp
	//and so it sees it as another point but actually it was the last point of the previous loop
	string FormatDateTime(DateTime d) => $"datetime({d.ToString("yyyy-MM-dd HH:mm:ss.fffffff")})";

	private string QueryString(DateTime after, DateTime before, string? idFilters, IEnumerable<string>? ruleIds = null, bool useExternalTable = false)
	{
		string tableSource;

		if (ruleIds?.Count() > 0 && useExternalTable)
		{
			tableSource = @$"let partition = external_table('RuleTimeSeriesMappingTable') | where RuleId in ('{string.Join("','", ruleIds)}');
							let trendIdsFilter = partition | where strlen(TrendId) > 0 | distinct TrendId | summarize trendIds = make_list(TrendId) | project trendIds;
							let externalIdsFilter = partition | where strlen(ExternalId) > 0 | extend ids = strcat(ExternalId, '_', ConnectorId) | distinct ids | summarize externalIds = make_list(ids) | project externalIds;
							Telemetry
							| where SourceTimestamp > {FormatDateTime(after)} and SourceTimestamp < {FormatDateTime(before)}
							| where (strlen(TrendId) > 0 and TrendId in (trendIdsFilter)) or (strlen(ExternalId) > 0 and strcat(ExternalId, '_', ConnectorId) in (externalIdsFilter))";
		}
		else
		{
			tableSource = $"Telemetry | where SourceTimestamp > {FormatDateTime(after)} and SourceTimestamp < {FormatDateTime(before)} {idFilters}";
		}

		
		string query = $"{tableSource} | order by SourceTimestamp asc | limit {limit} | project SourceTimestamp, ScalarValue, TrendId, ExternalId, ConnectorId, EnqueuedTimestamp, Properties = tostring(Properties)";

		return query;
	}

	/// <summary>
	/// Run a Kusto Query but do it in pages because Kusto can't just stream the whole lot to us
	/// </summary>
	/// <remarks>
	/// Pump the results into a bounded channel that will be consumed by a separate task
	/// </remarks>
	public (Task producer, ChannelReader<RawData> reader) RunRawQueryPaged(
		DateTime earliest,
		DateTime latestStop,
		IEnumerable<IdFilter>? idFilters = null,
		IEnumerable<string>? ruleIds = null,
		CancellationToken cancellationToken = default)
	{
		var channel = Channel.CreateBounded<RawData>(limit * 2);
		var producer = Producer(earliest, latestStop, channel.Writer, idFilters, ruleIds, cancellationToken);
		return (producer, channel.Reader);
	}

	private async Task Producer(
		DateTime earliest,
		DateTime latestStop,
		ChannelWriter<RawData> channelWriter,
		IEnumerable<IdFilter>? idFilters = null,
		IEnumerable<string>? ruleIds = null,
		CancellationToken cancellationToken = default)
	{
		bool useExternalTable = false;

		TimeSpan windowRange = TimeSpan.FromHours(2);

		if (ruleIds is not null && ruleIds?.Count() > 0)
		{
			useExternalTable = await ExternableTableExists(ruleIds);

			if(useExternalTable)
			{
				//larger range for specific rules
				windowRange = TimeSpan.FromHours(24);
			}
		}

		string? idFilterFormatted = string.Empty;

		if (idFilters?.Count() > 0)
		{
			idFilterFormatted = $"and ({string.Join(" or ", idFilters.Select(v => v.Format()))})";

			//larger range for specific rules
			windowRange = TimeSpan.FromDays(30);
		}

		//we use a date range to query ADX. This is especially important for rule specific execution
		//for it to go faster
		DateTime before = earliest.Add(windowRange);

		if (before > latestStop)
		{
			before = latestStop;
		}

		string adxQueryPaged = QueryString(earliest, before, idFilterFormatted, ruleIds, useExternalTable);

		ICslQueryProvider client = clientFactory();

		var policy = retryPolicy(() => client = clientFactory());

		bool any = true;
		bool finished = false;
		bool success = false;
		while (any && !finished)
		{
			any = false;
			int rowCount = 0;

			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				// Make a Kusto query
				var properties = new ClientRequestProperties()
				{
					Application = "Rules",
					//AuthorizationScheme = "",
					ClientRequestId = Guid.NewGuid().ToString()
				};

				await policy.ExecuteAsync(async () =>
				{
					healthCheckADX.Current = HealthCheckADX.Healthy;

					using (var dataSet = await client.ExecuteQueryV2Async(this.databaseName, adxQueryPaged, properties))
					{
						success = true;
						using (var frames = dataSet.GetFrames())
						{
							DateTimeOffset startedReading = DateTimeOffset.Now;
							var frameNum = 0;
							while (!finished && frames.MoveNext())  // order matters, when finished don't read more
							{
								if (DateTimeOffset.Now > startedReading.AddHours(1))
								{
									logger.LogWarning("Restarting ADX query as it appears it cannot survive longer than three hours");
									break;
								}

								var frame = frames.Current;
								foreach (var raw in UnpackFrames(frameNum, frame))
								{
									if (raw.SourceTimestamp > latestStop)
									{
										logger.LogInformation("ADX: Passed end of time window, stopping");
										finished = true; break;
									};

									any = true;
									rowCount++;
									earliest = raw.SourceTimestamp;  // needed to handle exception cases and restarting
									await channelWriter.WriteAsync(raw, cancellationToken);
								}
								if (finished) break; // break out of both loops when finished
							}

							//move to next date window if no records found in the window range
							if(!any && before < latestStop)
							{
								any = true;
								earliest = before;
							}
						}
					}
				});
			}
			catch (Azure.Identity.AuthenticationFailedException ex)
			{
				logger.LogError(ex, "ADX Reader Authentication failed exception");
				healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
				// Wait 30s before retry
				await Task.Delay(30000, cancellationToken);
				// And try to recover by continuing in loop
				any = true;
				continue;
			}
			catch (Kusto.Data.Exceptions.KustoRequestThrottledException ex)
			{
				logger.LogInformation("ADX Reader Throttled Exception {ex}", ex.Message);
				healthCheckADX.Current = HealthCheckADX.RateLimited;
				// Wait 30s before retry
				await Task.Delay(30000, cancellationToken);
				// And try to recover by continuing in loop
				any = true;
				continue;
			}
			catch (Kusto.Data.Exceptions.KustoRequestDeniedException ex)
			{
				logger.LogError(ex, "ADX Reader failed");
				logger.LogInformation("Closing channel, authorization failure is not recoverable");
				healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
				channelWriter.Complete(ex);
				throw;
			}
			catch (System.IO.IOException ioex) when (ioex.Message == "The decryption operation failed, see inner exception.")
			{
				logger.LogInformation("AXDService: {message}", ioex.Message);
				healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
				// Wait 30s before retry
				await Task.Delay(30000, cancellationToken);
				// And try to recover by continuing in loop
				any = true;
				continue;
			}
			// Kusto.Data.Exceptions.SemanticException -- ???
			//
			catch (OperationCanceledException ex)
			{
				logger.LogWarning("Shutting down");
				channelWriter.Complete(ex);
				finished = true;
			}
			catch (Exception ex)
			{
				healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
				logger.LogError(ex, "ADX Reader failed");
				logger.LogWarning("Closing channel, unexpected exception `{exceptionMessage}` is not recoverable", ex.Message);
				channelWriter.Complete(ex);
				// Cannot recover an unknown exception
				throw;
			}

			//move forward
			before = earliest.Add(windowRange);

			//don't go over max time
			if (before > latestStop)
			{
				before = latestStop;
			}

			// And move to the next tranche (might miss one if they have duplicate timestamps, ugh!)
			adxQueryPaged = QueryString(earliest, before, idFilterFormatted, ruleIds, useExternalTable);
		}

		if (success)
			logger.LogInformation("ADX Query completed, closing channel");
		else
			logger.LogWarning("ADX Query failed, closing channel");

		channelWriter.TryComplete();

		logger.LogInformation("Marked channel as complete");
	}


	IEnumerable<RawData> UnpackFrames(int frameNum, ProgressiveDataSetFrame frame)
	{
		switch (frame.FrameType)
		{
			case FrameType.DataSetHeader when frame is ProgressiveDataSetHeaderFrame:
				{
					// This is the first frame we'll get back
					//var banner = $"[{frameNum}] DataSet/HeaderFrame: Version={frameex.Version}, IsProgressive={frameex.IsProgressive}";
					//Console.WriteLine(banner);
					//Console.WriteLine();
				}
				break;

			case FrameType.TableHeader when frame is ProgressiveDataSetDataTableSchemaFrame:
				// (Progressive mode only)
				// This frame appears once for each table, before the table's data
				// is reported.
				{
					//var banner = $"[{frameNum}] DataTable/SchemaFrame: TableId={frameex.TableId}, TableName={frameex.TableName}, TableKind={frameex.TableKind}";
					//Console.WriteLine(banner);
				}
				break;

			case FrameType.TableFragment when frame is ProgressiveDataSetDataTableFragmentFrame frameex:
				// (Progressive mode only)
				// This frame provides one part of the table's data
				{
					//var banner = $"[{frameNum}] DataTable/FragmentFrame: TableId={frameex.TableId}, FieldCount={frameex.FieldCount}, FrameSubType={frameex.FrameSubType}";
					var record = new object[frameex.FieldCount];
					while (frameex.GetNextRecord(record))
					{
						bool ok = false;
						RawData rawData = default;

						try
						{
							if (frameex.FieldCount < 4)
							{
								logger.LogWarning("Bad telemetry data, too short");
								continue;
							}

							//logger.LogDebug($"TableFragment: {JsonConvert.SerializeObject(record)}");
							var sourceTimestamp = (DateTime)record[0];
							var enqueuedTimestamp = (DateTime)record[5];
							double value = GetValue(record);
							string externalId = record[3] as string ?? "";
							string connectorId = record[4] as string ?? "";
							string textValue = record[6] as string ?? "";

							rawData = new RawData
							{
								PointEntityId = record[2].ToString(),  // maybe a Guid or a string depending on ADX flavor
								SourceTimestamp = sourceTimestamp,
								EnqueuedTimestamp = enqueuedTimestamp,
								Value = value,
								ExternalId = externalId,
								ConnectorId = connectorId,
								TextValue = textValue
							};
							ok = true;
						}
						catch (Exception ex)
						{
							throttledLogger.LogError(ex, "Failed to process ADX data");
						}

						if (ok)
						{
							yield return rawData;
						}
					}
				}
				break;

			case FrameType.TableCompletion when frame is ProgressiveDataSetTableCompletionFrame:
				// (Progressive mode only)
				// This frame announces the completion of a table (no more data in it).
				{
					//var banner = $"[{frameNum}] DataTable/TableCompletionFrame: TableId={frameex.TableId}, RowCount={frameex.RowCount}";
					//Console.WriteLine(banner);
					//Console.WriteLine();
				}
				break;

			case FrameType.TableProgress when frame is ProgressiveDataSetTableProgressFrame:
				// (Progressive mode only)
				// This frame appears periodically to provide a progress estimateion.
				{
					//var banner = $"[{frameNum}] DataTable/TableProgressFrame: TableId={frameex.TableId}, TableProgress={frameex.TableProgress}";
					//Console.WriteLine(banner);
					//Console.WriteLine();
				}
				break;

			case FrameType.DataTable when frame is ProgressiveDataSetDataTableFrame frameex:
				{
					// This frame represents one data table (in all, when progressive results
					// are not used or there's no need for multiple-frames-per-table).
					// There are usually multiple such tables in the response, differentiated
					// by purpose (TableKind).
					// Note that we can't skip processing the data -- we must consume it.

					//var banner = $"[{frameNum}] DataTable/DataTableFrame: TableId={frameex.TableId}, TableName={frameex.TableName}, TableKind={frameex.TableKind}";

					while (frameex.TableData.Read())
					{
						bool ok = false;
						RawData rawData = default;
						try
						{
							object[] record = new object[frameex.TableData.FieldCount];
							int n = frameex.TableData.GetValues(record);

							//Console.Write(string.Join(",", record.Take(n)));
							if (frameex.TableKind == WellKnownDataSet.PrimaryResult)
							{
								if (n < 4)
								{
									logger.LogWarning("Bad telemetry data, too short");
									continue;
								}

								var sourceTimestamp = (DateTime)record[0];
								var enqueuedTimestamp = (DateTime)record[5];
								double value = GetValue(record);
								string externalId = record[3] as string ?? "";
								string connectorId = record[4] as string ?? "";
								string textValue = record[6] as string ?? "";

								rawData = new RawData
								{
									PointEntityId = record[2].ToString(),  // maybe a Guid or a string depending on ADX flavor
									SourceTimestamp = sourceTimestamp,
									EnqueuedTimestamp = enqueuedTimestamp,
									Value = !double.IsNaN(value) ? value : 0,
									ExternalId = externalId,
									ConnectorId = connectorId,
									TextValue = textValue
								};
								ok = true;
							}
						}
						catch (Exception ex)
						{
							throttledLogger.LogError(ex, "Failed to process ADX data");
						}
						if (ok)
						{
							yield return rawData;
						}
					}

					frameex.TableData.Close();
					//WriteResults(banner, frameex.TableData);
					//Console.WriteLine(banner);
				}
				break;

			case FrameType.DataSetCompletion when frame is ProgressiveDataSetCompletionFrame:
				{
					// This is the last frame in the data set.
					// It provides information on the overall success of the query:
					// Whether there were any errors, whether it got cancelled mid-stream,
					// and what exceptions were raised if either is true.
					//var banner = $"[{frameNum}] DataSet/CompletionFrame: HasErrors={frameex.HasErrors}, Cancelled={frameex.Cancelled}, Exception={frameex.Exception?.Message}";
					//Console.WriteLine(banner);
					//Console.WriteLine();
					yield break;
				}
			//break;

			case FrameType.LastInvalid:
			default:
				// In general this should not happen
				break;
		}
	}

	private double GetValue(object[] record)
	{
		try
		{
			//value can be basic types (IConvertible) or a newtonsoft JValue which is also IConvertible
			if (record[1] is IConvertible c)
			{
				TypeCode typeCode = c.GetTypeCode();

				//empty check caters for newtonsoft's JValue == null
				//otherwise it throws null to double conversion error
				if (typeCode == TypeCode.Empty)
				{
					return 0.0d;
				}

				if (typeCode == TypeCode.String)
				{
					string s = c.ToString(null);

					if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase))
					{
						return 1.0;
					}

					if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase))
					{
						return 0.0;
					}

					// Bad data
					if (string.Equals(s, "#N/A", StringComparison.OrdinalIgnoreCase))
					{
						return 0.0;
					}

					string stringValue = c.ToString(null);

					if (!double.TryParse(stringValue, out double value))
					{
						throttledLogger.LogWarning("External Id {record3} could not convert '{value}' to a double", record[3], stringValue);
					}

					return value;
				}

				return c.ToDouble(null);
			}

			throttledLogger.LogWarning("External Id {record3} failed to handle type {json}", record[3], JsonConvert.SerializeObject(record));
		}
		catch (Exception ex)
		{
			// The input string '#N/A' was not in a correct format.
			throttledLogger.LogError(ex, "External Id {record3} failed to parse {json}", record[3], JsonConvert.SerializeObject(record));
		}

		return 0.0;
	}

	/// <summary>
	/// Returns indicators whether there were any ADT changes since the specified date
	/// </summary>
	public async Task<(bool hasTwinChanges, bool hasRelationshipChanges)> HasADTChanges(DateTime startDate)
	{
		string adxQuery = $"Twins | where ['ExportTime'] > {FormatDateTime(startDate)} | summarize count()";

		var client = clientFactory();

		// Make a Kusto query
		var properties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };

		bool hasTwinChanges = false;
		bool hasRelationshipChanges = false;

		using (var reader = await client.ExecuteQueryAsync(this.databaseName, adxQuery, properties))
		{
			while (reader.Read())
			{
				hasTwinChanges = (long)reader.GetValue(0) > 0;
			}
		}

		adxQuery = $"Relationships | where ['ExportTime'] > {FormatDateTime(startDate)} | summarize count()";

		using (var reader = await client.ExecuteQueryAsync(this.databaseName, adxQuery, properties))
		{
			while (reader.Read())
			{
				hasRelationshipChanges = (long)reader.GetValue(0) > 0;
			}
		}

		return (hasTwinChanges, hasRelationshipChanges);
	}

	/// <summary>
	/// Get latest time stamp
	/// Check whether the rules mapping external table exists
	/// </summary>
	public async Task<bool> ExternableTableExists(IEnumerable<string> ruleIds)
	{
		try
		{
			// Newer format table
			string adxQuery = ".show external tables | where TableName == \"RuleTimeSeriesMappingTable\" | summarize Count=count()";
			long count = 0;

			var client = clientFactory();

			// Make a Kusto query
			var properties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
			using (var reader = await client.ExecuteQueryAsync(this.databaseName, adxQuery, properties))
			{
				while (reader.Read())
				{
					count = ((long)reader.GetValue(0));
					break;
				}
			}

			if(count == 0)
			{
				return false;
			}

			count = 0;

			// dont use the table for local rules that doesn't exist in ADX or for rules that don't have point entity ids
			adxQuery = $"external_table('RuleTimeSeriesMappingTable') | where RuleId in ('{string.Join("','", ruleIds)}') | summarize Count=count()";

			using (var reader = await client.ExecuteQueryAsync(this.databaseName, adxQuery, properties))
			{
				while (reader.Read())
				{
					count = ((long)reader.GetValue(0));
					break;
				}
			}

			return count > 0;
		}
		catch (Exception se)
		{
			logger.LogError(se, "Externable table check failed");
		}

		return false;
	}
}
