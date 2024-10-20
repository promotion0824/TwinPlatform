namespace Willow.TopologyIngestion.Mapped
{
    /// <summary>
    /// A list of relationship types used in the topology graph.
    /// </summary>
    internal static class RelationshipType
    {
        /// <summary>
        /// The hasLocation relationship type.
        /// </summary>
        internal const string HasLocation = "hasLocation";

        /// <summary>
        /// The hasPart relationship type.
        /// </summary>
        internal const string HasPart = "hasPart";

        /// <summary>
        /// The hasPoint relationship type.
        /// </summary>
        internal const string HasPoint = "hasPoint";

        /// <summary>
        /// The hasUtilityBill relationship type.
        /// </summary>
        internal const string HasUtilityBill = "hasUtilityBill";

        /// <summary>
        /// The isProvidedBy relationship type.
        /// </summary>
        internal const string IsProvidedBy = "isProvidedBy";

        /// <summary>
        /// The isAdjacentTo relationship type.
        /// </summary>
        internal const string IsAdjacentTo = "isAdjacentTo";

        /// <summary>
        /// The isBilledTo relationship type.
        /// </summary>
        internal const string IsBilledTo = "isBilledTo";

        /// <summary>
        /// The isFedBy relationship type.
        /// </summary>
        internal const string IsFedBy = "isFedBy";

        /// <summary>
        /// The isLocationOf relationship type.
        /// </summary>
        internal const string IsLocationOf = "isLocationOf";

        /// <summary>
        /// The isPartOf relationship type.
        /// </summary>
        internal const string IsPartOf = "isPartOf";

        /// <summary>
        /// The isLocatedIn relationship type.
        /// </summary>
        internal const string IsLocatedIn = "isLocatedIn";

        /// <summary>
        /// The serves relationship type.
        /// </summary>
        internal const string Serves = "serves";

        /// <summary>
        /// The served by relationship type.
        /// </summary>
        internal const string ServedBy = "servedBy";

        /// <summary>
        /// The locatedInGridRegion relationship type.
        /// </summary>
        internal const string LocatedInGridRegion = "locatedInGridRegion";
    }
}
