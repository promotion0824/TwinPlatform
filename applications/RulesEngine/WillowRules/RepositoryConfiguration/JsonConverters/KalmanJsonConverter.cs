using System;
using Newtonsoft.Json;
using WillowRules.Filters;

namespace WillowRules.RepositoryConfiguration;

/// <summary>
/// Converts <see cref="Kalman"/> to/from JSON for serialization
/// </summary>
public class KalmanJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Kalman);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var result = new Kalman();

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
					case nameof(Kalman.ErrMeasure):
						{
							result.ErrMeasure = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(Kalman.ErrEstimate):
						{
							result.ErrEstimate = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(Kalman.Q):
						{
							result.Q = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(Kalman.CurrentEstimate):
						{
							result.CurrentEstimate = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(Kalman.LastEstimate):
						{
							result.LastEstimate = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					case nameof(Kalman.KalmanGain):
						{
							result.KalmanGain = reader.ReadAsDouble().GetValueOrDefault();
							break;
						}
					default:
						{
							//break out for these
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
		var state = (Kalman)value!;

		writer.WriteStartObject();

		if (state.ErrMeasure != 0)
		{
			writer.WritePropertyName(nameof(Kalman.ErrMeasure));
			writer.WriteValue(state.ErrMeasure);
		}

		if (state.ErrEstimate != 0)
		{
			writer.WritePropertyName(nameof(Kalman.ErrEstimate));
			writer.WriteValue(state.ErrEstimate);
		}

		if (state.Q != 0)
		{
			writer.WritePropertyName(nameof(Kalman.Q));
			writer.WriteValue(state.Q);
		}

		if (state.CurrentEstimate != 0)
		{
			writer.WritePropertyName(nameof(Kalman.CurrentEstimate));
			writer.WriteValue(state.CurrentEstimate);
		}

		if (state.LastEstimate != 0)
		{
			writer.WritePropertyName(nameof(Kalman.LastEstimate));
			writer.WriteValue(state.LastEstimate);
		}

		if (state.KalmanGain != 0)
		{
			writer.WritePropertyName(nameof(Kalman.KalmanGain));
			writer.WriteValue(state.KalmanGain);
		}

		writer.WriteEndObject();
	}
}
