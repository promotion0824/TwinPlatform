using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using Willow.Rules.Cache;
using Willow.Rules.Repository;
using WillowRules.RepositoryConfiguration;

namespace WillowRules.Migrations;

/// <summary>
/// Helper to configure array properties as serialized values
/// </summary>
public static class ConfigurationExtensions
{
	public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
	{
		Converters = new List<JsonConverter>
		{
			new TokenExpressionJsonConverter(),
			new KalmanJsonConverter(),
			new TimedValueJsonConverter(),
			new OutputValueJsonConverter(),
			new CommandOutputValueJsonConverter(),
			new TimeSeriesBufferJsonConverter(),
			new TrajectoryCompressorStateJsonConverter()
		},
		NullValueHandling = NullValueHandling.Ignore,
		DefaultValueHandling = DefaultValueHandling.Ignore,
		TypeNameHandling = TypeNameHandling.Auto
	};

	public static readonly JsonSerializerSettings JsonSettingsWithoutTypeName = new JsonSerializerSettings
	{
		Converters = new List<JsonConverter> { new TokenExpressionJsonConverter() },
		NullValueHandling = NullValueHandling.Ignore,
		DefaultValueHandling = DefaultValueHandling.Ignore,
		TypeNameHandling = TypeNameHandling.None
	};

