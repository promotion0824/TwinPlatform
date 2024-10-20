namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Azure.Core;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Service Bus Admin Service.
/// </summary>
public class ServiceBusAdminService(IConfiguration configuration, TokenCredential tokenCredential) : IServiceBusAdminService
{
    private const string CommandsTopicName = "cnc-commands";
    private const string CommandStatusTopicName = "cnc-command-status";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusAdminService"/> class.
    /// </summary>
    /// <param name="connectorId">ConnectorId.</param>
    /// <returns>cnc-commands topic listen connection config.</returns>
    public async Task<ServiceBusConnectionConfig?> GetOrCreateServiceBusConnectionConfigAsync(string connectorId)
    {
        var host = configuration["AzureServiceBus:HostAddress"];

        var client = new ServiceBusAdministrationClient(host, tokenCredential);

        var commandsTopic = await client.GetTopicAsync(CommandsTopicName);
        var commandStatusTopic = await client.GetTopicAsync(CommandStatusTopicName);

        if (!(await client.TopicExistsAsync(CommandsTopicName)) || !(await client.TopicExistsAsync(CommandStatusTopicName)))
        {
            return null;
        }

        if (!(await client.SubscriptionExistsAsync(CommandsTopicName, connectorId)))
        {
            var createSubscriptionOptions = new CreateSubscriptionOptions(CommandsTopicName, connectorId);

            // Add a rule with a filter for the connectorId
            var ruleOptions = new CreateRuleOptions("FilterByConnectorId", new SqlRuleFilter($"ConnectorId='{connectorId}'"));

            await client.CreateSubscriptionAsync(createSubscriptionOptions);
            if (await client.RuleExistsAsync(CommandsTopicName, connectorId, "$Default"))
            {
                await client.DeleteRuleAsync(CommandsTopicName, connectorId, "$Default");
            }

            await client.CreateRuleAsync(CommandsTopicName, connectorId, ruleOptions);
        }

        // For every connector we have separate send/listen access policy to ensure security
        if (!commandsTopic.Value.AuthorizationRules.Any(x => x.KeyName == connectorId))
        {
            commandsTopic.Value.AuthorizationRules.Add(new SharedAccessAuthorizationRule(connectorId, [AccessRights.Listen]));
            await client.UpdateTopicAsync(commandsTopic.Value);
        }

        if (!commandStatusTopic.Value.AuthorizationRules.Any(x => x.KeyName == connectorId))
        {
            commandStatusTopic.Value.AuthorizationRules.Add(new SharedAccessAuthorizationRule(connectorId, [AccessRights.Send]));
            await client.UpdateTopicAsync(commandStatusTopic.Value);
        }

        var commandsRule = commandsTopic.Value.AuthorizationRules.First(x => x.KeyName == connectorId) as SharedAccessAuthorizationRule;
        var commandStatusRule = commandStatusTopic.Value.AuthorizationRules.First(x => x.KeyName == connectorId) as SharedAccessAuthorizationRule;
        var commandsTopicConnectionString = GetConnectionString(host!, connectorId, commandsRule!.PrimaryKey, CommandsTopicName);
        var commandStatusTopicConnectionString = GetConnectionString(host!, connectorId, commandStatusRule!.PrimaryKey, CommandStatusTopicName);

        return new(host!, CommandsTopicName, CommandStatusTopicName, commandsTopicConnectionString, commandStatusTopicConnectionString);
    }

    private string GetConnectionString(string host, string connectorId, string key, string topic) =>
        $"Endpoint={host};SharedAccessKeyName={connectorId};SharedAccessKey={key};EntityPath={topic}";
}

/// <summary>
/// Service Bus Configuration.
/// </summary>
/// <param name="ServiceBusHostAddress">Service Bus Host Address.</param>
/// <param name="ListenTopic">Listen Topic Name.</param>
/// <param name="SendTopic">Send Topic Name.</param>
/// <param name="ListenConnectionString">Listen Connection String.</param>
/// <param name="SendConnectionString">Send Connection String.</param>
public record ServiceBusConnectionConfig(string ServiceBusHostAddress, string ListenTopic, string SendTopic, string ListenConnectionString, string SendConnectionString);
