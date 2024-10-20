namespace Willow.IoTService.Deployment.Service.Application.BackgroundJobs;

using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Deployment.DataAccess.Db;

/// <summary>
/// Background job to listen to the Azure Service Bus queue for alert resolver messages.
/// </summary>
public class AlertResolverBackgroundJob : BackgroundService
{
    private const string QueueName = "alert-resolver";
    private static readonly Regex ModuleTypeRegex = new("(chipkinbacnet|bacnet|modbus|opcua)", RegexOptions.IgnoreCase);
    private const int MinLastRestartIntervalHour = 6;
    private readonly ILogger<AlertResolverBackgroundJob> logger;
    private readonly ServiceBusClient serviceBusClient;
    private ServiceBusProcessor processor;
    private readonly DeploymentDbContext dbContext;
    private readonly TokenCredential tokenCredentials;
    private readonly Dictionary<string, DateTime> lastRestartedModules = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertResolverBackgroundJob"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="dbContext">DbContext.</param>
    /// <param name="tokenCredentials">TokenCredentials.</param>
    public AlertResolverBackgroundJob(ILogger<AlertResolverBackgroundJob> logger,
        IConfiguration configuration,
        DeploymentDbContext dbContext,
        TokenCredential tokenCredentials)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.tokenCredentials = tokenCredentials;
        serviceBusClient = new ServiceBusClient(configuration["AzureServiceBus:HostAddress"],
            tokenCredentials,
            new ServiceBusClientOptions()
            {
                RetryOptions = new()
                {
                    MaxRetries = 3,
                },
            });
        processor = serviceBusClient.CreateProcessor(QueueName);
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        processor = serviceBusClient.CreateProcessor(QueueName, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += ProcessMessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync(cancellationToken);
        logger.LogInformation("Worker started and listening to Azure Service Bus...");
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker stopping...");
        await processor.StopProcessingAsync(cancellationToken);
        await processor.DisposeAsync();
        await serviceBusClient.DisposeAsync();
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        string messageBody = args.Message.Body.ToString();
        logger.LogInformation($"Received message: {messageBody}");

        try
        {
            // Deserialize the message to get the necessary details
            var body = JsonSerializer.Deserialize<AlertResolverMessage>(messageBody);

            if (body != null)
            {
                logger.LogInformation($"Receive Alert Resolve message for module: {body.ModuleName}");

                if (lastRestartedModules.TryGetValue(body.ModuleName, out var lastRestartedTime))
                {
                    var timeSinceLastRestart = DateTime.UtcNow - lastRestartedTime;
                    if (timeSinceLastRestart.TotalHours < MinLastRestartIntervalHour)
                    {
                        logger.LogInformation($"Module {body.ModuleName} was restarted less than {MinLastRestartIntervalHour} hours ago. Skipping restart.");
                        await args.CompleteMessageAsync(args.Message);
                        return;
                    }
                }

                var deployment = await dbContext.Deployments
                    .Include(x => x.Module)
                    .Include(x => x.Module.Config)
                    .FirstOrDefaultAsync(x => x.Name == body.ModuleName);

                if (deployment == null || deployment.Module.Config?.DeviceName == null)
                {
                    logger.LogError($"Module {body.ModuleName} or config not found in the database.");
                    await args.AbandonMessageAsync(args.Message);
                    return;
                }

                // Invoke the restart method on the IoT Edge device
                var methodInvocation = new CloudToDeviceMethod("RestartModule")
                {
                    ResponseTimeout = TimeSpan.FromSeconds(30),
                };

                var payload = JsonSerializer.Serialize(new
                {
                    schemaVersion = "1.0",
                    id = GetContainerModuleName(deployment.Module.ModuleType, deployment.Name),
                });

                methodInvocation.SetPayloadJson(payload);

                var iotHubHost = $"{deployment.Module.Config.IoTHubName}.azure-devices.net";
                using ServiceClient serviceClient = ServiceClient.Create(iotHubHost, tokenCredentials);
                CloudToDeviceMethodResult result =
                    await serviceClient.InvokeDeviceMethodAsync(deployment.Module.Config.DeviceName, "$edgeAgent", methodInvocation);

                logger.LogInformation($"Status: {result.Status}, Response: {result.GetPayloadAsJson()}");

                if (result.Status == 200)
                {
                    logger.LogInformation("Module restarted successfully!");
                }
                else
                {
                    logger.LogError("Failed to restart the module.");
                }

                if (!lastRestartedModules.ContainsKey(body.ModuleName))
                {
                    lastRestartedModules.Add(body.ModuleName, DateTime.UtcNow);
                }

                lastRestartedModules[body.ModuleName] = DateTime.UtcNow;
            }

            // Complete the message so it's not received again.
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error processing message: {ex.Message}");

            // Abandon the message so it can be retried later.
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private string GetContainerModuleName(string moduleType, string connectorName)
    {
        var validContainerName = Regex.Replace(connectorName, "[^a-zA-Z0-9_.-]", string.Empty);
        var match = ModuleTypeRegex.Match(moduleType);
        var moduleName = match.Value.ToUpperInvariant() switch
        {
            "CHIPKINBACNET" => "CBacnetConnectorModule",
            "BACNET" => "BacnetConnectorModule",
            "MODBUS" => "ModbusConnectorModule",
            "OPCUA" => "OpcuaConnectorModule",
            _ => throw new ArgumentException($"Invalid module type: {match.Value}"),
        };
        return $"{validContainerName}-{moduleName}";
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError($"Message handler encountered an error: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    internal record AlertResolverMessage(string ModuleName);
}
