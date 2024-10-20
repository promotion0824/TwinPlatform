using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Willow.ExpressionParser;
using Willow.Expressions;

namespace Willow.Rules.Model;

public static class ExtensionMethods
{
	public static Batch<TTarget> Transform<TTSource, TTarget>(this Batch<TTSource> value, Func<TTSource, TTarget> transform)
	{
		return new Batch<TTarget>(value.QueryString, value.Before, value.After, value.Total, value.Items.Select(v => transform(v)).ToList(), value.Next);
	}

	public static Batch<TTarget> Transform<TTSource, TTarget>(this Batch<TTSource> value, IEnumerable<TTarget> transform)
	{
		return new Batch<TTarget>(value.QueryString, value.Before, value.After, value.Total, transform, value.Next);
	}

	public static double Min(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end, TimedValue defaultValue)
	{
		int count = 0;
		double minValue = double.MaxValue;

		foreach (var interpolated in list.Interpolated(start, end, v => v.ValueBool.HasValue ? v.BoolValue : v.NumericValue))
		{
			count++;
			minValue = Math.Min(minValue, interpolated.previousValue);
			minValue = Math.Min(minValue, interpolated.currentValue);
		}

		if (count == 0)
		{
			return defaultValue.NumericValue;
		}

		return minValue;
	}

	/// <summary>
	/// Calculates the standard deviation for a time series
	/// </summary>
	public static double StandardDeviation(this IEnumerable<TimedValue> list)
	{
		return WillowMath.StandardDeviation(list.Select(v => v.NumericValue));
	}

	/// <summary>
	/// Validates an <see cref="MLModel"/>
	/// </summary>
	public static (bool ok, string result) ValidateMLModel(this MLModel model)
	{
		if(string.IsNullOrEmpty(model.FullName))
		{
			return (false, $"{nameof(MLModel.FullName)} is required");
		}

		if (model.ModelData is null || model.ModelData.Length == 0)
		{
			return (false, $"{nameof(MLModel.ModelData)} is required");
		}

		return (true, "");
	}

	/// <summary>
	/// Validates a <see cref="GlobalVariable"/>
	/// </summary>
	public static (bool ok, string result) ValidateGlobal(this Model.GlobalVariable global)
	{
		(bool ok, string message) = global.ValidateGlobalName();

		if (!ok)
		{
			return (false, message);
		}

		foreach (var parameter in global.Parameters)
		{
			(bool paramOk, string paramError) = parameter.ValidateFunctionName();

			if (!paramOk)
			{
				return (false, paramError);
			}
		}

		return (true, "");
	}

	/// <summary>
	/// Validates a <see cref="GlobalVariable"/> name
	/// </summary>
	public static (bool ok, string result) ValidateGlobalName(this Model.GlobalVariable global)
	{
		string name = $"{global.VariableType} Name";

		if (string.IsNullOrEmpty(global.Name))
		{
			return (false, $"{name} is required");
		}

		if (!global.Name.ToArray().All(v => char.IsLetterOrDigit(v)))
		{
			return (false, $"{name} may only contain letters and digits");
		}

		return (true, "");
	}

	/// <summary>
	/// Validates a <see cref="FunctionParameter"/> name
	/// </summary>
	public static (bool ok, string result) ValidateFunctionName(this Model.FunctionParameter parameter)
	{
		if (string.IsNullOrEmpty(parameter.Name))
		{
			return (false, "Paramater name is required");
		}

		if (!parameter.Name.ToArray().All(v => char.IsLetterOrDigit(v) || v == '_'))
		{
			return (false, $"Paramater name may only contain letters, digits and underscores");
		}

		if (!parameter.Name.ToArray().Any(v => char.IsLetter(v)))
		{
			return (false, $"Paramater name must contain at least one letter");
		}

		return (true, "");
	}

