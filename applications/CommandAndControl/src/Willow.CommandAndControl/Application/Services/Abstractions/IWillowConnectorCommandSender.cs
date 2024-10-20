namespace Willow.CommandAndControl.Application.Services.Abstractions;

internal interface IWillowConnectorCommandSender
{
    Task SendSetValueCommandAsync(string resolvedCommandId, string connectorId, string pointId, double value);

    Task PostCommandExecutionResultAsync(CommandExecutionResultDto result);
}
