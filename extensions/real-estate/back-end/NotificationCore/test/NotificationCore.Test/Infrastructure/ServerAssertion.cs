

namespace NotificationCore.Test.Infrastructure;

public class ServerAssertion
{
    public IServiceProvider MainServices { get; }

    public ServerAssertion(IServiceProvider mainServices)
    {
        MainServices = mainServices;
    }

}
