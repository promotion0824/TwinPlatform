using Azure.DigitalTwins.Core;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.CognitiveSearch;
namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;

/// <summary>
/// Class implementation of ITwinProcessor to update ACS index.
/// </summary>
public class TwinAcsSyncProcessor : ITwinProcessor
{
    private readonly ILogger<TwinAcsSyncProcessor> _logger;
    private readonly IAcsService _acsService;
    private readonly TwinProcessorOption _jobProcessorOption;
    private IDictionary<string,UnifiedItemDto> searchIndexDict;
    private readonly JsonDiffPatch diffPatch = new();
    private readonly ITelemetryCollector _telemetryCollector;

    public TwinProcessorOption twinProcessorOption => _jobProcessorOption;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">ILogger Instance</param>
    /// <param name="acsService">IAcsService Instance.</param>
    /// <param name="configuration">IConfiguration Instance.</param>
    /// <param name="telemetryCollector">Telemetry Collector.</param>
    public TwinAcsSyncProcessor(ILogger<TwinAcsSyncProcessor> logger,
        IAcsService acsService,
        IConfiguration configuration,
        ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _acsService = acsService;
        _jobProcessorOption = configuration.GetRequiredSection("TwinJobProcessor").GetRequiredSection(nameof(TwinAcsSyncProcessor)).Get<TwinProcessorOption>();
        _telemetryCollector = telemetryCollector;
    }

    /// <summary>
    /// Method to execute the twin task
    /// </summary>
    /// <param name="twin">Target twin to process</param>
    /// <param name="modelMappings">Model Mapping Dictionary.</param>
    /// <param name="token">cancellation token</param>
    /// <returns>Awaitable task that returns updated Basic Digital Twin</returns>
    public async Task<BasicDigitalTwin> ExecuteTaskAsync(BasicDigitalTwin twin, IDictionary<string, string> modelMappings, CancellationToken token)
    {
        using (_logger.BeginScope($"{nameof(TwinAcsSyncProcessor)} processing Id:{twin.Id}"))
        {
            var newSearchDocTwin = await _acsService.BuildSearchIndexDocumentFromTwin(twin);

            searchIndexDict.TryGetValue(twin.Id, out var existingSearchDoc);

            if (existingSearchDoc != null)
            {
                newSearchDocTwin.Key = existingSearchDoc.Key;
            }

            // HappyPath => Create: If there is no existing twin document in the index, we simply insert a new one.
            // Update :For twin document that already exist in the index, we deep compare the newly generated document
            //          with the existing one to see if there is any difference before we insert; ignore the update if no difference.

            // We could have simply update the document without comparison. Since the background scan job can trigger 2 or more times
            // for the same twin (due to write back to ADT changing the lastupdatetime) we do the comparison to minimize the writes to ACS.
            if (existingSearchDoc is null || !DeepCompareObjects(newSearchDocTwin, existingSearchDoc, nameof(UnifiedItemDto.Latest), nameof(UnifiedItemDto.IndexedDate)))
            {
                await _acsService.QueueForUpsertTwinAsync(twin);
                _logger.LogDebug("Uploaded twin with dtId: {Id} to ACS queue for upsert", twin.Id);
            }
        }
        return twin;
    }

    /// <summary>
    /// Preloads all the twins from the batch into the processor to speed up retrieval.
    /// </summary>
    /// <param name="twinIds">Enumerable of twin Ids to load.</param>
    /// <param name="token">Cancellation Token.</param>
    /// <returns>Awaitable task.</returns>
    public async Task PreLoadTwinsAsync(IEnumerable<string> twinIds, CancellationToken token)
    {
        _logger.LogInformation("{ClassName} start preloading count:{count} ACS documents", nameof(TwinAcsSyncProcessor),twinIds.Count());
        var searchDocumentList = await _acsService.GetUnifiedIndexDocumentsByTwinIds(twinIds);
        searchIndexDict = searchDocumentList.ToDictionary(x => x.Id, y => y);
        _logger.LogInformation("{ClassName} pre loading ACS documents completed. Loaded Count:{count} documents.", nameof(TwinAcsSyncProcessor), searchDocumentList.Count);
    }

    /// <summary>
    /// Logic to execute post processing of twins.
    /// </summary>
    /// <returns>Awaitable task.</returns>
    public Task PostLoadTwinAsync()
    {
        var flushResults = _acsService.Flush();
        _telemetryCollector.TrackACSFlushInsertedRowCount(flushResults.Result.insertedDocsCount);
        _telemetryCollector.TrackACSFlushFailedRowCount(flushResults.Result.failedDocsCount);
        _telemetryCollector.TrackACSFlushPendingRowCount(flushResults.Result.pendingDocsCount);
        _telemetryCollector.TrackACSFlushDeletedRowCount(flushResults.Result.deletedDocsCount);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Todo: Move this method in to any helper class
    /// </summary>
    /// <typeparam name="T">Type of Object.</typeparam>
    /// <param name="newObj">Object to compare.</param>
    /// <param name="existingObj">Object to compare against.</param>
    /// <param name="propertiesToIgnore">Property names to ignore comparison.</param>
    /// <returns>True if same; false if different.</returns>
    private bool DeepCompareObjects<T>(T newObj, T existingObj, params string[] propertiesToIgnore)
    {
        string newObjSerialized = JsonSerializer.Serialize<T>(newObj);
        string existingObjSerialized = JsonSerializer.Serialize<T>(existingObj);
        var fullEntityDiff = diffPatch.Diff(newObjSerialized, existingObjSerialized);
        if (fullEntityDiff is not null)
        {
            var differedProps = JsonSerializer.Deserialize<IDictionary<string, object>>(fullEntityDiff);
            if (!differedProps.Keys.Except(propertiesToIgnore).Any()) return true;

            _logger.LogInformation("ACS Found difference: {differences}", fullEntityDiff);
            return false;
        }
        return true;
    }
}