	/// <summary>
	/// Validates a <see cref="Rule"/>
	/// </summary>
	public static (bool ok, string result) ValidateRule(this Model.Rule rule, ILogger logger)
	{
		using var logScope = logger.BeginScope("Validate rule {ruleId}", rule.Id);

		bool ok = true;

		if(string.IsNullOrWhiteSpace(rule.PrimaryModelId))
		{
			return (false, "PrimaryModelId field is required");
		}

		foreach (var parameter in rule.Parameters)
		{
			if (string.IsNullOrWhiteSpace(parameter.Name))
			{
				return (false, $"Capability {parameter.FieldId} is missing the 'Name' field");
			}
			else if (string.IsNullOrWhiteSpace(parameter.FieldId))
			{
				return (false, $"Capability {parameter.Name} is missing the 'FieldId' field");
			}
		}

		foreach (var parameter in rule.ImpactScores)
		{
			if (string.IsNullOrWhiteSpace(parameter.Name))
			{
				return (false, $"ImpactScore {parameter.FieldId} is missing the 'Name' field");
			}
			else if (string.IsNullOrWhiteSpace(parameter.FieldId))
			{
				return (false, $"ImpactScore {parameter.Name} is missing the 'FieldId' field");
			}
		}

		foreach (var parameter in rule.Parameters.Concat(rule.ImpactScores))
		{
			try
			{
				var test = Parser.Deserialize(parameter.PointExpression);
			}
			catch (ParserException pex)
			{
				ok = false;
				logger.LogWarning("Could not parse {expression}: {message}", parameter.PointExpression, pex.Message);
				return (ok, $"Could not parse {parameter.PointExpression}: {pex.Message}");
			}
		}
		return (ok, "");
	}

	/// <summary>
	/// Calculates the slope of the line with days as the X-axis
	/// </summary>
	public static double Slope(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end)
	{
		var data = list.InterpolatedMerged(start, end, v => v.NumericValue).Select(interpolated =>
		{
			return (interpolated.time, interpolated.value);
		});

		var regression = WillowMath.LinearRegression(data);
		return regression.Slope;
	}

	/// <summary>
	/// Forecasts the value at the end of the period using linear regression
	/// </summary>
	public static double Forecast(this IEnumerable<TimedValue> list, TimeSpan timePeriod)
	{
		if (!list.Any()) return double.NaN;
		if (list.Count() == 1) return list.First().NumericValue;
		var regression = WillowMath.LinearRegression(list.Select(v => (v.Timestamp, v.NumericValue)));
		return regression.Extrapolate(list.Last().Timestamp.Add(timePeriod));
	}

	/// <summary>
	///	Counts the number of leading edge transitions in a boolean time series
	///	</summary>
	public static double CountLeadingEdge(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end)
	{
		int count = 0;
		bool current = false;

		foreach (var item in list)
		{
			bool value = item.NumericValue > 0;
			if (item.Timestamp < start)
			{
				// Track current value before start so we get the transition (or not) after start
				current = value;  // includes bool 0 or 1
				continue;
			}
			if (item.Timestamp > end)
			{
				break;
			}
			if (value)
			{
				if (!current)
				{
					count++;
				}
				current = true;
			}
			else
			{
				current = false;
			}
		}

		return count;
	}

	public static double Max(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end, TimedValue defaultValue)
	{
		int count = 0;
		double maxValue = double.MinValue;

		foreach (var interpolated in list.Interpolated(start, end, v => v.ValueBool.HasValue ? v.BoolValue : v.NumericValue))
		{
			count++;
			maxValue = Math.Max(maxValue, interpolated.previousValue);
			maxValue = Math.Max(maxValue, interpolated.currentValue);
		}

		if (count == 0)
		{
			return defaultValue.NumericValue;
		}

		return maxValue;
	}

	/// <summary>
	/// Delta with interpolation
	/// </summary>
	public static double Delta(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end)
	{
		// Need to iterate sequence just once
		double first = 0;
		double last = 0;
		int count = 0;

		// Assuming the list is sorted by time
		foreach (var interpolated in list.Interpolated(start, end, v => v.NumericValue))
		{
			count++;

			if (count == 1)
			{
				first = interpolated.previousValue;
			}

			last = interpolated.currentValue;
		}

		return last - first;
	}

