using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Willow.AzureDigitalTwins.Api.Custom;

/// <summary>
/// JSON Converter for serializing DateOffset without the offset inthe serialzied string
/// </summary>
/// <remarks>
/// By default, ADX uses automatic type detection during ingestion.
/// If the datetime string in the JSON field has a recognizable format, ADX may automatically convert it to a datetime data type.
/// However, this behavior can sometimes lead to unwanted conversions or parsing issues.
/// To avoid this automatic type conversion (and to avoid confusion during app inmemory adx record comparison) we do the conversion upfront,
/// by removing the offset from the datetime string and convert it to standard ISO datetime string.
/// </remarks>
public class DateTimeOffsetConverterWithoutOffset : JsonConverter<DateTimeOffset>
{
    /// <summary>
    /// Deserialize logic
    /// </summary>
    /// <param name="reader">Json Reader</param>
    /// <param name="typeToConvert">Object type to convert</param>
    /// <param name="options">Json Serialization Option</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization is not supported for DateTimeOffset without offset.");
    }

    /// <summary>
    /// Serialize logic
    /// </summary>
    /// <param name="writer">Stream to write the data</param>
    /// <param name="value">target value to write</param>
    /// <param name="options">Json Serialization Option</param>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        // Format the DateTimeOffset without the offset information
        string formattedDateTime = value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

        // Write the formatted DateTimeOffset to the JSON writer
        writer.WriteStringValue(formattedDateTime);
    }
}
