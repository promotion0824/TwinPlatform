using Azure.DigitalTwins.Core;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Ingest;
using Willow.AzureDataExplorer.Model;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;

/// <summary>
/// Class implementation of ITwinProcessor for syncing adx changes.
/// </summary>
public class TwinAdxSyncProcessor : ITwinProcessor
{
    private readonly ILogger<TwinAdxSyncProcessor> _logger;
    private readonly IAdxDataIngestionLocalStore _adxDataIngestionLocalStore;
    private readonly string _adxDatabase;
    private readonly TwinProcessorOption _jobProcessorOption;
    private readonly ICustomColumnService _customColumnService;
    private readonly IAdxService _adxService;
    private readonly JsonDiffPatch diffPatch = new();
    private IDictionary<string, IDictionary<ExportColumn, string>> _adxTwinIdColumnValueDict;
    private readonly ITelemetryCollector _telemetryCollector;

    private const string tagColumnName = "Tags";
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="adxDataIngestionLocalStore">Azure Data Explorer Ingestion Local Store.</param>
    /// <param name="azureDataExplorerOptions">Azure Data Explorer Options</param>
    /// <param name="configuration">IConfiguration Instance.</param>
    /// <param name="customColumnService">Implementation of ICustomColumnService</param>
    /// <param name="adxService">Instance of IAdxService.</param>
    public TwinAdxSyncProcessor(ILogger<TwinAdxSyncProcessor> logger,
        IAdxDataIngestionLocalStore adxDataIngestionLocalStore,
        IOptions<AzureDataExplorerOptions> azureDataExplorerOptions,
        IConfiguration configuration,
        ICustomColumnService customColumnService,
        IAdxService adxService,
        ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _adxDataIngestionLocalStore = adxDataIngestionLocalStore;
        _adxDatabase = azureDataExplorerOptions.Value.DatabaseName;
        _jobProcessorOption = configuration.GetRequiredSection("TwinJobProcessor").GetRequiredSection(nameof(TwinAdxSyncProcessor)).Get<TwinProcessorOption>();
        _customColumnService = customColumnService;
        _adxService = adxService;
        _telemetryCollector = telemetryCollector;
    }

    /// <summary>
    /// Twin Processor Option
    /// </summary>
    public TwinProcessorOption twinProcessorOption => _jobProcessorOption;

    /// <summary>
    /// Preloads all the twins from the batch into the processor to speed up retrieval.
    /// </summary>
    /// <param name="twinIds">Enumerable of twin Ids to load.</param>
    /// <param name="token">Cancellation Token.</param>
    /// <returns>Awaitable task.</returns>
    public async Task PreLoadTwinsAsync(IEnumerable<string> twinIds, CancellationToken token)
    {
        // Clear the previous data. 
        _adxTwinIdColumnValueDict = null;

        // Reload with the new twinIds
        _adxTwinIdColumnValueDict = await _adxService.GetRawTwinsById(twinIds.ToArray());
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
        using (_logger.BeginScope("Executing ADX Synchronization for twin with Id: {twinId}", twin.Id))
        {
            try
            {
                _adxTwinIdColumnValueDict.TryGetValue(twin.Id, out var adxTwinColumnValues);
                var columnValues = await _customColumnService.CalculateEntityColumns(twin, EntityType.Twins, false);

                // CREATE Adx Record SCENARIO
                // adxTwin will be null, if there is no twin that previously ingested
                // Ingest a twin record with all the column values

                // UPADTE Adx Record SCENARIO
                // Compare the twins with column values to check if the values changed, if it is then Update else Ignore
                if (adxTwinColumnValues is null || !AreTwinColumnValuesEqual(columnValues, adxTwinColumnValues))
                {
                    var ingestedRowCount = await _adxDataIngestionLocalStore.AddRecordForIngestion(_adxDatabase, AdxConstants.TwinsTable, columnValues.ToDictionary(x => new AdxColumn(x.Key.Name, (ColumnType)(int)x.Key.Type), y => (object)y.Value));
                    _telemetryCollector.TrackADXTwinCreationCount(ingestedRowCount);
                }
                else
                {
                    _logger.LogTrace("Ignoring sync changes with ADX for twin with Id: {twinId}", twin.Id);
                }

            }
            catch (Exception)
            {
                _logger.LogError("Error executing adx sync for twin with Id :{twinId}", twin.Id);

                throw;
            }
        }

        return twin;
    }

    /// <summary>
    /// Logic to execute post processing of twins.
    /// </summary>
    /// <returns>Awaitable task.</returns>
    public Task PostLoadTwinAsync()
    {
        return _adxDataIngestionLocalStore.FlushLocalStore();
    }


    private bool AreTwinColumnValuesEqual(IDictionary<ExportColumn, string> newColumnValues, IDictionary<ExportColumn, string> oldColumnValues)
    {
        // Check for differences in the lifted column values
        bool columnsToCompare(KeyValuePair<ExportColumn, string> w) => !(w.Key.IsIngestionTimeColumn || w.Key.IsDeleteColumn || w.Key.Type == CustomColumnType.Object) || w.Key.Name == tagColumnName;
        var newValues = newColumnValues.Where(columnsToCompare).ToDictionary(a => a.Key.Name, b => b.Value);
        var oldValues = oldColumnValues.Where(columnsToCompare).ToDictionary(a => a.Key.Name, b => b.Value);

        var differedValues = newValues.Where(x => !oldValues.ContainsKey(x.Key) || oldValues[x.Key] != x.Value).ToList();

        if (differedValues.Any())
        {
            _logger.LogInformation("ADX Found difference: {differences}", string.Join(',', differedValues));
            return false;
        }

        //If no difference in the lifted scalar columns, Check for difference in the dynamic columns except tags (Tags are not json columns)
        var dynamicColNames = oldColumnValues.Keys.Where(w => w.Type == CustomColumnType.Object && w.Name != tagColumnName).ToList();

        foreach (var dynamicColName in dynamicColNames.Select(x => x.Name))
        {
            var oldRawValue = oldColumnValues.Where(w => w.Key.Name == dynamicColName).Select(s => s.Value).FirstOrDefault();
            var newRawValue = newColumnValues.Where(w => w.Key.Name == dynamicColName).Select(s => s.Value).FirstOrDefault();

            if (oldRawValue is null && newRawValue is null) continue;
            if ((oldRawValue is null && newRawValue is not null) || (oldRawValue is not null && newRawValue is null))
            {
                _logger.LogInformation("ADX Found difference: {colName}:{value}", dynamicColName, newRawValue);
                return false;
            }

            var fullEntityDiff = diffPatch.Diff(oldRawValue, newRawValue);
            if (fullEntityDiff is not null)
            {
                _logger.LogInformation("ADX Found difference: {differences}", fullEntityDiff);
                return false;
            }
        }

        return true;
    }
}
