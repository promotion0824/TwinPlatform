using System;
using Newtonsoft.Json;
using Willow.Rules.Model;

namespace WillowRules.RepositoryConfiguration;

/// <summary>
/// Converts <see cref="CommandOutputValue"/> to/from JSON for serialization
/// </summary>
public class CommandOutputValueJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(CommandOutputValue);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		DateTimeOffset startTime = default;
		DateTimeOffset endTime = default;
		DateTimeOffset triggerStartTime = default;
		DateTimeOffset? triggerEndTime = null;
		bool triggered = false;
		double value = 0;

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
					case nameof(CommandOutputValue.StartTime):
						{
							startTime = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(CommandOutputValue.EndTime):
						{
							endTime = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(CommandOutputValue.TriggerStartTime):
						{
							triggerStartTime = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(CommandOutputValue.TriggerEndTime):
						{
							triggerEndTime = reader.ReadAsDateTimeOffset();
							break;
						}
					case nameof(CommandOutputValue.Triggered):
						{
							triggered = reader.ReadAsBoolean() ?? triggered;
							break;
						}
					case nameof(CommandOutputValue.Value):
						{
							value = reader.ReadAsDouble() ?? value;
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

		return new CommandOutputValue(startTime, endTime, triggerStartTime, triggerEndTime, triggered, value);
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		var state = (CommandOutputValue)value!;

		writer.WriteStartObject();

		writer.WritePropertyName(nameof(CommandOutputValue.StartTime));
		writer.WriteValue(state.StartTime);

		writer.WritePropertyName(nameof(CommandOutputValue.EndTime));
		writer.WriteValue(state.EndTime);

		writer.WritePropertyName(nameof(CommandOutputValue.TriggerStartTime));
		writer.WriteValue(state.TriggerStartTime);

		if (state.Triggered)
		{
			writer.WritePropertyName(nameof(CommandOutputValue.Triggered));
			writer.WriteValue(state.Triggered);
		}

		if (state.Value != 0)
		{
			writer.WritePropertyName(nameof(CommandOutputValue.Value));
			writer.WriteValue(state.Value);
		}

		if (state.TriggerEndTime is not null)
		{
			writer.WritePropertyName(nameof(CommandOutputValue.TriggerEndTime));
			writer.WriteValue(state.TriggerEndTime.Value);
		}

		writer.WriteEndObject();
	}
}
