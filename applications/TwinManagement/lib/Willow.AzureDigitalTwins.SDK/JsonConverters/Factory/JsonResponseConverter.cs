using System.Text.Json;
using System.Text.Json.Serialization;
using Willow.AzureDigitalTwins.SDK.JsonConverters.Converters;

namespace Willow.AzureDigitalTwins.SDK.JsonConverters.Factory
{
	internal class JsonResponseConverter : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{

			if (typeToConvert == typeof(JsonDocument))
				return true;

			return false;
		}

		public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (typeToConvert == typeof(JsonDocument))
				return new JsonDocumentJsonConverter();

			return null;
		}
	}
}
