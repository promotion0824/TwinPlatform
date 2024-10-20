using DigitalTwinCore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DigitalTwinCore.Serialization
{
    public class TwinJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Twin).IsAssignableFrom(objectType) || typeof(TwinWithRelationships).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);
            var target = new Twin();

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            if (jObject["modelId"] != null)
            {
                var modeId = jObject["modelId"].ToString();
                target.Metadata = new TwinMetadata { ModelId = modeId };
            }

            target.CustomProperties = target.CustomProperties.AsEnumerable().ToDictionary(
                kv => kv.Key,
                kv => kv.Value switch
                {
                    JObject element => TwinExtensions.ToCollection(element),
                    JArray element => TwinExtensions.ToCollection(element),
                    _ => kv.Value
                });

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
