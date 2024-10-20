namespace Willow.LiveData.IoTHubAdaptor;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Allows for the deserialization of JSON data where there is a null value for a property expecting a double.
/// </summary>
internal class NullableDoubleConverter : JsonConverter<double?>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(double) || typeToConvert == typeof(double?);
    }

    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => double.TryParse(reader.GetString(), out var value) ? value : null,
            JsonTokenType.Number => reader.TryGetDouble(out var value) ? value : null,
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
