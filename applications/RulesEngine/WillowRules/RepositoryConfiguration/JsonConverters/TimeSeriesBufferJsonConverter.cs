using Newtonsoft.Json;
using System;
using Willow.Rules.Model;

namespace WillowRules.RepositoryConfiguration;

/// <summary>
/// Converts <see cref="TimeSeriesBuffer"/> to/from JSON for serialization
/// </summary>
public class TimeSeriesBufferJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(TimeSeriesBuffer);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var result = new TimeSeriesBuffer();

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
					case nameof(TimeSeriesBuffer.CompressionState):
						{
							reader.Read();
							result.CompressionState = serializer.Deserialize<TrajectoryCompressorState>(reader);
							break;
						}
					case nameof(TimeSeriesBuffer.MaxCountToKeep):
						{
							result.MaxCountToKeep = reader.ReadAsInt32();
							break;
						}
					case nameof(TimeSeriesBuffer.MaxTimeToKeep):
						{
							if (TimeSpan.TryParse(reader.ReadAsString(), out var value))
							{
								result.MaxTimeToKeep = value;
							}
							break;
						}
					case nameof(TimeSeriesBuffer.Points):
						{
							reader.Read();//read property
							result.Points = serializer.Deserialize<TimedValue[]>(reader);
							break;
						}
					case nameof(TimeSeriesBuffer.UnitOfMeasure):
						{
							result.UnitOfMeasure = reader.ReadAsString();
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
		var timeseries = (TimeSeriesBuffer)value!;

		writer.WriteStartObject();

		if (timeseries.CompressionState != null)
		{
			writer.WritePropertyName(nameof(TimeSeriesBuffer.CompressionState));
			serializer.Serialize(writer, timeseries.CompressionState);
		}

		if (timeseries.MaxCountToKeep.HasValue)
		{
			writer.WritePropertyName(nameof(TimeSeriesBuffer.MaxCountToKeep));
			writer.WriteValue(timeseries.MaxCountToKeep);
		}

		if (timeseries.MaxTimeToKeep.HasValue)
		{
			writer.WritePropertyName(nameof(TimeSeriesBuffer.MaxTimeToKeep));
			writer.WriteValue(timeseries.MaxTimeToKeep);
		}

		writer.WritePropertyName(nameof(TimeSeriesBuffer.Points));
		serializer.Serialize(writer, timeseries.Points);

		if (!string.IsNullOrEmpty(timeseries.UnitOfMeasure))
		{
			writer.WritePropertyName(nameof(TimeSeriesBuffer.UnitOfMeasure));
			writer.WriteValue(timeseries.UnitOfMeasure);
		}

		writer.WriteEndObject();
	}
}
