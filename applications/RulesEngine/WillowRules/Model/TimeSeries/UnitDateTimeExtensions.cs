using System;
using Willow.Expressions;

namespace Willow.Rules.Model;
public static class UnitDateTimeExtensions
{
	/// <summary>
	/// Converts a unit of time to it's equivelant time span
	/// </summary>
	public static TimeSpan GetTimeSpanDuration(this Unit unit, double unitOfMeasureValue, DateTimeOffset date)
	{
		if (Unit.day.Equals(unit))
		{
			return TimeSpan.FromDays(unitOfMeasureValue);
		}

		if (Unit.hour.Equals(unit))
		{
			return TimeSpan.FromHours(unitOfMeasureValue);
		}

		if (Unit.minute.Equals(unit))
		{
			return TimeSpan.FromMinutes(unitOfMeasureValue);
		}

		if (Unit.week.Equals(unit))
		{
			return TimeSpan.FromDays(unitOfMeasureValue * 7);
		}

		if (Unit.month.Equals(unit))
		{
			var targetDate = date.AddMonths((int)-unitOfMeasureValue);
			return TimeSpan.FromDays((date - targetDate).TotalDays);
		}

		return TimeSpan.FromSeconds(unitOfMeasureValue);
	}

	public static DateTimeOffset SnapToDateTime(this Unit unit, double unitOfMeasureValue, DateTimeOffset date)
	{
		DateTime result = date.DateTime;

		if (Unit.month.Equals(unit))
		{
			var start = new DateTime(date.Year, date.Month, 1);
			result = start.AddMonths((int)unitOfMeasureValue + 1);
		}
		else if (Unit.week.Equals(unit))
		{
			var diff = date.DayOfWeek - DayOfWeek.Sunday;

			if (diff < 0)
			{
				diff += 7;
			}

			var start = date.AddDays(-1 * diff).Date;

			result = start.AddDays(unitOfMeasureValue * 7);
		}
		else if (Unit.day.Equals(unit))
		{
			var start = new DateTime(date.Year, date.Month, date.Day);
			result = start.AddDays(unitOfMeasureValue);
		}
		else if (Unit.hour.Equals(unit))
		{
			var start = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
			result = start.AddHours(unitOfMeasureValue);
		}
		else if (Unit.minute.Equals(unit))
		{
			var start = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
			result = start.AddMinutes(unitOfMeasureValue);
		}
		else if (Unit.second.Equals(unit))
		{
			result = result.AddSeconds(unitOfMeasureValue);
		}

		return new DateTimeOffset(result, date.Offset);
	}

	public static double FromSeconds(this Unit unit, double timeInSeconds)
	{
		if (Unit.minute.Equals(unit)) { return timeInSeconds / 60; }
		if (Unit.hour.Equals(unit)) { return timeInSeconds / 3600; }
		if (Unit.day.Equals(unit)) { return timeInSeconds / 86400; }

		return timeInSeconds;
	}
}