	/// <summary>
	/// Average with interpolation
	/// </summary>
	public static double Average(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end)
	{
		// Need to iterate sequence just once
		TimedValue previous = default;
		TimedValue first = list.FirstOrDefault();
		double integral = 0.0;
		double totalTime = 0.0;
		int count = 0;

		// Assuming the list is sorted by time
		foreach (var interpolated in list.Interpolated(start, end, v => v.NumericValue))
		{
			count++;
			previous = interpolated.previous;
			integral += CalculateRiemannSum(interpolated.previousValue, interpolated.currentValue, interpolated.previousTime, interpolated.currentTime);
			totalTime += (interpolated.currentTime - interpolated.previousTime).TotalMilliseconds;
		}

		if (count == 0) return first.NumericValue;
		if (end == start) return previous.NumericValue;  // Assume point value is OK, or zero
		if (totalTime == 0.0) return previous.NumericValue;  // Assume point value is OK, or zero
		return integral / totalTime;
	}

	public static double AverageTrue(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end)
	{
		TimedValue previous = default;
		TimedValue first = list.FirstOrDefault();

		double integral = 0.0;
		int count = 0;

		// Assuming the list is sorted by time
		foreach (var interpolated in list.Interpolated(start, end, v => v.BoolValue))
		{
			count++;
			previous = interpolated.previous;
			integral += CalculateRiemannSum(interpolated.previousValue, interpolated.currentValue, interpolated.previousTime, interpolated.currentTime);
		}

		if (count == 0) return first.BoolValue;
		if (end == start) return previous.NumericValue;  // Assume point value is OK, or zero
		return integral / (end - start).TotalMilliseconds;
	}

	private static double LinearInterpolate(double v1, double v2, DateTimeOffset d1, DateTimeOffset d2, DateTimeOffset t)
	{
		double milliseconds = (d2 - d1).TotalMilliseconds;
		if (milliseconds == 0) return (v1 + v2) / 2.0;
		double m = (v2 - v1) / milliseconds;
		double r = v1 + m * (t - d1).TotalMilliseconds;
		return r;
	}

	/// <summary>
	/// Checks whether the values' Timestamp is greater than the offset - 3 x the trend interval
	/// </summary>
	public static bool IsTimely(this TimeSeries value, DateTimeOffset now)
	{
		if (!string.IsNullOrEmpty(value.LastValueString))
		{
			//text values are event based data so they come in very rarely, don't do timely checks
			return true;
		}


		if (value.Count < 2) return false;               // not enough data, we need at least 2 points.
														 // For buffers where very old data is cut off and suddenly a new point comes in, we wait for the 2nd one.
														 // the rule must first go into Insufficient data (to record the large gap output) before it can go back to valid/faulted
		if (!value.TrendInterval.HasValue) { return true; } // change of value, can't detect untimely values

		//some telemetry comes in slower than the configured interval so we take the largest of trendinverval vs estimatedperiod
		var trendInterval = Math.Max(value.EstimatedPeriod.TotalSeconds, value.TrendInterval ?? 0);
		var longTimeOffset = trendInterval * 3.0;

		//Haven't seen any data for a long time
		if (value.LastSeen.AddSeconds(longTimeOffset) < now)
		{
			return false;
		}

		// Have seen data but it's not regular enough to be trustworthy
		// If less than three intervals have elapsed, it's OK, i.e. can miss the odd value and will use previous value
		if (value.LastGap.TotalSeconds <= longTimeOffset) { return true; }

		// It's more than three intervals overdue, declare it bad
		return false;
	}

