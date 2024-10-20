using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace WillowRules.RepositoryConfiguration;

/// <summary>
/// Converts <see cref="OutputValue"/> to/from JSON for serialization
/// </summary>
public class OutputValueJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(OutputValue);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		DateTimeOffset startTime = default;
		DateTimeOffset endTime = default;
		bool isValid = false;
		bool faulted = false;
		string? invalidCategory = "";
		string? text = "";
		var variables = Array.Empty<KeyValuePair<string, object>>();

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
					case nameof(OutputValue.StartTime):
						{
							startTime = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(OutputValue.EndTime):
						{
							endTime = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(OutputValue.IsValid):
						{
							isValid = reader.ReadAsBoolean() ?? false;
							break;
						}
					case nameof(OutputValue.Faulted):
						{
							faulted = reader.ReadAsBoolean() ?? faulted;
							break;
						}
					case nameof(OutputValue.InvalidCategory):
						{
							invalidCategory = reader.ReadAsString();
							break;
						}
					case nameof(OutputValue.Text):
						{
							text = reader.ReadAsString();
							break;
						}
					case nameof(OutputValue.Variables):
						{
							reader.Read();
							variables = serializer.Deserialize<KeyValuePair<string, object>[]>(reader)!;
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

		return new OutputValue(startTime, endTime, isValid, faulted, text ?? string.Empty, invalidCategory ?? string.Empty, variables);
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		var state = (OutputValue)value!;

		writer.WriteStartObject();

		writer.WritePropertyName(nameof(OutputValue.StartTime));
		writer.WriteValue(state.StartTime);

		writer.WritePropertyName(nameof(OutputValue.EndTime));
		writer.WriteValue(state.EndTime);

		if (state.IsValid)
		{
			writer.WritePropertyName(nameof(OutputValue.IsValid));
			writer.WriteValue(state.IsValid);
		}

		if (state.Faulted)
		{
			writer.WritePropertyName(nameof(OutputValue.Faulted));
			writer.WriteValue(state.Faulted);
		}

		if (!string.IsNullOrEmpty(state.InvalidCategory))
		{
			writer.WritePropertyName(nameof(OutputValue.InvalidCategory));
			writer.WriteValue(state.InvalidCategory);
		}

		if (!string.IsNullOrEmpty(state.Text))
		{
			writer.WritePropertyName(nameof(OutputValue.Text));
			writer.WriteValue(state.Text);
		}

		if (state.Variables.Length > 0)
		{
			writer.WritePropertyName(nameof(OutputValue.Variables));
			serializer.Serialize(writer, state.Variables);
		}

		writer.WriteEndObject();
	}
}
