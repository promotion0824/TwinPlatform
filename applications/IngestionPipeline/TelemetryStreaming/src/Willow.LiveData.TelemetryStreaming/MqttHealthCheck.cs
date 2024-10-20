namespace Willow.LiveData.TelemetryStreaming;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQTTnet.Extensions.ManagedClient;

internal class MqttHealthCheck : IHealthCheck
{
    private readonly IManagedMqttClient mqttClient;
    private string? connectionError;

    public MqttHealthCheck(IManagedMqttClient mqttClient)
    {
        this.mqttClient = mqttClient;
        mqttClient.ConnectedAsync += MqttClientConnectedAsync;
        mqttClient.DisconnectedAsync += MqttClientDisconnectedAsync;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (mqttClient.IsStarted && mqttClient.IsConnected)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Connected"));
        }

        if (mqttClient.IsStarted && connectionError != null)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(connectionError));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Starting up"));
    }

    private Task MqttClientDisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs e)
    {
        connectionError = "Disconnected";

        if (e.ConnectResult != null)
        {
            connectionError = e.ConnectResult.ResultCode.ToString();
        }
        else if (e.Exception != null)
        {
            connectionError = "Exception thrown when connecting";
        }

        return Task.CompletedTask;
    }

    private Task MqttClientConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
    {
        connectionError = null;
        return Task.CompletedTask;
    }
}