	/// <summary>
	/// Removes the area of the curve below this line y = v
	/// </summary>
	/// <remarks>
	/// Useful for calculating the amount of a trend that is above some threshold
	/// If the entire curve is below v it returns one zero value for each point in the input
	/// </remarks>
	public static IEnumerable<TimedValue> Above(this IEnumerable<TimedValue> list2, double v)
	{
		// Check for start, end, no elements
		if (!list2.Any()) yield break;

		var previous = list2.First();
		double previousValue = previous.NumericValue;

		// Start of interval is above
		if (previousValue > v)
		{
			yield return new TimedValue(previous.Timestamp, previousValue - v);
		}

		// Assuming the list is sorted by time
		foreach (var current in list2.Skip(1))
		{
			var adjustedCurrent = current;
			double currentValue = current.NumericValue;

			if (TryIntersect(previous, current, v, out var intersected))
			{
				// Return the intersection point
				yield return new TimedValue(intersected!.Timestamp, 0.0);
			}

			// Clamp if below
			if (currentValue < v)
			{
				adjustedCurrent = new TimedValue(current.Timestamp, 0.0);
			}
			else
			{
				adjustedCurrent = new TimedValue(current.Timestamp, currentValue - v);
			}

			yield return adjustedCurrent;

			previousValue = currentValue;
			previous = current;  // The actual point not the adjusted point
		}
	}

	/// <summary>
	/// Removes the area of the curve above this line y = v
	/// </summary>
	/// <remarks>
	/// Useful for calculating the amount of a trend that is below some threshold
	/// If the entire curve is above v it returns one zero value for each point in the input
	/// </remarks>
	public static IEnumerable<TimedValue> Below(IEnumerable<TimedValue> list2, double v)
	{
		// Check for start, end, no elements
		if (!list2.Any()) yield break;

		var previous = list2.First();
		double previousValue = previous.NumericValue;

		if (previousValue < v)
		{
			yield return new TimedValue(previous.Timestamp, v - previousValue);
		}

		// Assuming the list is sorted by time
		foreach (var current in list2.Skip(1))
		{
			double currentValue = current.NumericValue;

			// Does this segment intersect the line y = v?
			if (TryIntersect(previous, current, v, out var intersected))
			{
				// Return the intersection point as a zero (y=v)
				yield return new TimedValue(intersected!.Timestamp, 0.0);
			}

			TimedValue adjustedCurrent = default;

			// Clamp if above
			if (currentValue > v)
			{
				adjustedCurrent = new TimedValue(current.Timestamp, 0.0);
			}
			else
			{
				adjustedCurrent = new TimedValue(current.Timestamp, v - currentValue);
			}

			yield return adjustedCurrent;
			previousValue = currentValue;
			previous = current;  // The actual point not the adjusted point
		}
	}


	public static IEnumerable<TimedValue> Union(this List<TimedValue> a, List<TimedValue> b)
	{
		var ae = a.GetEnumerator();
		var be = b.GetEnumerator();

		bool hasa = ae.MoveNext();
		bool hasb = be.MoveNext();

		while (hasa && hasb)
		{
			if (ae.Current.Timestamp <= be.Current.Timestamp)
			{
				yield return ae.Current;
				hasa = ae.MoveNext();
			}
			else
			{
				yield return be.Current;
				hasb = be.MoveNext();
			}
		}

		while (hasa)
		{
			yield return ae.Current;
			hasa = ae.MoveNext();
		}

		while (hasb)
		{
			yield return be.Current;
			hasb = be.MoveNext();
		}
	}

