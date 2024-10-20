namespace Willow.Infrastructure.MultiRegion;

internal interface IMachineToMachineTokenAgent
{
    string GetToken(string regionId);
}
