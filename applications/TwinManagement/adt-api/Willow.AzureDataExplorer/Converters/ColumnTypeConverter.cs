using System.Text.Json;
using System.Text.Json.Serialization;
using Willow.AzureDataExplorer.Helpers;
using Willow.AzureDataExplorer.Model;

namespace Willow.AzureDataExplorer.Converters;

sealed internal class ColumnTypeConverter : JsonConverter<ColumnType>
{
    public override ColumnType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        var enumValues = Enum.GetValues<ColumnType>().ToDictionary(x => x.GetDescription(), x => x);

        return value != null && enumValues.TryGetValue(value, out var enumValue) ? enumValue : ColumnType.String;
    }

    public override void Write(Utf8JsonWriter writer, ColumnType value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
