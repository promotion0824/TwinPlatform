namespace Willow.IoTService.Deployment.Common;

public static class BaseModuleDeploymentHelper
{
    public static readonly string BaseModuleTypeString = "BaseModules";

    public static bool IsBaseDeployment(string moduleType)
    {
        return BaseModuleTypeString.Equals(moduleType, StringComparison.InvariantCultureIgnoreCase);
    }

    public static string GetBaseDeploymentNameFromId(Guid id)
    {
        return id.ToString("N");
    }
}
