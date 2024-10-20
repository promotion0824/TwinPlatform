using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Ingest;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Extensions.Logging;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Class implementation for IJobProcessor to periodically flush adx records.
/// </summary>
public class AdxFlushJob : BaseJob<JobBaseOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "AdxFlush";

    private readonly ILogger<AdxFlushJob> _logger;
    private readonly IAdxDataIngestionLocalStore _adxDataIngestionLocalStore;
    private readonly ITelemetryCollector _telemetryCollector;

    /// <summary>
    /// Instantiate new instance of <see cref="AdxFlushJob"/>
    /// </summary>
    /// <param name="logger">Instance of ILogger.</param>
    /// <param name="adxDataIngestionLocalStore">Instance of IAdxDataIngestionLocalStore.</param>
    /// <param name="telemetryCollector">Telemetry Collector Service Instance.</param>
    public AdxFlushJob(ILogger<AdxFlushJob> logger, IAdxDataIngestionLocalStore adxDataIngestionLocalStore, ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _adxDataIngestionLocalStore = adxDataIngestionLocalStore;
        _telemetryCollector = telemetryCollector;
    }


    /// <summary>
    /// Method to execute the background jobs
    /// </summary>
    /// <param name="jobsEntry"> Jobs Entry.</param>
    /// <param name="jobExecutionContext"> Job Execution Context.</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Awaitable task</returns>
    public async Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Flushing all the adx store records.");

        var ingestedRowCount = await MeasureExecutionTime.ExecuteTimed(async () => await _adxDataIngestionLocalStore.FlushLocalStore(),
                (res, ms) =>
                {
                    _logger.LogInformation($"AdxIngestion flush took :{ms} milliseconds");
                    _telemetryCollector.TrackADXFlushJobExecutionTime(ms);
                });

        _telemetryCollector.TrackADXFlushedRowCount(ingestedRowCount);
    }
}
