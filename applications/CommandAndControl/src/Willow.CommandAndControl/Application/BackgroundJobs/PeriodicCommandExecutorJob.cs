namespace Willow.CommandAndControl.Application.BackgroundJobs;

/// <summary>
/// Background Service for managing the command.
/// </summary>
internal class PeriodicCommandExecutorJob : BackgroundService
{
    private readonly ILogger<PeriodicCommandExecutorJob> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IServiceProvider serviceProvider;
    private readonly HealthCheckCommandExecutor healthCheckCommandExecutor;
    private readonly TimeSpan period;
    private readonly bool isEnabled;
    private readonly bool useInhouseConnector;

    public PeriodicCommandExecutorJob(ILogger<PeriodicCommandExecutorJob> logger,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        HealthCheckCommandExecutor healthCheckCommandExecutor)
    {
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        this.serviceProvider = serviceProvider;
        this.healthCheckCommandExecutor = healthCheckCommandExecutor ?? throw new ArgumentNullException(nameof(healthCheckCommandExecutor));
        isEnabled = configuration.GetValue("BackgroundService:IsEnabled", true);
        useInhouseConnector = configuration.GetValue("UseInhouseConnector", true);
        period = TimeSpan.FromSeconds(configuration.GetValue("BackgroundService:TimePeriodInSeconds", 30));

        this.healthCheckCommandExecutor.Current = HealthCheckCommandExecutor.Starting;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(period);
        while (!cancellationToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                if (isEnabled)
                {
                    healthCheckCommandExecutor.Current = HealthCheckCommandExecutor.Healthy;
                    await using (var asyncScope = serviceScopeFactory.CreateAsyncScope())
                    {
                        var dbContext = asyncScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                        var activityLogger = asyncScope.ServiceProvider.GetRequiredService<IActivityLogger>();
                        var commandsForExecution = await dbContext.ResolvedCommands
                            .Include(x => x.RequestedCommand)
                            .Where(x =>
                                x.Status == ResolvedCommandStatus.Scheduled &&
                                x.StartTime < DateTimeOffset.UtcNow)
                            .ToListAsync(cancellationToken);
                        foreach (var resolvedCommand in commandsForExecution)
                        {
                            resolvedCommand.Status = ResolvedCommandStatus.Executing;
                            await dbContext.SaveChangesAsync(cancellationToken);
                            try
                            {
                                if (useInhouseConnector)
                                {
                                    var willowConnectorCommandSender = asyncScope.ServiceProvider.GetRequiredService<IWillowConnectorCommandSender>();
                                    await willowConnectorCommandSender.SendSetValueCommandAsync(resolvedCommand.Id.ToString(),
                                        resolvedCommand.RequestedCommand.ConnectorId,
                                        resolvedCommand.RequestedCommand.ExternalId,
                                        resolvedCommand.RequestedCommand.Value);
                                }
                                else
                                {
                                    var result = await ExecuteMappedBasedConnectorsCommand(resolvedCommand.RequestedCommand.ExternalId, resolvedCommand.RequestedCommand.Value);
                                    if (result is not null && result.StatusCode == HttpStatusCode.OK)
                                    {
                                        resolvedCommand.Status = ResolvedCommandStatus.Executed;
                                        await activityLogger.LogAsync(ActivityType.MessageReceivedSuccess, resolvedCommand, cancellationToken);
                                        await dbContext.SaveChangesAsync(cancellationToken);
                                    }
                                    else
                                    {
                                        resolvedCommand.Status = ResolvedCommandStatus.Failed;
                                        await activityLogger.LogAsync(ActivityType.MessageReceivedFailed, resolvedCommand, cancellationToken);
                                        await dbContext.SaveChangesAsync(cancellationToken);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                resolvedCommand.Status = ResolvedCommandStatus.Failed;
                                await activityLogger.LogAsync(ActivityType.MessageReceivedFailed, resolvedCommand, cancellationToken);
                                await dbContext.SaveChangesAsync(cancellationToken);
                                logger.LogError(ex, "Error executing command");
                            }
                            finally
                            {
                                //TODO: Save failure result into MappedRequestLog
                            }
                        }
                    }
                }
                else
                {
                    healthCheckCommandExecutor.Current = HealthCheckCommandExecutor.NotEnabled;
                    logger.LogInformation("Skipped executing jobs");
                }
            }
            catch (Exception ex)
            {
                healthCheckCommandExecutor.Current = HealthCheckCommandExecutor.FailedToExecuteCommand;
                logger.LogInformation(ex, "Failed the executor job!");
            }
        }
    }

    private async Task<SetValueResponse> ExecuteMappedBasedConnectorsCommand(string externalId, double value)
    {
        var mappedGatewayService = serviceProvider.GetRequiredService<IMappedGatewayService>();
        return await mappedGatewayService.SendSetValueCommandAsync(externalId, value);
    }
}
