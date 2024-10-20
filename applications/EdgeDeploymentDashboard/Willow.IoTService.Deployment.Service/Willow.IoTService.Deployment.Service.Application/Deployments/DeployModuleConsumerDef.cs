namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using MassTransit;
using Microsoft.Extensions.Configuration;

/// <summary>
///     Consumer definition for DeployModuleConsumer.
/// </summary>
public class DeployModuleConsumerDef : ConsumerDefinition<DeployModuleConsumer>
{
    private const string ConfigSectionName = "QueueName";
    private const string DefaultQueueName = "deploy-module";

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeployModuleConsumerDef" /> class.
    /// </summary>
    /// <param name="configuration">Configuration class.</param>
    public DeployModuleConsumerDef(IConfiguration configuration)
    {
        this.EndpointName = configuration.GetSection(ConfigSectionName).Value ?? DefaultQueueName;
        this.ConcurrentMessageLimit = 1;
    }

    /// <summary>
    ///     Configures the MassTransit consumer.
    /// </summary>
    /// <param name="endpointConfigurator">Service bus endpoint configurator.</param>
    /// <param name="consumerConfigurator">Consumer configurator.</param>
    /// <param name="context">Context.</param>
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<DeployModuleConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.PrefetchCount = 0;
    }
}
