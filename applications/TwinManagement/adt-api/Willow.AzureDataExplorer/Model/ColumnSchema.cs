using System.Text.Json.Serialization;
using Willow.AzureDataExplorer.Converters;

namespace Willow.AzureDataExplorer.Model;

public class ColumnSchema
{
    public string? Name { get; set; }

    [JsonConverter(typeof(ColumnTypeConverter))]
    public ColumnType Type { get; set; }
}
