using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public static class AdtConstants
    {
        public const int MaxDisplayPriority = 2;

        public const string AssetModel = "Asset";
        public const string SpaceModel = "Space";
        public const string BuildingComponentModel = "BuildingComponent";
        public const string StructureModel = "Structure";

        public static readonly string DtmiWillowPrefix = "dtmi:com:willowinc:";
        public static readonly string WarrantyModelId = DtmiWillowPrefix + "Warranty;1";
        public static readonly string SiteModelId = DtmiWillowPrefix + "Building;1";
        public static readonly string FloorModelId = DtmiWillowPrefix + "Level;1";

        public const string CustomPropertiesProperty = "Custom Properties";
        public const string MetadataProperty = "$metadata";

        // Default models to be returned
        public static List<string> DefaultAdtModels =>
            new List<string> { AssetModel, SpaceModel, BuildingComponentModel, StructureModel };

        internal const int MaxLocationHops = 5;

        public static class RelationshipNames
        {
            internal const string LocatedIn = "locatedIn";
            internal const string IsPartOf = "isPartOf";
            internal static readonly string[] Location = new[] { LocatedIn, IsPartOf };

            internal const string HasDocument = "hasDocument";
            internal const string IsCapabilityOf = "isCapabilityOf";
            internal const string CommissionedBy = "commissionedBy";
            internal const string InstalledBy = "installedBy";
            internal const string ServicedBy = "servicedBy";
            internal const string ServiceResponsibility = "serviceResponsibility";
            internal const string ProducedBy = "producedBy";
        }
    }
}
