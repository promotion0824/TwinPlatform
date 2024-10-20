using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Options;

public interface IThirdPartyConsumerOptions
{
    List<ThirdPartyConsumer> ThirdPartyConsumers { get; set; }
}