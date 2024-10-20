using System.Security.Cryptography.X509Certificates;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using static System.Console;
using static System.ConsoleColor;
using static Common.ConsoleHelper;

namespace Common;

public class MqttClientFactory
{
    public static IManagedMqttClient CreateClient()
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateManagedMqttClient();

        // Runs on disconnection
        mqttClient.DisconnectedAsync += e =>
        {
            ClearNotifications();
            ForegroundColor = Yellow;
            WriteLine("Disconnected");
            WriteLine(e.Reason);
            WriteLine(e.ReasonString);
            WriteLine(e.Exception?.Message);
            ForegroundColor = Gray;

            return Task.CompletedTask;
        };

        mqttClient.ConnectedAsync += e =>
        {
            ForegroundColor = Yellow;
            WriteLine(e.ConnectResult.ReasonString);
            ForegroundColor = Gray;

            if (e.ConnectResult.ResultCode == 0)
            {
                ForegroundColor = Green;
                WriteLine("Connected");
            }
            else
            {
                ForegroundColor = Red;
                WriteLine("Connection failed");
            }

            return Task.CompletedTask;
        };

        return mqttClient;
    }

    /// <summary>
    /// Creates the MQTT connection options.
    /// </summary>
    /// <param name="clientId">The ID of the client. It must match the certificate subject.</param>
    /// <param name="certificateName">The path to the client certificate.</param>
    /// <returns>MQTT connection options.</returns>
    public static ManagedMqttClientOptions CreateOptions(string clientId, string certificateName, string passphrase, string mqttUrl)
    {
        var cert = new X509Certificate2(certificateName, passphrase);

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttUrl, 8883)
            .WithClientId(clientId) // client ID must match the certificate subject name
            .WithCleanStart(false) // Required for persistent sessions
            .WithSessionExpiryInterval(3600) // A non-zero value is required for persistent sessions
            .WithCredentials(clientId, String.Empty) // Required. Username must match client ID.
            .WithWillRetain(false)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500) // MQTT version (5 is latest)
            .WithTlsOptions(t =>
            {
                t.UseTls()
                 .WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12)
                 .WithClientCertificates(new List<X509Certificate2>()
                 {
                     cert
                 });
            })
            .Build();

        return new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(mqttClientOptions)
            .Build();
    }

    private static void SetCursorLine(int? inputCursorLine)
    {
        if (inputCursorLine == null) return;

        SetCursorPosition(2, inputCursorLine.Value);
    }
}