	/// <summary>
	/// Multiply the numeric values in one time series by another time series
	/// </summary>
	/// <remarks>
	/// Also uses 1 and 0 for bool values which means they can act as a gating function, e.g. occupancy
	/// </remarks>
	public static IEnumerable<TimedValue> Multiply(this IEnumerable<TimedValue> a, IEnumerable<TimedValue> b)
	{
		var ae = a.GetEnumerator();
		var be = b.GetEnumerator();

		bool hasa = ae.MoveNext();
		bool hasb = be.MoveNext();

		bool seenA = false;
		bool seenB = false;

		double lastA = 0.0;
		double lastB = 0.0;

		while (hasa && hasb)
		{
			if (ae.Current.Timestamp <= be.Current.Timestamp)
			{
				seenA = true;
				lastA = ae.Current.NumericValue;
				hasa = ae.MoveNext();
			}
			else
			{
				seenB = true;
				lastB = be.Current.NumericValue;
				hasb = be.MoveNext();
			}

			// TODO: Really need to interpolate values first!

			if (seenA && seenB)
			{
				DateTimeOffset latest = ae.Current.Timestamp > be.Current.Timestamp ? ae.Current.Timestamp : be.Current.Timestamp;
				yield return new TimedValue(latest, lastA * lastB);
			}
		}

		while (hasa)
		{
			seenA = true;
			lastA = ae.Current.NumericValue;
			hasa = ae.MoveNext();

			if (seenA && seenB)
			{
				DateTimeOffset latest = ae.Current.Timestamp > be.Current.Timestamp ? ae.Current.Timestamp : be.Current.Timestamp;
				yield return new TimedValue(latest, lastA * lastB);
			}
		}

		while (hasb)
		{
			seenB = true;
			lastB = ae.Current.NumericValue;
			hasb = be.MoveNext();

			if (seenA && seenB)
			{
				DateTimeOffset latest = ae.Current.Timestamp > be.Current.Timestamp ? ae.Current.Timestamp : be.Current.Timestamp;
				yield return new TimedValue(latest, lastA * lastB);
			}
		}
	}


	/// <summary>
	/// Calculates the average amount above a given value (e.g. average over 77F)
	/// ignoring any portion below. Correctly handles start and end interpolation and
	/// all crossing point interpolations.
	/// </summary>
	public static double AverageAbove(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end, double v)
	{
		var above = Above(list, v);
		return Average(above, start, end);
	}

	/// <summary>
	/// Calculates the average amount below a given value (e.g. average below 77F)
	/// ignoring any portion below. Correctly handles start and end interpolation and
	/// all crossing point interpolations.
	/// </summary>
	public static double AverageBelow(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end, double v)
	{
		var below = Below(list, v);
		return Average(below, start, end);
	}

	/// <summary>
	/// Gets the duration of the curve that is above zero with interpolation
	/// </summary>
	public static TimeSpan DurationAboveZero(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end)
	{
		TimeSpan integral = TimeSpan.Zero;
		int count = 0;

		// Assuming the list is sorted by time
		foreach (var interpolated in list.Interpolated(start, end, v => v.NumericValue))
		{
			count++;

			if (interpolated.previousValue > 0 || interpolated.currentValue > 0)
			{
				integral += interpolated.currentTime - interpolated.previousTime;
			}
		}

		if (count < 1) return TimeSpan.Zero;
		if (end == start) return TimeSpan.Zero;
		return integral;
	}

	/// <summary>
	/// Intersect the line from value1 to value2 with the horizontal line y = v
	/// </summary>
	private static bool TryIntersect(TimedValue value1, TimedValue value2, double v, out TimedValue result)
	{
		double d1 = value1.NumericValue;
		double d2 = value2.NumericValue;

		if (d1 < v && d2 < v) { result = default; return false; }
		if (d1 >= v && d2 >= v) { result = default; return false; }
		if (d1 == d2) { result = default; return false; } // all same value d1 == d2 == v

		// We know d1 and d2 lie on opposite sides of the line so we have a crossing

		long t1 = value1.Timestamp.ToUnixTimeMilliseconds();
		long t2 = value2.Timestamp.ToUnixTimeMilliseconds();

		double m = (d2 - d1) / (t2 - t1);  // will never be zero

		// Solve v = d1 + m * deltat to find deltat

		long deltat = (long)((v - d1) / m);

		DateTimeOffset ts = value1.Timestamp.AddMilliseconds(deltat);

		result = new TimedValue(ts, v);
		return true;
	}

	private static double CalculateRiemannSum(double base1, double base2, DateTimeOffset t1, DateTimeOffset t2)
	{
		return (base1 + base2) / 2 * (t2 - t1).TotalMilliseconds;
	}

