namespace WillowRules.DTO;

public static class TwinDtoExtensions
{
	/// <summary>
	/// Until building has fixes in it, these are heuristic workarounds
	/// </summary>
	public static TwinDto ApplyUnitHackFix(this TwinDto value)
	{
		value.Unit = UnitFixHack(value.Id, value.Name, value.Unit);

		return value;
	}

	private static string UnitFixHack(string twinId, string name, string unit)
	{
		return unit switch
		{
			_ when twinId.EndsWith("HAM_") => "hourandminute",
			_ when twinId.EndsWith("YEAR_") => "year",
			_ when twinId.EndsWith("DAM_") => "dayandmonth",
			_ when twinId.Contains("_FAULT_COUNTER_") => "Count",
			_ when twinId.Contains("_TRIPPOINT_") => "???",    // Not a bool, but could be ppm, temperature, ...
															   //_ when twinId.Contains("_WARNING_DISPLAY_") => "Count",  // not a fault
			_ when twinId.Contains("_WARNING_TOTAL_") => "Count",  // already a count
			_ when twinId.EndsWith("PeopleCountSensor-Total") => "people count (total)",
			_ when twinId.EndsWith("PeopleCountSensor-Unique") => "people count (unique)",

			null => "NO UNIT",

			_ => unit
		};
	}
}
