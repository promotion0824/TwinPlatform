using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;
public class TwinCustomColumnProcessor : ITwinProcessor
{

    private readonly ILogger<TwinCustomColumnProcessor> _logger;
    private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
    private readonly TwinProcessorOption _jobProcessorOption;
    private readonly ICustomColumnService _customColumnService;

    /// <summary>
    /// Twin Custom Column Processor Constructor
    /// </summary>
    /// <param name="logger">Logger Instance</param>
    /// <param name="azureDigitalTwinWriter">Implementation for IAzureDigitalTwinWriter</param>
    /// <param name="customColumnService">Implementation for ICustomColumnService</param>
    /// <param name="configuration">IConfiguration Instance.</param>
    public TwinCustomColumnProcessor(ILogger<TwinCustomColumnProcessor> logger,
        IAzureDigitalTwinWriter azureDigitalTwinWriter,
        ICustomColumnService customColumnService,
        IConfiguration configuration)
    {
        _logger = logger;
        _azureDigitalTwinWriter = azureDigitalTwinWriter;
        _jobProcessorOption = configuration.GetRequiredSection("TwinJobProcessor").GetRequiredSection(nameof(TwinCustomColumnProcessor)).Get<TwinProcessorOption>();
        _customColumnService = customColumnService;
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
    public Task PreLoadTwinsAsync(IEnumerable<string> twinIds, CancellationToken token)
    {
        // ADT Twin will be loaded and iterated in the respective job execute method.
        // Preloading here would be unnecessary.
        return Task.CompletedTask;
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
        using (_logger.BeginScope("Calculating column values for twin with Id: {twinId}", twin.Id))
        {
            try
            {
                bool hasUpdates = false;

                if (TryUpdateTwinModel(twin, modelMappings))
                {
                    hasUpdates = true;
                }


                var columnValues = await _customColumnService.CalculateEntityColumns(twin,
                    EntityType.Twins,
                    deleted: false,
                    columnFilter: x => x.WriteBackToADT,
                    reEvaluate: false);

                // Find and update twin property that needs to be updated
                if (TryUpdateTwinColumns(twin, columnValues))
                {
                    hasUpdates = true;
                }

                if (hasUpdates)
                {
                    // Update the twin with the updated values
                    var updatedTwin = await _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin, token);

                    _logger.LogTrace("Done updating custom column values for twin with Id:{twinId}", twin.Id);

                    // return updated twin 
                    return updatedTwin;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating custom column values for twin with Id :{twinId}", twin.Id);
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
        return Task.CompletedTask;
    }

    private bool TryUpdateTwinColumns(BasicDigitalTwin twin, IDictionary<ExportColumn, string> columnValues)
    {
        bool hasUpdates = false;
        foreach (var (column, columnValue) in columnValues)
        {
            if (columnValue is null)
            {
                // if new column value to update is null, then remove the prop from twin content
                twin.Contents.Remove(column.AdtPropName);
                _logger.LogTrace("Removing twin column {columnName} ", column.AdtPropName);
            }
            else
            {
                if (twin.Contents.FirstOrDefault(a => string.Equals(a.Key, column.Name, StringComparison.InvariantCultureIgnoreCase)).Value?.ToString() != columnValue)
                {
                    twin.Contents[column.AdtPropName] = columnValue;
                    _logger.LogTrace("Updating twin column {columnName} with value {value}", column.AdtPropName, columnValue);
                    hasUpdates = true;
                }
            }
        }
        return hasUpdates;
    }

    private bool TryUpdateTwinModel(BasicDigitalTwin twin, IDictionary<string, string> modelMapping)
    {
        if (modelMapping is null)
            return false;

        if (modelMapping.TryGetValue(twin.Metadata.ModelId, out string toModelId))
        {
            _logger.LogTrace("Updating twin with Id:{ID} from Model:{FromModel} to Model:{ToModel}.", twin.Id, twin.Metadata.ModelId, toModelId);
            twin.Metadata.ModelId = toModelId;
            return true;
        }

        return false;
    }
}