	/// <summary>
	/// Implements hysteresis on a single timedpoint value sequence
	/// </summary>
	/// <remarks>
	/// Use .where(x => x.PointEntityId == pointEntityId) before calling
	/// </remarks>
	public static bool Hysteresis(this IEnumerable<TimedValue> input,
		bool hasHighLimit, bool hasLowLimit, bool hasHighReset, bool hasLowReset,
		double highLimit, double lowLimit, double highReset, double lowReset)
	{
		bool outputState = false;

		foreach (var tpv in input)
		{
			bool highFault = false;
			bool lowFault = false;
			if (hasHighLimit)
			{
				if (tpv.ValueDouble > highLimit)
				{
					highFault = true;
				}
				else if (hasHighReset && (tpv.ValueBool ?? false) && tpv.ValueDouble > highReset)
				{
					// was faulted and it's still above (ignore the case where it leaps from below dead-band to above dead-band)
					highFault = true;
				}
			}
			if (hasLowLimit)
			{
				if (tpv.ValueDouble < lowLimit)
				{
					lowFault = true;
				}
				else if (hasLowReset && (tpv.ValueBool ?? false) && tpv.ValueDouble < lowReset)
				{
					lowFault = true;
				}
			}

			outputState = outputState || highFault || lowFault;
		}

		return outputState;
	}

	/// <summary>
	/// Create a rule Id based on the rule name
	/// </summary>
	/// <remarks>
	/// Regex will raplace any spaces (and special characters) with dash
	/// </remarks>
	public static string ToIdStandard(this string name)
	{
		// Replace any sequence of non-alphanumeric characters with a dash
		string result = Regex.Replace(name, @"[^0-9a-zA-Z]+", "-");

		// Trim leading and trailing dashes
		result = result.Trim('-');

		return result.ToLower();
	}

	/// <summary>
	/// Verify the id string adheres to Id naming standards
	/// </summary>
	public static bool IsToIdStandard(this string id)
	{
		return Regex.IsMatch(id, @"^[0-9a-zA-Z-]+$", RegexOptions.IgnoreCase);
	}

	/// <summary>
	/// Returns interpolation where previous and next are conveniently merged
	/// </summary>
	public static IEnumerable<
		(double value,
		DateTimeOffset time)>
		InterpolatedMerged(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end, Func<TimedValue, double> value)
	{
		int count = 0;
		foreach(var interpolated  in list.Interpolated(start, end, value))
		{
			count++;

			if(count == 1)
			{
				yield return (interpolated.previousValue, interpolated.previousTime);
				yield return (interpolated.currentValue, interpolated.currentTime);
			}
			else
			{
				yield return (interpolated.currentValue, interpolated.currentTime);
			}
		}
	}

	/// <summary>
	/// Convert a timeseries list to interpolated values
	/// </summary>
	public static IEnumerable<
		(TimedValue previous,
		double previousValue,
		DateTimeOffset previousTime,
		TimedValue current,
		double currentValue,
		DateTimeOffset currentTime)>
		Interpolated(this IEnumerable<TimedValue> list, DateTimeOffset start, DateTimeOffset end, Func<TimedValue, double> value)
	{
		TimedValue previous = default;
		int count = 0;

		// Assuming the list is sorted by time
		foreach (var current in list)
		{
			count++;

			if (count == 1)
			{
				previous = current;
				continue;
			}

			if (current.Timestamp < start)  // entirely before
			{
			}
			else if (previous.Timestamp > end) // entirely after
			{
			}
			else
			{
				double startValue = value(previous);
				double endValue = value(current);

				DateTimeOffset startTime = previous.Timestamp;
				DateTimeOffset endTime = current.Timestamp;

				DateTimeOffset adjustedStartTime = startTime;
				DateTimeOffset adjustedEndTime = endTime;

				// Move start forward to intersection if overlaps start
				if (previous.Timestamp <= start)
				{
					startValue = LinearInterpolate(startValue, endValue, previous.Timestamp, current.Timestamp, start);
					adjustedStartTime = start;
				}

				// Move end back if overlaps end
				if (current.Timestamp > end)
				{
					endValue = LinearInterpolate(startValue, endValue, previous.Timestamp, current.Timestamp, end);
					adjustedEndTime = end;
				}

				yield return (previous, startValue, adjustedStartTime, current, endValue, adjustedEndTime);
			}

			previous = current;
		}
	}
}
