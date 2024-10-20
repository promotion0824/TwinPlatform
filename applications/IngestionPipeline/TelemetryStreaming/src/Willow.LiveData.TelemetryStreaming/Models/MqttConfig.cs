namespace Willow.LiveData.TelemetryStreaming.Models;

/// <summary>
/// Supported MQTT authentication methods.
/// </summary>
internal enum AuthenticationMethod
{
    /// <summary>
    /// JWT.
    /// </summary>
    Jwt = 0,

    /// <summary>
    /// Client Certificate.
    /// </summary>
    ClientCertificate = 1,
}

/// <summary>
/// Configuration for MQTT.
/// </summary>
internal record MqttConfig
{
    /// <summary>
    /// Gets the fully qualified domain name of the MQTT server.
    /// </summary>
    public required string Server { get; init; }

    /// <summary>
    /// Gets the port of the MQTT server.
    /// </summary>
    public int Port { get; init; } = 8833;

    /// <summary>
    /// Gets the client ID to use when connecting to the MQTT server.
    /// </summary>
    public string ClientId { get; init; } = "willow";

    /// <summary>
    /// Gets the maximum time to keep the TCP connection alive .
    /// </summary>
    /// <remarks>
    /// Default is 20 minutes in expectation of receiving telemetry every 15 minutes.
    /// </remarks>
    public int KeepAlivePeriodMinutes { get; init; } = 20;

    /// <summary>
    /// Gets the expiry time for the MQTT session.
    /// </summary>
    public int SessionExpiryIntervalMinutes { get; init; } = 60;

    /// <summary>
    /// Gets the authentication method to use when connecting to the MQTT server.
    /// </summary>
    public AuthenticationMethod AuthenticationMethod { get; init; }

    /// <summary>
    /// Gets the configuration for obtaining a client certificate.
    /// </summary>
    /// <remarks>
    /// Required if <see cref="AuthenticationMethod"/> is set to <see cref="AuthenticationMethod.ClientCertificate"/>.
    /// </remarks>
    public CertificateAuthenticationConfig? CertificateAuthentication { get; init; }
}

/// <summary>
/// Configuration for obtaining a client certificate.
/// </summary>
internal record CertificateAuthenticationConfig
{
    /// <summary>
    /// Gets the URL of the Key Vault instance.
    /// </summary>
    public required Uri KeyVault { get; init; }

    /// <summary>
    /// Gets the name of the certificate in Key Vault.
    /// </summary>
    public required string CertificateName { get; init; }
}
