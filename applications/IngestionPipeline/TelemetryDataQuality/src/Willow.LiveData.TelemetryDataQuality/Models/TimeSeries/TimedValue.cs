namespace Willow.LiveData.TelemetryDataQuality.Models.TimeSeries;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

/// <summary>
/// A timestamped value for a single point.
/// </summary>
[DebuggerDisplay("{Timestamp} {NumericValue}")]
[StructLayout(LayoutKind.Auto)]
public struct TimedValue
{
    /// <summary>
    /// Invalid marker value.
    /// </summary>
    public static readonly TimedValue Invalid = new(DateTimeOffset.MinValue, double.NaN);

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedValue"/> struct.
    /// Creates a new <see cref="TimedValue"/>.
    /// </summary>
    /// <param name="timestamp">Timestamp.</param>
    /// <param name="value">Telemetry value.</param>
    private TimedValue(DateTimeOffset timestamp, double value)
    {
        Timestamp = timestamp;
        ValueDouble = value;
        ValueText = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedValue"/> struct.
    /// Creates a new <see cref="TimedValue"/>.
    /// </summary>
    /// <param name="timestamp">Timestamp.</param>
    /// <param name="value">Telemetry value.</param>
    public TimedValue(DateTimeOffset timestamp, IConvertible value)
    {
        Timestamp = timestamp;
        ValueText = string.Empty;
        var typeCode = value.GetTypeCode();
        switch (typeCode)
        {
            case TypeCode.Boolean:
            {
                ValueBool = value.ToBoolean(CultureInfo.InvariantCulture);
                break;
            }

            default:
            {
                ValueDouble = value.ToDouble(CultureInfo.InvariantCulture);
                break;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedValue"/> struct.
    /// Creates a new <see cref="TimedValue"/>.
    /// </summary>
    /// <param name="timestamp">Timestamp.</param>
    /// <param name="value">Telemetry value.</param>
    public TimedValue(DateTimeOffset timestamp, bool value)
    {
        Timestamp = timestamp;
        ValueBool = value;
        ValueText = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedValue"/> struct.
    /// Creates a new <see cref="TimedValue"/>.
    /// </summary>
    /// <param name="timestamp">Timestamp.</param>
    /// <param name="value">Telemetry value.</param>
    public TimedValue(DateTimeOffset timestamp, string value)
    {
        Timestamp = timestamp;
        ValueText = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedValue"/> struct.
    /// Creates a new <see cref="TimedValue"/>.
    /// </summary>
    /// <param name="timestamp">Timestamp.</param>
    /// <param name="valueDouble">Telemetry value.</param>
    /// <param name="valueText">Text associated with the value.</param>
    public TimedValue(DateTimeOffset timestamp, double valueDouble, string valueText)
    {
        Timestamp = timestamp;
        ValueDouble = valueDouble;
        ValueText = valueText;
    }

    /// <summary>
    /// Gets or sets the timestamp of this observation.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets for points with a double or int value.
    /// </summary>
    public double? ValueDouble { get; set; }

    /// <summary>
    /// Gets or sets for points that are bool values, or for tracking state against a point.
    /// </summary>
    public bool? ValueBool { get; set; }

    /// <summary>
    /// Gets or sets for points that are string values, or for tracking state against a point.
    /// </summary>
    public string ValueText { get; set; }

    /// <summary>
    /// Gets the double value or 1 for true and zero for false or zero for a string.
    /// </summary>
    [JsonIgnore]
    public double NumericValue => ValueDouble ?? (ValueBool == true ? 1 : 0);

    /// <summary>
    /// Gets the bool value as a numeric 0.0 or 1.0.
    /// </summary>
    [JsonIgnore]
    public double BoolValue => ValueBool.HasValue ? ValueBool == true ? 1.0 : 0.0 : (ValueDouble ?? 0) != 0 ? 1.0 : 0.0;
}
