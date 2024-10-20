namespace Willow.CommandAndControl.Application.Services;

using System.Security.Cryptography;
using System.Text.Json;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Keys.Cryptography;

internal class WillowConnectorCommandSender(
    IApplicationDbContext dbContext,
    IOptions<ServiceBusOptions> serviceBusOptions,
    IActivityLogger activityLogger,
    IConfiguration configuration,
    TokenCredential tokenCredential,
    ServiceBusClient serviceBusClient)
    : IWillowConnectorCommandSender
{
    private ServiceBusSender? serviceBusSender;

    public async Task SendSetValueCommandAsync(string resolvedCommandId, string connectorId, string pointId, double value)
    {
        var command = new SendCommandRequestDto
        {
            CommandId = resolvedCommandId,
            ExternalId = pointId,
            Value = value,
        };

        var preventSending = configuration.GetValue<bool>("PreventSendingCommandsToEdge");
        if (preventSending)
        {
            var resolvedCommand = await dbContext.ResolvedCommands.FirstOrDefaultAsync(x => x.Id == Guid.Parse(resolvedCommandId));
            if (resolvedCommand != null)
            {
                resolvedCommand.Status = ResolvedCommandStatus.Failed;
                var extraInfo = "This command has been blocked from executing by system settings.";
                await activityLogger.LogAsync(ActivityType.Failed, resolvedCommand, extraInfo);
                await dbContext.SaveChangesAsync();
            }

            return;
        }

        string json = JsonSerializer.Serialize(command);

        var message = new ServiceBusMessage(json);

        message.ApplicationProperties["ConnectorId"] = connectorId;
        message.ApplicationProperties["Signature"] = await SignMessageBody(json);

        if (serviceBusSender is null)
        {
            InitServiceBusSender();
        }

        await serviceBusSender!.SendMessageAsync(message);
    }

    private async Task<string> SignMessageBody(string json)
    {
        var keyVaultUri = configuration.GetValue<string>("KeyVault:Url");
        ArgumentException.ThrowIfNullOrWhiteSpace(keyVaultUri);
        var keyName = configuration.GetValue<string>("KeyVault:SigningKeyName");
        ArgumentException.ThrowIfNullOrWhiteSpace(keyName);
        var cryptoClient = new CryptographyClient(new Uri($"{keyVaultUri}/keys/{keyName}"), tokenCredential);
        byte[] messageBytes = Encoding.UTF8.GetBytes(json);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(messageBytes);
        var signResult = await cryptoClient.SignAsync(SignatureAlgorithm.RS256, hash);
        return Convert.ToBase64String(signResult.Signature);
    }

    public async Task PostCommandExecutionResultAsync(CommandExecutionResultDto result)
    {
        if (!Guid.TryParse(result.CommandId, out var commandId))
        {
            return;
        }

        var command = dbContext.ResolvedCommands.Include(x => x.RequestedCommand)
            .FirstOrDefault(x => x.Id == commandId);
        if (command is null || command.Status != ResolvedCommandStatus.Executing)
        {
            return;
        }

        command.StatusUpdatedBy = ActivityLogSources.EdgeDevice;
        var extraInfo =
            $@"
Message:{result.Message}

RequestBody:
{result.RequestBody}

ResponseBody:
{result.ResponseBody}";
        command.Status = result.StatusCode == 0 ? ResolvedCommandStatus.Executed : ResolvedCommandStatus.Failed;
        await activityLogger.LogAsync(
            result.StatusCode == 0 ? ActivityType.MessageReceivedSuccess : ActivityType.MessageReceivedFailed, command, extraInfo);
        await activityLogger.LogAsync(result.StatusCode == 0 ? ActivityType.Completed : ActivityType.Failed, command);
        await dbContext.SaveChangesAsync();
    }

    private void InitServiceBusSender()
    {
        serviceBusSender = serviceBusClient.CreateSender(serviceBusOptions.Value.SendCommandsTopic);
    }
}
