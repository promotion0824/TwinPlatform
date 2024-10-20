using System;
using Newtonsoft.Json;
using Willow.Rules.Model;

namespace WillowRules.RepositoryConfiguration;

/// <summary>
/// Converts <see cref="TrajectoryCompressorState"/> to/from JSON for serialization
/// </summary>
public class TrajectoryCompressorStateJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(TrajectoryCompressorState);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var result = new TrajectoryCompressorState();

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
					case nameof(TrajectoryCompressorState.startTime):
						{
							result.startTime = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.startValue):
						{
							result.startValue = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.previousTime):
						{
							result.previousTime = reader.ReadAsDateTimeOffset().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.previousValue):
						{
							result.previousValue = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.upper_slope):
						{
							result.upper_slope = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.lower_slope):
						{
							result.lower_slope = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.hasPrevious):
						{
							result.hasPrevious = reader.ReadAsBoolean().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.Sum):
						{
							result.Sum = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.SumSquare):
						{
							result.SumSquare = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.Count):
						{
							result.Count = reader.ReadAsInt32().GetValueOrDefault();
							break;
						}
					case nameof(TrajectoryCompressorState.LastDelta):
						{
							result.LastDelta = reader.ReadAsDouble().GetValueOrDefault();
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

		return result;
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		var state = (TrajectoryCompressorState)value!;

		writer.WriteStartObject();

		writer.WritePropertyName(nameof(TrajectoryCompressorState.startTime));
		writer.WriteValue(state.startTime);

		if (state.startValue != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.startValue));
			writer.WriteValue(state.startValue);
		}

		writer.WritePropertyName(nameof(TrajectoryCompressorState.previousTime));
		writer.WriteValue(state.previousTime);

		if (state.previousValue != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.previousValue));
			writer.WriteValue(state.previousValue);
		}

		if (state.upper_slope != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.upper_slope));
			writer.WriteValue(state.upper_slope);
		}

		if (state.lower_slope != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.lower_slope));
			writer.WriteValue(state.lower_slope);
		}

		if (state.hasPrevious == true)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.hasPrevious));
			writer.WriteValue(state.hasPrevious);
		}

		if (state.Sum != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.Sum));
			writer.WriteValue(state.Sum);
		}

		if (state.SumSquare != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.SumSquare));
			writer.WriteValue(state.SumSquare);
		}

		if (state.Count != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.Count));
			writer.WriteValue(state.Count);
		}

		if (state.LastDelta != 0)
		{
			writer.WritePropertyName(nameof(TrajectoryCompressorState.LastDelta));
			writer.WriteValue(state.LastDelta);
		}

		writer.WriteEndObject();
	}
}
