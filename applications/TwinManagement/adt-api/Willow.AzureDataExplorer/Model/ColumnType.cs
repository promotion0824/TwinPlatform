using System.ComponentModel;

namespace Willow.AzureDataExplorer.Model;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Explicitly declaring this names for clarity")]
public enum ColumnType
{
    [Description("System.String")]
    String,
    [Description("System.Object")]
    Object,
    [Description("System.Int32")]
    Int,
    [Description("System.Boolean")]
    Boolean,
    [Description("System.DateTime")]
    DateTime,
    [Description("System.Guid")]
    Guid,
    [Description("System.Int64")]
    Long,
    [Description("System.Double")]
    Double,
    [Description("System.TimeSpan")]
    TimeSpan
}
