using Willow.IoTService.Deployment.DataAccess.PortService;

namespace Willow.IoTService.Deployment.DataAccess.DependencyInjection;

public class SimplerUserInfoService : IUserInfoService
{
    private readonly string _name;

    public SimplerUserInfoService(string name)
    {
        _name = name;
    }

    public string GetUserName()
    {
        return _name;
    }

    public string GetUserId()
    {
        return _name;
    }
}
