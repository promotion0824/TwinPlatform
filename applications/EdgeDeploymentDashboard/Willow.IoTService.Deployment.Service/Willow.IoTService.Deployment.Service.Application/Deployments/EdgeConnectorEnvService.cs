namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Security.KeyVault.Keys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class EdgeConnectorEnvService(
    ILogger<EdgeConnectorEnvService> logger,
    IConfiguration configuration,
    IKeyVaultService keyVaultService,
    IServiceBusAdminService serviceBusAdminService,
    TokenCredential tokenCredential)
    : IEdgeConnectorEnvService
{
    private const string CommandAndControlSigningKey = "command-and-control-signing-key";
    private const string EnableActiveControl = "EnableActiveControl";

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetEdgeConnectorEnvs(
        string connectorId,
        string moduleName,
        string? environment,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Get edge connector envs");

        var instrumentationKey = configuration["ApplicationInsights:ConnectionString"]?.Split(";").FirstOrDefault() ?? string.Empty;
        var key = instrumentationKey.Split("=").LastOrDefault() ?? string.Empty;
        var authConnectorId = connectorId.Replace("-", string.Empty).ToLowerInvariant();
        var connectorPasswordKey = $"AuthPassword--{authConnectorId}";

        var envs = new List<string>
        {
            $"APPINSIGHTS_INSTRUMENTATIONKEY={key}",
            $"ApplicationInsights__ConnectionString={configuration["ApplicationInsights:ConnectionString"]}",
            $"AuthorizationOptions__Username={authConnectorId}@connector.willowinc.com",
            $"AuthorizationOptions__ClientId={configuration["Auth0:ClientId"]}",
            $"AuthorizationOptions__TokenEndpoint={configuration["Auth0:TokenEndpoint"]}",
            $"ConnectorOptions__ConnectorId={connectorId}",
            $"ConnectorOptions__ModuleName={moduleName}",
            $"LivedataXlClientOptions__ApiUrl={configuration["ConnectorXlUrl"]}",
            $"PortalXlClientOptions__ApiUrl={configuration["PortalXlUrl"]}",

            //WillowContext
            $"WillowContext__EnvironmentConfiguration__ShortName={configuration["WillowContext:EnvironmentConfiguration:ShortName"]}",
            $"WillowContext__RegionConfiguration__ShortName={configuration["WillowContext:RegionConfiguration:ShortName"]}",
            $"WillowContext__StampConfiguration__Name={configuration["WillowContext:StampConfiguration:Name"]}",
            $"WillowContext__CustomerInstanceConfiguration__CustomerInstanceName={configuration["WillowContext:CustomerInstanceConfiguration:CustomerInstanceName"]}",

            //Secrets from keyvault
            "AuthorizationOptions__Password=" + await keyVaultService.GetSecretFromKeyVault(connectorPasswordKey, cancellationToken),
            "AuthorizationOptions__ClientSecret=" + await keyVaultService.GetSecretFromKeyVault("Auth0--ClientSecret", cancellationToken),
            "ScannerResultBlobStorageOptions__ConnectionString=" + await keyVaultService.GetSecretFromKeyVault("StorageConnectionString", cancellationToken),
        };

        try
        {
            var moduleEnvVariable = JsonSerializer.Deserialize<Dictionary<string, string>>(environment ?? "{}");
            if (moduleEnvVariable?.ContainsKey(EnableActiveControl) == true &&
                Convert.ToBoolean(moduleEnvVariable[EnableActiveControl]))
            {
                var serviceBusConfig =
                    await serviceBusAdminService.GetOrCreateServiceBusConnectionConfigAsync(connectorId);
                if (serviceBusConfig is not null)
                {
                    envs.Add($"CommandAndControl__ServiceBusHost={serviceBusConfig.ServiceBusHostAddress}");
                    envs.Add($"CommandAndControl__ListenTopic={serviceBusConfig.ListenTopic}");
                    envs.Add(
                        $"CommandAndControl__ListenTopicConnectionString={serviceBusConfig.ListenConnectionString}");
                    envs.Add($"CommandAndControl__SendTopic={serviceBusConfig.SendTopic}");
                    envs.Add($"CommandAndControl__SendTopicConnectionString={serviceBusConfig.SendConnectionString}");
                }

                var keyVaultUrlString = configuration.GetValue<string>("AppSecretsUrl");
                var keyVaultUrl = new Uri(keyVaultUrlString!);
                var keyClient = new KeyClient(keyVaultUrl, tokenCredential);

                var signingKey = await keyClient.GetKeyAsync(CommandAndControlSigningKey, cancellationToken: cancellationToken);
                var rsa = signingKey.Value.Key.ToRSA();
                var publicKey = rsa.ExportRSAPublicKeyPem();
                var base64PublicKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKey));
                envs.Add($"CommandAndControl__SignatureVerificationKey={base64PublicKey}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get service bus connection config");
        }

        return envs;
    }
}
