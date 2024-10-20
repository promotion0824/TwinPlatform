using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Willow.Rules.Model;

/// <summary>
/// A timestamped value for a single trendId
/// </summary>
/// <remarks>
/// struct is marked readonly to properly take advantage of "in" keyword
/// </remarks>
[DebuggerDisplay("{Timestamp} {NumericValue}")]
[StructLayout(LayoutKind.Auto)]
public readonly struct TimedValue
{
	/// <summary>
	/// Invalid marker value
	/// </summary>
	public static readonly TimedValue Invalid = new TimedValue(DateTimeOffset.MinValue, double.NaN);

	/// <summary>
	/// Timestamp of this observation
	/// </summary>
	public DateTimeOffset Timestamp { get; }

	/// <summary>
	/// For points with a double or int value
	/// </summary>
	public double? ValueDouble { get; }

	/// <summary>
	/// For points that are bool values, or for tracking state against a point
	/// </summary>
	public bool? ValueBool { get; }

	/// <summary>
	/// For points that are string values, or for tracking state against a point
	/// </summary>
	public string ValueText { get; }

	/// <summary>
	/// Gets the double value or 1 for true and zero for false or zero for a string
	/// </summary>
	[System.Text.Json.Serialization.JsonIgnore]
	[Newtonsoft.Json.JsonIgnore]
	public double NumericValue => ValueDouble ?? (ValueBool == true ? 1 : 0);

	/// <summary>
	/// Gets the bool value as a numeric 0.0 or 1.0
	/// </summary>
	[System.Text.Json.Serialization.JsonIgnore]
	[Newtonsoft.Json.JsonIgnore]
	public double BoolValue => (ValueBool.HasValue ? (ValueBool == true ? 1.0 : 0.0) : (ValueDouble ?? 0) != 0 ? 1.0 : 0.0);

	/// <summary>
	/// Creates a new <see cref="TimedValue"/>
	/// </summary>
	public TimedValue(DateTimeOffset timestamp, double value)
	{
		this.Timestamp = timestamp;
		this.ValueDouble = value;
		this.ValueText = string.Empty;
	}

	/// <summary>
	/// Creates a new <see cref="TimedPointValue"/>
	/// </summary>
	public TimedValue(DateTimeOffset timestamp, IConvertible value)
	{
		this.Timestamp = timestamp;
		this.ValueText = string.Empty;
		var typeCode = value.GetTypeCode();
		switch (typeCode)
		{
			case TypeCode.Boolean:
				{
					this.ValueBool = value.ToBoolean(CultureInfo.InvariantCulture);
					break;
				}
			default:
				{
					this.ValueDouble = value.ToDouble(CultureInfo.InvariantCulture);
					break;
				}
		}
	}

	/// <summary>
	/// Creates a new <see cref="TimedValue"/>
	/// </summary>
	public TimedValue(DateTimeOffset timestamp, bool value)
	{
		this.Timestamp = timestamp;
		this.ValueBool = value;
		this.ValueText = string.Empty;
	}

	/// <summary>
	/// Creates a new <see cref="TimedValue"/>
	/// </summary>
	public TimedValue(DateTimeOffset timestamp, string value)
	{
		this.Timestamp = timestamp;
		this.ValueText = value;
	}

	/// <summary>
	/// Creates a new <see cref="TimedValue"/>
	/// </summary>
	public TimedValue(DateTimeOffset timestamp, double valueDouble, string valueText)
	{
		this.Timestamp = timestamp;
		this.ValueDouble = valueDouble;
		this.ValueText = valueText;
	}
}
