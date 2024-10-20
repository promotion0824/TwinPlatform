namespace Connector.XL.Infrastructure.MultiRegion;

internal interface IMachineToMachineTokenAgent
{
    string GetToken(string regionId);
}
