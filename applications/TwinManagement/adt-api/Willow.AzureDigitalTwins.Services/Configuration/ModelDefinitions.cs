namespace Willow.AzureDigitalTwins.Services.Configuration;

public static class ModelDefinitions
{
    public const string ModelPrefix = "dtmi:com:willowinc:";
    public const string ModelVersionNumber = "1";

    public const string Documents = ModelPrefix + "Document;" + ModelVersionNumber;
    public const string Capability = ModelPrefix + "Capability;" + ModelVersionNumber;
}
