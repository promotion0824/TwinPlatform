using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using Willow.Model.Adt;
using Azure.DigitalTwins.Core;


namespace Willow.AzureDigitalTwins.SDK.JsonConverters.Converters
{

    /// <summary>
    /// JSON Converter for NestedTwins 
    /// </summary>
    /// <remarks>
    /// This custom JsonConverter is needed to properly deserialize Children in NestedTwins
    /// </remarks>
    internal class NestedTwinsJsonConverter : JsonConverter<NestedTwin>
    {
        public override NestedTwin Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = DeserializeTreeTwin(doc.RootElement, options);
                return root;
            }
        }

        public override void Write(Utf8JsonWriter writer, NestedTwin value, JsonSerializerOptions options)
        {
            WriteTreeTwin(writer, value, options);
        }

        private NestedTwin DeserializeTreeTwin(JsonElement element, JsonSerializerOptions options)
        {
            var basicTwin = JsonSerializer.Deserialize<BasicDigitalTwin>(
            element.GetProperty("twin").GetRawText()); 

            var children = new List<NestedTwin>();

            if (element.TryGetProperty("children", out var childrenElement))
            {
                foreach (var childElement in childrenElement.EnumerateArray())
                {
                    var child = DeserializeTreeTwin(childElement, options);
                    children.Add(child);
                }
            }

           
            return new NestedTwin(basicTwin!, children);
        }

        private void WriteTreeTwin(Utf8JsonWriter writer, NestedTwin value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("twin");
            JsonSerializer.Serialize(writer, value.Twin);

            if (value.Children != null && value.Children.Count > 0)
            {
                writer.WritePropertyName("children");
                writer.WriteStartArray();

                foreach (var child in value.Children)
                {
                    WriteTreeTwin(writer, child, options);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
