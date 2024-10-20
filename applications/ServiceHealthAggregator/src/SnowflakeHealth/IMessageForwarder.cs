namespace Willow.ServiceHealthAggregator.Snowflake;

internal interface IMessageForwarder
{
    Task ForwardAsync(Notification notification, CancellationToken cancellationToken = default);
}
