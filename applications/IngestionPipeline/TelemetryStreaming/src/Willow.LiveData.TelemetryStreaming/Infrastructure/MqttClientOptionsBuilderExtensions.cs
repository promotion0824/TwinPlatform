namespace Willow.LiveData.TelemetryStreaming.Infrastructure;

using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using MQTTnet.Client;
using Willow.LiveData.TelemetryStreaming.Models;

/// <summary>
/// Extensions for the <see cref="MqttClientOptionsBuilder"/> class.
/// </summary>
internal static class MqttClientOptionsBuilderExtensions
{
    /// <summary>
    /// Applies JWT-based authentication to the options.
    /// </summary>
    /// <param name="builder">The <see cref="MqttClientOptionsBuilder"/> object that this method extends.</param>
    /// <returns>The <see cref="MqttClientOptionsBuilder"/> so that calls can be chained.</returns>
    public static MqttClientOptionsBuilder WithJwtAuth(this MqttClientOptionsBuilder builder) =>
        builder.WithAuthentication("OAUTH2-JWT", TokenProvider.GetToken());

    /// <summary>
    /// Applies client certificate-based authentication to the options.
    /// </summary>
    /// <remarks>
    /// The <paramref name="config"/> is used to retrieve the certificate from Key Vault.
    /// </remarks>
    /// <param name="builder">The <see cref="MqttClientOptionsBuilder"/> object that this method extends.</param>
    /// <param name="credential">A token credential to access Key Vault.</param>
    /// <param name="config">The configuration to obtain the certificate.</param>
    /// <returns>The <see cref="MqttClientOptionsBuilder"/> so that calls can be chained.</returns>
    public static MqttClientOptionsBuilder WithClientCertAuth(this MqttClientOptionsBuilder builder, TokenCredential credential, MqttConfig config)
    {
        CertificateClient certificateClient = new(config.CertificateAuthentication!.KeyVault, credential);

        var response = certificateClient.DownloadCertificate(config.CertificateAuthentication!.CertificateName);

        return builder
            .WithCredentials(config.ClientId)
            .WithTlsOptions(t =>
            {
                t.WithClientCertificates(new List<X509Certificate2>()
                {
                    response.Value,
                });
            });
    }
}
