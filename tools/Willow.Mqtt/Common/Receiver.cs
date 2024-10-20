using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using static System.Console;
using static System.ConsoleColor;
using static Common.ConsoleHelper;

namespace Common;

/// <summary>
/// Creates an MQTT client for receiving messages.
/// </summary>
public class Receiver
{
    private static IManagedMqttClient? MqttClient;

    /// <summary>
    /// Starts the receiver
    /// </summary>
    /// <param name="clientId">The ID of the client. It must match the certificate subject.</param>
    /// <param name="certificateName">The path to the client certificate.</param>
    /// <param name="subscriptionId">The ID of the subscription. In this example, if it does not match the client ID, the subscription will not be authorised.</param>
    /// <returns><c>0</c> if successful, otherwise <c>-1</c></returns>
    public static async Task<int> Start(string subscriptionId, string certificateName, string passphrase, string mqttUrl)
    {
        ForegroundColor = Gray;
        ClearNotifications();
        SetCursorPosition(0, 0);
        WriteLine("Receiver starting...");

        var mqttFactory = new MqttFactory();
        MqttClient = MqttClientFactory.CreateClient();

        var mqttClientOptions = MqttClientFactory.CreateOptions(subscriptionId, certificateName, passphrase, mqttUrl);

        // Setup message handling before connecting so that queued messages
        // are also handled properly. When there is no event handler attached all
        // received messages get lost.
        MqttClient.ApplicationMessageReceivedAsync += e =>
        {
            ForegroundColor = Yellow;
            WriteLine("Received application message.");
            ForegroundColor = Blue;
            WriteLine(e.ApplicationMessage.ConvertPayloadToString());
            ForegroundColor = Gray;
            WriteLine();

            return Task.CompletedTask;
        };

        MqttClient.SynchronizingSubscriptionsFailedAsync += e =>
        {
            ForegroundColor = Red;
            WriteLine(e.Exception?.Message ?? "Subscription failure");
            return Task.CompletedTask;
        };

        await MqttClient.StartAsync(mqttClientOptions);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic($"telemetry/{subscriptionId}/#").WithAtLeastOnceQoS(); // Required for receiving messages published while offline
                })
            .Build();

        await MqttClient.SubscribeAsync(mqttSubscribeOptions.TopicFilters);

        return 0;
    }

    public static void Stop()
    {
        WriteLine();

        if (MqttClient == null)
        {
            ForegroundColor = Yellow;
            WriteLine("Receiver not started.");
            return;
        }

        MqttClient.StopAsync().Wait();
    }
}
