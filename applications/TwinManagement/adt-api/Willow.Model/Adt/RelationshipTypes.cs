namespace Willow.Model.Adt;

public static class RelationshipTypes
{
    public const string IsPartOf = "isPartOf";
    public const string IsDocumentOf = "isDocumentOf";
    public const string HasDocument = "hasDocument";
    public const string IsCapabilityOf = "isCapabilityOf"; // Formerly "isPointOf"
    public const string LocatedIn = "locatedIn";
    public static readonly string HostedBy = "hostedBy";
    public static readonly string IncludedIn = "includedIn";

    public static IEnumerable<string> GetLocationSearchRelTypes()
    {
        return new[] { IsPartOf, LocatedIn, IncludedIn };
    }

}
