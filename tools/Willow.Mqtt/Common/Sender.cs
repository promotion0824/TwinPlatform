using System.Text;
using MQTTnet;
using static System.Console;
using static System.ConsoleColor;

namespace Common;
public static class Sender
{
    public static async Task<int> Start(string subscriptionId, string certificate, string passphrase, string mqttUrl, string destinationSubscriptionId, string connectorId, string externalId)
    {
        using var mqttClient = MqttClientFactory.CreateClient();

        var mqttClientOptions = MqttClientFactory.CreateOptions(subscriptionId, certificate, passphrase, mqttUrl);

        mqttClient.ApplicationMessageProcessedAsync += e =>
        {
            if (e.Exception != null)
            {
                WriteLine();
                ForegroundColor = Red;
                WriteLine("Error publishing");
                WriteLine(e.Exception.Message);
            }

            return Task.CompletedTask;
        };

        try
        {
            WriteLine("Connecting...");
            await mqttClient.StartAsync(mqttClientOptions);
        }
        catch (Exception ex)
        {
            WriteLine(ex.ToString());
            return -1;
        }

        Random valueGenerator = new(Guid.NewGuid().GetHashCode());

        do
        {
            string topic = $"telemetry/{destinationSubscriptionId}";

            RawData message = new()
            {
                SourceTimestamp = DateTime.Now.AddMinutes(-15).AddMilliseconds(-valueGenerator.Next(3000000)),
                EnqueuedTimestamp = DateTime.Now,
                Value = Math.Round(valueGenerator.NextDouble() * 10.0, 2),
                ExternalId = externalId,
                ConnectorId = connectorId,
                Metadata = new WalmartMetadata
                {
                    ModelId = "dtmi:willowinc:Occupancy",
                    TwinId = "SPC12f452gs7232353",
                    NuvoloId = "LOC12345",
                    Code = "Room 23",
                    GeometryViewerId = new Guid("d4a5a3fe-edb8-4366-862e-2cf7fc6d5def"),
                    Unit = "ppl",
                }
            };

            topic += $"/{connectorId}/{externalId}";

            ForegroundColor = Yellow;
            WriteLine("Sending application message...");
            ForegroundColor = Blue;

            WriteLine(message);

            ForegroundColor = Gray;

            await mqttClient.EnqueueAsync(new MqttApplicationMessage()
            {
                ContentType = "application/json",
                Retain = false, // Must be false as Event Grid does not support it
                Topic = topic,
                PayloadSegment = Encoding.ASCII.GetBytes(message.ToString()),
                // Increases the chance of a message getting through, does not exclude the possibility of duplicates
                QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
            });

            Thread.Sleep(10000);

        } while (true);
    }
}
