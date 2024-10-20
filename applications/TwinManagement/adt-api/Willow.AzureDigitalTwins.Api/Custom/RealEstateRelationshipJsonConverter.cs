using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Willow.AzureDigitalTwins.Api.Custom
{
	public class RealEstateRelationshipJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(RealEstateRelationship).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// Load JObject from stream
			JObject jObject = JObject.Load(reader);
			var target = new RealEstateRelationship();

			// Populate the object properties
			serializer.Populate(jObject.CreateReader(), target);

			target.CustomProperties = target.CustomProperties.AsEnumerable().ToDictionary(
				kv => kv.Key,
				kv => kv.Value switch
				{
					JObject element => ToCollection(element),
					_ => kv.Value
				});

			return target;
		}

		public object ToCollection(object element)
		{
			if (element is JObject jo)
			{
				return jo.ToObject<IDictionary<string, object>>().ToDictionary(k => k.Key, v => ToCollection(v.Value));
			}
			if (element is JArray ja)
			{
				return ja.ToObject<List<object>>().Select(ToCollection).ToList();
			}
			return element;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
