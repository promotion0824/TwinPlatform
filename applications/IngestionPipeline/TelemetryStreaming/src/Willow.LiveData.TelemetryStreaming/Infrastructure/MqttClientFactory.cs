namespace Willow.LiveData.TelemetryStreaming.Infrastructure;

using Azure.Core;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Willow.LiveData.TelemetryStreaming.Metrics;
using Willow.LiveData.TelemetryStreaming.Models;

internal class MqttClientFactory : IDisposable
{
    private readonly ILoggerFactory loggerFactory;
    private readonly MqttConfig config;
    private readonly TokenCredential credential;
    private readonly IMetricsCollector metricsCollector;

    private readonly List<Timer> refreshTimers = [];
    private bool disposedValue;

    public MqttClientFactory(ILoggerFactory loggerFactory, IOptions<MqttConfig> config, TokenCredential credential, IMetricsCollector metricsCollector)
    {
        this.loggerFactory = loggerFactory;
        this.config = config.Value;
        this.credential = credential;
        this.metricsCollector = metricsCollector;

        if (this.config.AuthenticationMethod == AuthenticationMethod.ClientCertificate && (this.config.CertificateAuthentication == null || this.config.CertificateAuthentication.KeyVault == null ||
            string.IsNullOrEmpty(this.config.CertificateAuthentication.CertificateName)))
        {
            throw new InvalidOperationException("Authentication method is ClientCertificate but configuration is incomplete or provided!");
        }
    }

    public IManagedMqttClient CreateManagedClient()
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateManagedMqttClient();

        var logger = loggerFactory.CreateLogger<IManagedMqttClient>();

        // Runs on disconnection
        mqttClient.DisconnectedAsync += e =>
        {
            metricsCollector.TrackMqttDisconnectCount(1, null);

            if (e.ConnectResult != null)
            {
                logger.LogInformation("Disconnected from MQTT Server. Result {result}; Reason \"{reason}\"", e.ConnectResult.ResultCode, e.ConnectResult.ReasonString);

                if (e.ConnectResult.ResultCode == MqttClientConnectResultCode.NotAuthorized)
                {
                    logger.LogError("Unable to authorize with MQTT");
                }
            }
            else if (e.Exception != null)
            {
                logger.LogError(e.Exception, "Disconnected from MQTT Server. Reason \"{reason}\"", e.Exception?.Message);
            }
            else if (e is MqttClientDisconnectedEventArgs mqtteargs)
            {
                if (e.ReasonString != null)
                {
                    logger.LogError(e.Exception, $"Disconnected from MQTT Server. Reason \"{e.Reason}\" with Message \"{e.ReasonString}\"");
                }
                else
                {
                    logger.LogError(e.Exception, $"Disconnected from MQTT Server. Reason \"{e.Reason}\"");
                }
            }
            else
            {
                logger.LogInformation("Disconnected from MQTT Server");
            }

            return Task.CompletedTask;
        };

        mqttClient.ConnectedAsync += e =>
        {
            metricsCollector.TrackMqttConnectCount(1, null);
            if (e.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                logger.LogInformation("Connected to MQTT Server");
            }
            else
            {
                logger.LogError("Unable to connect to MQTT Server. Result {result}; Reason \"{reason}\"", e.ConnectResult.ResultCode, e.ConnectResult.ReasonString);
            }

            return Task.CompletedTask;
        };

        if (config.AuthenticationMethod == AuthenticationMethod.Jwt)
        {
            refreshTimers.Add(new Timer(RefreshToken, mqttClient.InternalClient, TimeSpan.FromMinutes(10).Milliseconds, TimeSpan.FromMinutes(10).Milliseconds));
        }

        mqttClient.StartAsync(CreateManagedOptions()).Wait();

        return mqttClient;
    }

    /// <summary>
    /// Creates the MQTT connection options.
    /// </summary>
    /// <returns>MQTT connection options.</returns>
    private MqttClientOptionsBuilder CreateOptionsBuilder()
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(config.Server, config.Port)
            .WithClientId(config.ClientId)
            .WithCleanStart(false) // Required for persistent sessions
            .WithKeepAlivePeriod(TimeSpan.FromMinutes(config.KeepAlivePeriodMinutes))
            .WithSessionExpiryInterval((uint)TimeSpan.FromMinutes(config.SessionExpiryIntervalMinutes).Seconds) // A non-zero value is required for persistent sessions
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithTlsOptions(t =>
            {
                t.UseTls();
            });

        return mqttClientOptions;
    }

    private MqttClientOptions CreateJwtAuthOptions() =>
        CreateOptionsBuilder().WithJwtAuth().Build();

    private MqttClientOptions CreateCertAuthOptions() =>
        CreateOptionsBuilder().WithClientCertAuth(credential, config!).Build();

    private void RefreshToken(object? state)
    {
        Console.WriteLine("Refreshing Token " + DateTime.Now.ToString("o"));
        IMqttClient mqttClient = (MqttClient)state!;
        Task.Run(async () =>
        {
            await mqttClient.SendExtendedAuthenticationExchangeDataAsync(
                new MqttExtendedAuthenticationExchangeData()
                {
                    AuthenticationData = TokenProvider.GetToken(),
                    ReasonCode = MQTTnet.Protocol.MqttAuthenticateReasonCode.ReAuthenticate,
                });
        });
    }

    private ManagedMqttClientOptions CreateManagedOptions() =>
        new ManagedMqttClientOptionsBuilder()

            //.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(config.AuthenticationMethod == AuthenticationMethod.Jwt ? CreateJwtAuthOptions() : CreateCertAuthOptions())
            .Build();

#pragma warning disable SA1124 // Do not use regions
    #region Dispose
    protected virtual void Dispose(bool disposing)
#pragma warning restore SA1124 // Do not use regions
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                refreshTimers.ForEach(t => t.Dispose());
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
