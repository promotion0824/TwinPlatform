using System.Text.Json;
using System.Text.Json.Serialization;

namespace Willow.AzureDigitalTwins.SDK.JsonConverters.Converters
{
	internal class JsonDocumentJsonConverter : JsonConverter<JsonDocument>
	{
		public override JsonDocument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return JsonDocument.ParseValue(ref reader);
		}

		public override void Write(
			Utf8JsonWriter writer,
			JsonDocument JDoc,
			JsonSerializerOptions options) => JDoc.WriteTo(writer);
	}
}
