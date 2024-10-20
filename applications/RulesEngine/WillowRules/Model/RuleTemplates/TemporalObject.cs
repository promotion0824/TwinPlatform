using System;
using System.Linq;
using Willow.Expressions;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// An <see cref="TimeSeries"/> representation of <see cref="ITemporalObject"/>
/// </summary>
public class TemporalObject : ITemporalObject
{
	private readonly TimeSpan maxTime;
	private readonly TimeSeriesBuffer timeseries;
	private readonly DateTimeOffset now;
	private readonly IConvertible lastValue;

	/// <summary>
	/// Constructor
	/// </summary>
	public TemporalObject(TimeSeriesBuffer timeseries, TimeSpan maxTime, DateTimeOffset now)
	{
		this.timeseries = timeseries ?? throw new ArgumentNullException(nameof(timeseries));
		this.maxTime = maxTime;
		this.now = now;

		var lastPoint = timeseries.LastOrDefault();

		lastValue = 0;

		if (lastPoint.ValueDouble is double d)
		{
			lastValue = d;
		}
		else if (lastPoint.ValueBool is bool b)
		{
			lastValue = b;
		}
	}

	public IConvertible All(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);
		return timeseries.GetRange(start, end).All(v => v.ValueBool ?? false);
	}

	public IConvertible Any(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);
		return timeseries.GetRange(start, end).Any(v => v.ValueBool ?? false);
	}

	public IConvertible Average(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);

		return timeseries.Points.Average(start, end);
	}

	public IConvertible Count(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);

		return timeseries.Points.CountLeadingEdge(start, end);
	}

	public IConvertible Max(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);

		return timeseries.Points.Max(start, end, TimedValue.Invalid);
	}

	public IConvertible Min(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);

		return timeseries.Points.Min(start, end, TimedValue.Invalid);
	}

	public IConvertible Sum(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);

		return timeseries.GetRange(start, end).Sum(v => v.ValueDouble ?? 0);
	}

	public IConvertible DeltaLastAndPrevious()
	{
		return timeseries.GetLastDelta();
	}

	public IConvertible DeltaTimeLastAndPrevious(Unit? unitOfMeasure)
	{
		var lastDeltaTime = timeseries.GetLastDeltaTime();

		if (unitOfMeasure != null)
		{
			lastDeltaTime = unitOfMeasure.FromSeconds(lastDeltaTime);
		}

		return lastDeltaTime;
	}

	public IConvertible Delta(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);

		return timeseries.Points.Delta(start, end);
	}

	public IConvertible StandardDeviation(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);
		return timeseries.GetRange(start, end).StandardDeviation();
	}

	public IConvertible Slope(UnitValue startPeriod, UnitValue endPeriod)
	{
		(var timeseries, var start, var end) = GetTimeseries(startPeriod, endPeriod);

		return timeseries.Points.Slope(start, end);
	}

	public IConvertible Forecast(UnitValue startPeriod, UnitValue endPeriod)
	{
		return timeseries.Points.Forecast(startPeriod.Unit.GetTimeSpan(startPeriod.Value));
	}

	public (bool ok, TimeSpan buffer) IsInRange(UnitValue startPeriod, UnitValue endPeriod)
	{
		var (bufferTime, _, _) = GetMaxBufferTime(startPeriod, endPeriod);

		if (timeseries.Points.Count() >= 2)
		{
			var timeDiff = (timeseries.Points.LastOrDefault().Timestamp - timeseries.Points.FirstOrDefault().Timestamp);
			bool ok = timeDiff >= bufferTime;

			return (ok, bufferTime);
		}

		return (false, bufferTime);
	}

	public TypeCode GetTypeCode()
	{
		return lastValue.GetTypeCode();
	}

	public bool ToBoolean(IFormatProvider? provider)
	{
		return lastValue.ToBoolean(provider);
	}

	public byte ToByte(IFormatProvider? provider)
	{
		return lastValue.ToByte(provider);
	}

	public char ToChar(IFormatProvider? provider)
	{
		return lastValue.ToChar(provider);
	}

	public DateTime ToDateTime(IFormatProvider? provider)
	{
		return lastValue.ToDateTime(provider);
	}

	public decimal ToDecimal(IFormatProvider? provider)
	{
		return lastValue.ToDecimal(provider);
	}

	public double ToDouble(IFormatProvider? provider)
	{
		return lastValue.ToDouble(provider);
	}

	public short ToInt16(IFormatProvider? provider)
	{
		return lastValue.ToInt16(provider);
	}

	public int ToInt32(IFormatProvider? provider)
	{
		return lastValue.ToInt32(provider);
	}

	public long ToInt64(IFormatProvider? provider)
	{
		return lastValue.ToInt64(provider);
	}

	public sbyte ToSByte(IFormatProvider? provider)
	{
		return lastValue.ToSByte(provider);
	}

	public float ToSingle(IFormatProvider? provider)
	{
		return lastValue.ToSingle(provider);
	}

	public string ToString(IFormatProvider? provider)
	{
		return lastValue.ToString(provider);
	}

	public object ToType(Type conversionType, IFormatProvider? provider)
	{
		return lastValue.ToType(conversionType, provider);
	}

	public ushort ToUInt16(IFormatProvider? provider)
	{
		return lastValue.ToUInt16(provider);
	}

	public uint ToUInt32(IFormatProvider? provider)
	{
		return lastValue.ToUInt32(provider);
	}

	public ulong ToUInt64(IFormatProvider? provider)
	{
		return lastValue.ToUInt64(provider);
	}

	public override string ToString()
	{
#pragma warning disable CS8603 // Possible null reference return.
		return lastValue.ToString();
#pragma warning restore CS8603 // Possible null reference return.
	}

	private (TimeSeriesBuffer timeseries, DateTimeOffset start, DateTimeOffset end) GetTimeseries(UnitValue startPeriod, UnitValue endPeriod)
	{
		var (maxBufferTime, start, end) = GetMaxBufferTime(startPeriod, endPeriod);

		timeseries.SetMaxBufferTime(maxBufferTime);

		return (timeseries, start, end);
	}

	private (TimeSpan bufferTime, DateTimeOffset start, DateTimeOffset end) GetMaxBufferTime(UnitValue startPeriod, UnitValue endPeriod)
	{
		var end = (endPeriod.Value != 0) ? endPeriod.Unit.SnapToDateTime(endPeriod.Value, now) : now;
		var timespanDuration = startPeriod.Unit.GetTimeSpanDuration(startPeriod.Value, end);
		timespanDuration = (timespanDuration > maxTime) ? maxTime : timespanDuration;
		var start = end - timespanDuration;

		TimeSpan bufferTime;
		if (endPeriod.Value != 0)
		{
			// To prevent time period validation from failing too quickly, request 1 day less in buffer time
			bufferTime = TimeSpan.FromDays((now - start).TotalDays - 1);
		}
		else
		{
			bufferTime = timespanDuration;
		}

		return (bufferTime, start, end);
	}
}
