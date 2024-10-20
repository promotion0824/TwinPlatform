#nullable enable
using System;

namespace Willow.Calendar
{
    public static class TimeZoneExtensions
    {
      
        public static TimeZoneInfo FindEquivalentWindowsTimeZoneInfo(this string timeZoneId)
        {
            var ts = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            
            try
            { 
                if (ts.HasIanaId && TimeZoneInfo.TryConvertIanaIdToWindowsId(ts.Id, out var windowsTz))
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(windowsTz);
                }
            }
            catch 
            {
                // Not found?
            }

            return ts;
        }
        
    }
}
