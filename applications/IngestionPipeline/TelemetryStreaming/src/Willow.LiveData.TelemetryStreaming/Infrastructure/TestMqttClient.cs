#pragma warning disable CS0067 // The event is never used
namespace Willow.LiveData.TelemetryStreaming.Infrastructure;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;

internal class TestMqttClient : IManagedMqttClient
{
    public event Func<ApplicationMessageProcessedEventArgs, Task>? ApplicationMessageProcessedAsync;

    public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceivedAsync;

    public event Func<ApplicationMessageSkippedEventArgs, Task>? ApplicationMessageSkippedAsync;

    public event Func<MqttClientConnectedEventArgs, Task>? ConnectedAsync;

    public event Func<ConnectingFailedEventArgs, Task>? ConnectingFailedAsync;

    public event Func<EventArgs, Task>? ConnectionStateChangedAsync;

    public event Func<MqttClientDisconnectedEventArgs, Task>? DisconnectedAsync;

    public event Func<SubscriptionsChangedEventArgs, Task>? SubscriptionsChangedAsync;

    public event Func<ManagedProcessFailedEventArgs, Task>? SynchronizingSubscriptionsFailedAsync;

    public IMqttClient InternalClient => throw new NotSupportedException();

    public bool IsConnected => true;

    public bool IsStarted => true;

    public ManagedMqttClientOptions Options => new();

    public int PendingApplicationMessagesCount => 0;

    public void Dispose()
    {
    }

    public Task EnqueueAsync(MqttApplicationMessage applicationMessage)
    {
        return Task.CompletedTask;
    }

    public Task EnqueueAsync(ManagedMqttApplicationMessage applicationMessage)
    {
        return Task.CompletedTask;
    }

    public Task PingAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(ManagedMqttClientOptions options)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(bool cleanDisconnect = true)
    {
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(ICollection<MqttTopicFilter> topicFilters)
    {
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(ICollection<string> topics)
    {
        return Task.CompletedTask;
    }
}
#pragma warning restore CS0067 // The event is never used
