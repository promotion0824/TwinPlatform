namespace Willow.LiveData.Pipeline.EventHub;

internal interface IBatchProcessor
{
    Task StartProcessingAsync(CancellationToken cancellationToken);

    Task StopProcessingAsync(CancellationToken cancellationToken);
}
