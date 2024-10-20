using Microsoft.Extensions.DependencyInjection;
using Moq;
using NotificationCore.Infrastructure.MultiRegion;

namespace NotificationCore.Test.Infrastructure;
public class ServerArrangement
{
    public IServiceProvider MainServices { get; }

    public ServerArrangement(IServiceProvider mainServices)
    {
        MainServices = mainServices;
    }

    public DependencyServiceHttpHandler GetHttpHandler(string dependencyServiceName)
    {
        var httpHandler = MainServices.GetRequiredService<Mock<HttpMessageHandler>>();
        return new DependencyServiceHttpHandler(httpHandler, $"https://{dependencyServiceName}.com");
    }

    public DependencyServiceHttpHandler GetHttpHandler(string dependencyServiceName, string regionId)
    {
        var serviceName = MultiRegionHelper.ServiceName(dependencyServiceName, regionId);
        var httpHandler = MainServices.GetRequiredService<Mock<HttpMessageHandler>>();
        return new DependencyServiceHttpHandler(httpHandler, $"https://{serviceName}.com");
    }

}
