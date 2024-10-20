namespace Willow.AzureDataExplorer.Builders;

public static class AdxConstants
{
    public const string RelationshipsFunctionName = "ActiveRelationships";
    public const string TwinsFunctionName = "ActiveTwins";
    public const string ModelIdColumnName = "ModelId";
    public const string SourceIdColumnName = "SourceId";
    public const string TargetIdColumnName = "TargetId";

    public const string TwinsTable = "Twins";
    public const string RelationshipsTable = "Relationships";
    public const string ModelsTable = "Models";

    public const string exportTimeColumnAlias = "ExportTime";
    public const string lastUpdateTimeColumnAlias = "LastUpdateTime";
    public const string locationColumnAlias = "Location";
    public const string twinColumnAlias = "TwinColumn";
    public const string relationshipNameColumnAlias = "RelationshipName";
    public const string outgoingRelationshipsColumnAlias = "OutgoingRelationships";
    public const string incomingRelationshipsColumnAlias = "IncomingRelationships";
    public const string twinIdColumnName = "Id";
}
