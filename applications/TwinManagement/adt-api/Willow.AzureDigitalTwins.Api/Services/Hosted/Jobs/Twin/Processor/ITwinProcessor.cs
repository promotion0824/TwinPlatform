using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;

/// <summary>
/// Abstract interface for processing twin task. 
/// </summary>
public interface ITwinProcessor
{
    /// <summary>
    /// Preloads all the twins from the batch into the processor to speed up retrieval.
    /// </summary>
    /// <param name="twinIds">Enumerable of twin Ids to load.</param>
    /// <param name="token">Cancellation Token.</param>
    /// <returns>Awaitable task.</returns>
    public Task PreLoadTwinsAsync(IEnumerable<string> twinIds, CancellationToken token);

    /// <summary>
    /// Method to execute the twin task
    /// </summary>
    /// <param name="twin">Target twin to process</param>
    /// <param name="modelMappings">Model Mapping Dictionary.</param>
    /// <param name="token">cancellation token</param>
    /// <returns>Awaitable task of updated Basic Digital Twin</returns>
    public Task<BasicDigitalTwin> ExecuteTaskAsync(BasicDigitalTwin twin, IDictionary<string, string> modelMappings, CancellationToken token);

    /// <summary>
    /// Logic to execute post processing of twins.
    /// </summary>
    /// <returns>Awaitable task.</returns>
    public Task PostLoadTwinAsync();

    /// <summary>
    /// Twin Processor Option
    /// </summary>
    TwinProcessorOption twinProcessorOption { get; }

}
