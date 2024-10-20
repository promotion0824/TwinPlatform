using System;
using Newtonsoft.Json;
using Willow.Rules.Model;

namespace WillowRules.RepositoryConfiguration;

/// <summary>
/// Converts <see cref="TimedValue"/> to/from JSON for serialization
/// </summary>
public class TimedValueJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(TimedValue);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		DateTimeOffset timestamp = default;
		double? valueDouble = null;
		bool? valueBool = null;
		string valueString = string.Empty;

		while (reader.Read())
		{
			if (reader.TokenType == JsonToken.EndObject)
			{
				break;
			}

			if (reader.TokenType == JsonToken.PropertyName)
			{
				var property = reader.Value!.ToString()!;

				switch (property)
				{
					case nameof(TimedValue.Timestamp):
						{
							timestamp = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(TimedValue.ValueDouble):
						{
							valueDouble = reader.ReadAsDouble();
							break;
						}
					case nameof(TimedValue.ValueBool):
						{
							valueBool = reader.ReadAsBoolean();
							break;
						}
					case nameof(TimedValue.ValueText):
						{
							valueString = reader.ReadAsString() ?? string.Empty;
							break;
						}
					default:
						{
							reader.Read();
							break;
						}
				}
			}
		}

		if (valueBool.HasValue)
		{
			return new TimedValue(timestamp, valueBool.Value);
		}

		if (!string.IsNullOrEmpty(valueString))
		{
			return new TimedValue(timestamp, valueDouble ?? 0, valueString);
		}

		return new TimedValue(timestamp, valueDouble ?? 0);//zero double aren't written if no bool or string
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		var state = (TimedValue)value!;

		writer.WriteStartObject();

		writer.WritePropertyName(nameof(TimedValue.Timestamp));
		writer.WriteValue(state.Timestamp);
				
		if (state.ValueDouble.HasValue)
		{
			//value double can be null but remove 0, when deserialize we will assume if bool and string is null we will populate double value with 0
			if (state.ValueDouble.Value == 0 && !state.ValueBool.HasValue)
			{
				//dont write
			}
			else
			{
				writer.WritePropertyName(nameof(TimedValue.ValueDouble));
				writer.WriteValue(Math.Round(state.ValueDouble!.Value, 2));
			}
		}

		if (state.ValueBool.HasValue)
		{
			writer.WritePropertyName(nameof(TimedValue.ValueBool));
			writer.WriteValue(state.ValueBool);
		}

		if (!string.IsNullOrEmpty(state.ValueText))
		{
			writer.WritePropertyName(nameof(TimedValue.ValueText));
			writer.WriteValue(state.ValueText);
		}

		writer.WriteEndObject();
	}
}
