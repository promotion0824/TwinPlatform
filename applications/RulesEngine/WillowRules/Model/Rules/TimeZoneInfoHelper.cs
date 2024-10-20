// POCO class
#nullable disable

using System;

namespace Willow.Rules.Model;

/// <summary>
/// Converts strings to TimeZoneInfo with fallback to UTC
/// </summary>
public static class TimeZoneInfoHelper
{
	/// <summary>
	/// Converts a timezone string to a TimeZoneInfo object
	/// </summary>
	public static TimeZoneInfo From(string timeZone)
	{
		var tz = timeZone;

		TimeZoneInfo localTimeZoneInfo = TimeZoneInfo.Utc;
		try
		{
			if (!string.IsNullOrEmpty(tz))
			{
				localTimeZoneInfo = TimeZoneConverter.TZConvert.GetTimeZoneInfo(tz);
			}
		}
		catch (Exception)
		{
			//logger.LogWarning(ex, "Failed to find timezone {tz}", tz);
			localTimeZoneInfo = TimeZoneInfo.Utc;
		}
		return localTimeZoneInfo;
	}
}
