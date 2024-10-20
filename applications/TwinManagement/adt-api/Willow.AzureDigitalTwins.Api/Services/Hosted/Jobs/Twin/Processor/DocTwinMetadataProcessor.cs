using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;

/// <summary>
/// Class implementation of ITwinProcessor to update document blob metadata.
/// </summary>
public class DocTwinMetadataProcessor(ILogger<DocTwinMetadataProcessor> logger, IDocumentService documentService, IConfiguration configuration) : ITwinProcessor
{
    public TwinProcessorOption twinProcessorOption => configuration
        .GetRequiredSection("TwinJobProcessor")
        .GetRequiredSection(nameof(DocTwinMetadataProcessor))
        .Get<TwinProcessorOption>();

    /// <summary>
    /// Method to execute the twin task
    /// </summary>
    /// <param name="twin">Target twin to process</param>
    /// <param name="modelMappings">Model Mapping Dictionary.</param>
    /// <param name="token">cancellation token</param>
    /// <returns>Awaitable task that returns updated Basic Digital Twin</returns>
    public async Task<BasicDigitalTwin> ExecuteTaskAsync(BasicDigitalTwin twin, IDictionary<string, string> modelMappings, CancellationToken token)
    {
        using (logger.BeginScope($"{nameof(DocTwinMetadataProcessor)} processing Id:{twin.Id}"))
        {
            if (documentService.IsValidDocumentTwin(twin))
            {
                await documentService.UpdateDocumentBlobMetaData(twin);
            }

            return twin;
        }
    }

    public Task PostLoadTwinAsync()
    {
        return Task.CompletedTask;
    }

    public Task PreLoadTwinsAsync(IEnumerable<string> twinIds, CancellationToken token)
    {
        return Task.CompletedTask;
    }
}
