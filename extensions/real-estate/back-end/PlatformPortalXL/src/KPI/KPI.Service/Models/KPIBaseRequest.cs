using System;
using System.Collections.Generic;
using Willow.Common;
using Willow.KPI.Repository;

namespace Willow.KPI.Service
{
    public class KPIBaseRequest
    {
        public DateTime? StartDate  { get; set; }
        public DateTime? EndDate    { get; set; }
		public string[] SelectedDayRange { get; set; }
		public string[] SelectedBusinessHourRange { get; set; }
        public string GroupBy { get; set; }
	}

	public static class KPIRequestExtensions
	{
		private static readonly Dictionary<string, object> _dayRangeLookup = new()
		{
			{ "allDays", RepositoryNullTypes.Boolean },
			{ "weekDays", true },
			{ "weekEnds", false }
		};

		private static readonly Dictionary<string, object> _businessHourRangeLookup = new()
		{
			{ "allHours", RepositoryNullTypes.Boolean },
			{ "inBusinessHours", true },
			{ "outBusinessHours", false }
		};

		public static IDictionary<string, object> ToDictionary(this KPIRequest kpiRequest)
		{
			var result = ObjectExtensions.ToDictionary(kpiRequest);

			if (result != null)
			{
				result.Remove(nameof(KPIRequest.SelectedDayRange));
				result.Add("isWeekDay", kpiRequest.SelectedDayRange.ToRange(_dayRangeLookup, RepositoryNullTypes.Boolean));

				result.Remove(nameof(KPIRequest.SelectedBusinessHourRange));
				result.Add("isBusinessHour", kpiRequest.SelectedBusinessHourRange.ToRange(_businessHourRangeLookup, RepositoryNullTypes.Boolean));
			}

			return result;
		}
	}

	public static class RangeExtensions
	{
		public static T ToRange<T>(this string[] keys, IDictionary<string, T> lookup, T defaultVal = default)
		{
			return (keys?.Length ?? 0) > 0 ? keys[0].ToRange(lookup, defaultVal) : defaultVal;
		}

		public static T ToRange<T>(this string key, IDictionary<string, T> lookup, T defaultVal = default)
		{
			return !string.IsNullOrEmpty(key) && lookup.ContainsKey(key) ? lookup[key] : defaultVal;
		}
	}
}
