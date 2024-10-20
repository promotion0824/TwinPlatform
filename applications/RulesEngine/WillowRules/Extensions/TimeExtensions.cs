using System;

namespace Willow.Rules;

/// <summary>
/// Extensions for <see cref="DateTime"/> and <see cref="DateTimeOffset"/>
/// </summary>
public static class TimeExtensions
{
	/// <summary>
	/// Apply an offset to a <see cref="DateTime"/> and convert to <see cref="DateTimeOffset"/>
	/// </summary>
	public static DateTimeOffset ConvertToDateTimeOffset(this DateTime timestamp, TimeZoneInfo timeZoneInfo)
	{
		var timeOffset = new DateTimeOffset(timestamp);
		return TimeZoneInfo.ConvertTime(timeOffset, timeZoneInfo);
	}
}
