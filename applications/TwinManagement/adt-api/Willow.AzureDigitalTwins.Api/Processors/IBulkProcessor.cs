using System;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.Model.Async;

namespace Willow.AzureDigitalTwins.Api.Processors
{
    public interface IBulkProcessor<T, R>
    {
        Task ProcessImport(JobsEntry importJob, T entities, CancellationToken cancellationToken);
        Task ProcessDelete(JobsEntry importJob, R twinIds, CancellationToken cancellationToken);
    }
}