	/// <summary>
	/// Marks a virtual IList[T] field to be serialized to and from JSON 
	/// </summary>
	public static PropertyBuilder<IList<U>> ArrayAsJson<T, U>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, IList<U>>> propertyGetter) where T : class, IId
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property<IList<U>>(propertyGetter).HasConversion(
			v => JsonConvert.SerializeObject(v, JsonSettings),
			v => JsonConvert.DeserializeObject<IList<U>>(v, JsonSettings)!,
				new ValueComparer<IList<U>>(
				(c1, c2) => c1!.SequenceEqual(c2!),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
				c => c.ToList()));
	}

	/// <summary>
	/// Marks a virtual IList[T] field to be serialized to and from GZIP and JSON 
	/// </summary>
	public static PropertyBuilder<IList<U>> ArrayAsCompressedJson<T, U>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, IList<U>>> propertyGetter) where T : class, IId
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property<IList<U>>(propertyGetter).HasConversion(
			v => v.Compress(),
			v => (v != null && v.Length > 0) ? v.Decompress<IList<U>>() : new List<U>(),
				new ValueComparer<IList<U>>(
				(c1, c2) => c1!.SequenceEqual(c2!),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
				c => c.ToList()));
	}

	/// <summary>
	/// Marks a virtual IEnumerable[T] field to be serialized to and from GZIP and JSON 
	/// </summary>
	public static PropertyBuilder<IEnumerable<U>> EnumerableAsCompressedJson<T, U>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, IEnumerable<U>>> propertyGetter) where T : class
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property(propertyGetter).HasConversion(
			v => v.Compress(),
			v => (v != null && v.Length > 0) ? v.Decompress<U[]>() : new U[0]);
	}

	/// <summary>
	/// Marks a complex object to be serialized to and from JSON 
	/// </summary>
	public static PropertyBuilder<U> AsJson<T, U>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, U>> propertyGetter)
		where T : class, IId
		where U : IEquatable<U>
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property(propertyGetter).HasConversion(
			// must include type information in root object
			v => JsonConvert.SerializeObject(v, JsonSettings),
			v => JsonConvert.DeserializeObject<U>(v, JsonSettings)!,
				new ValueComparer<U>(
				(c1, c2) => c1!.Equals(c2!),
				c => c!.GetHashCode()));
	}

	/// <summary>
	/// Marks a complex object to be serialized to and from JSON 
	/// </summary>
	public static PropertyBuilder<U> AsJsonAny<T, U>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, U>> propertyGetter)
		where T : class
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property(propertyGetter).HasConversion(
			// must include type information in root object
			v => JsonConvert.SerializeObject(v, JsonSettings),
			v => JsonConvert.DeserializeObject<U>(v, JsonSettings)!);
	}

	/// <summary>
	/// Marks a complex object to be serialized to and from GZIP and JSON 
	/// </summary>
	public static PropertyBuilder<U> AsCompressedJsonAnyType<T, U>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, U>> propertyGetter)
		where T : class
		where U : new()
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property(propertyGetter).HasConversion(
			v => v!.Compress(),
			v => (v != null && v.Length > 0) ? v.Decompress<U>() : new U());
	}

	/// <summary>
	/// Marks a complex object to be serialized to and from JSON 
	/// </summary>
	public static PropertyBuilder<U> AsJsonWithDefault<T, U>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, U>> propertyGetter, U defaultValue)
		where T : class, IId
		where U : IEquatable<U>
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property<U>(propertyGetter).HasConversion(
			// must include type information in root object
			v => JsonConvert.SerializeObject(v, JsonSettings),
			v => string.IsNullOrEmpty(v) ? defaultValue : JsonConvert.DeserializeObject<U>(v, JsonSettings)!,
				new ValueComparer<U>(
				(c1, c2) => c1!.Equals(c2!),
				c => c!.GetHashCode()));
	}

	/// <summary>
	/// Marks a dictionary object to be serialized to and from JSON 
	/// </summary>
	public static PropertyBuilder<IReadOnlyDictionary<string, string>> AsDictionaryJson<T>(this EntityTypeBuilder<T> builder,
		Expression<Func<T, IReadOnlyDictionary<string, string>>> propertyGetter)
		where T : class, IId
	{
		// This Converter will perform the conversion to and from Json to the desired type
		return builder.Property<IReadOnlyDictionary<string, string>>(propertyGetter).HasConversion(
			// must include type information in root object
			v => JsonConvert.SerializeObject(v, JsonSettingsWithoutTypeName),
			v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v, JsonSettingsWithoutTypeName)!,
				new ValueComparer<IReadOnlyDictionary<string, string>>(
				(c1, c2) => AreEqual(c1!, c2!),
				c => c!.Keys.GetListHashCode()));
	}

	/// <summary>
	/// Compresses a string using GZIP compression
	/// </summary>
	public static byte[] Compress(this object value)
	{
		using (var msi = new MemoryStream())
		using (var mso = new MemoryStream())
		{
			using (var writer = new StreamWriter(stream: msi, leaveOpen: true))
			{
				using (var jsonWriter = new JsonTextWriter(writer))
				{
					var serializer = JsonSerializer.Create(JsonSettings);
					serializer.Serialize(jsonWriter, value);
					jsonWriter.Flush();
					msi.Seek(0, SeekOrigin.Begin);

					using (var gs = new GZipStream(mso, CompressionMode.Compress))
					{
						CopyTo(msi, gs);
					}
				}
			}

			return mso.ToArray();
		}
	}

	/// <summary>
	/// Decompresses bindary using GZIP compression
	/// </summary>
	public static T Decompress<T>(this byte[] bytes)
	{
		using (var msi = new MemoryStream(bytes))
		using (var mso = new MemoryStream())
		{
			using (var gs = new GZipStream(msi, CompressionMode.Decompress))
			{
				using (var sr = new StreamReader(gs))
				using (var reader = new JsonTextReader(sr))
				{
					var serializer = JsonSerializer.Create(JsonSettings);
					return serializer.Deserialize<T>(reader)!;
				}
			}
		}
	}

	private static void CopyTo(Stream src, Stream dest)
	{
		byte[] bytes = new byte[4096];

		int cnt;

		while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
		{
			dest.Write(bytes, 0, cnt);
		}
	}

	/// <summary>
	/// Gets a hashcode for an enumerable
	/// </summary>
	static int GetListHashCode<T>(this IEnumerable<T> obj)
	{
		return obj.Aggregate(17, (hash, item) => hash * 23 ^ item!.GetHashCode());
	}

	static bool AreEqual(IReadOnlyDictionary<string, string> thisItems, IReadOnlyDictionary<string, string> otherItems)
	{
		if (thisItems.Count != otherItems.Count)
		{
			return false;
		}
		var thisKeys = thisItems.Keys;
		foreach (var key in thisKeys)
		{
			if (!(otherItems.TryGetValue(key, out var value) &&
				  string.Equals(thisItems[key], value, StringComparison.Ordinal)))
			{
				return false;
			}
		}
		return true;
	}
}
