using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace WillowRules.Migrations;

/// <summary>
/// Base class for repository configuration
/// </summary>
public class ConfigurationBase
{
	/// <summary>
	/// Deserialize a Bson value to T
	/// </summary>
	protected static T? FromBson<T>(byte[] data)
	{
		using (var ms = new MemoryStream(data))
		using (var reader = new BsonDataReader(ms))
		{
			var serializer = new JsonSerializer();
			return serializer.Deserialize<T>(reader);
		}
	}

	/// <summary>
	/// Serialize a value to Bson
	/// </summary>
	protected static byte[] ToBson<T>(T value)
	{
		using (var ms = new MemoryStream())
		using (var datawriter = new BsonDataWriter(ms))
		{
			var serializer = new JsonSerializer()
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			serializer.Serialize(datawriter, value);
			return ms.ToArray();
		}
	}
}
