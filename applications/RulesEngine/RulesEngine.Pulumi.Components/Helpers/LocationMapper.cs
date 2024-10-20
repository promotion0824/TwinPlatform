namespace RulesEngine.Pulumi.Components.Helpers
{
    public static class LocationMapper
    {

        // Matches the existing mapping names from The terraform modules
        public static string AzureRegionToShort(string location) => location.ToLower() switch
        {
            "australiaeast" => "aue",
            "eastus2" => "eu2",
            "westeurope" => "weu",
            _ => throw new ArgumentOutOfRangeException(nameof(location), location, null)
        };
    }
}
