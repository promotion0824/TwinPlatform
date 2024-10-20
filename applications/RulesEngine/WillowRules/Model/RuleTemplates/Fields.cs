
using System;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// A registry of common fields
/// </summary>
public class Fields
{
	public static readonly IntegerField Count = new IntegerField("Count limit", "count");

	public static readonly IntegerField Hours = new IntegerField("Hours", "hours", "hours");

	public static readonly DoubleField OverHowManyHours = new DoubleField("Over how many hours", "over_hours", "hours");

	public static readonly IntegerField OverHowManyDays = new IntegerField("Over how many days", "over_days", "days");

	public static readonly PercentageField PercentageOfTime = new PercentageField("Percentage of time", "percentage_of_time");

	public static readonly PercentageField PercentageOfTimeOff = new PercentageField("Percentage of Time (off)", "percentage_of_time_off");

	public static readonly DoubleField MinTrigger = new DoubleField("Min trigger", "min_trigger", "F");
	public static readonly DoubleField MaxTrigger = new DoubleField("Max trigger", "max_trigger", "F");
	public static readonly DoubleField MinReset = new DoubleField("Min reset", "min_reset", "F");
	public static readonly DoubleField MaxReset = new DoubleField("Max reset", "max_reset", "F");

	public static readonly DoubleField HumidityHighThreshold = new DoubleField("Humidity over", "humidity_over_percentage", "%rh");
	public static readonly DoubleField HumidityLowThreshold = new DoubleField("Humidity under", "humidity_under_percentage", "%rh");

	public static readonly PercentageField SpeedThreshold = new PercentageField("Speed over", "speed_percentage");
	public static readonly PercentageField DamperPosition = new PercentageField("Damper position over", "damper_position");
	public static readonly PercentageField ValvePosition = new PercentageField("Valve position over", "valve_position");

	public static readonly DoubleField DeadBandFarenheit = new DoubleField("Dead band", "dead_band", "F");

	/// <summary>
	/// A single expression
	/// </summary>
	public static readonly ExpressionField Result = new ExpressionField("Expression", "result");

	/// <summary>
	/// A cooling coil value position binding
	/// </summary>
	public static readonly ExpressionField Capability_Cooling_Coil_Valve_Position = new ExpressionField("Cooling coil valve position binding", "capability_cooling_coil_valve_position");

	/// <summary>
	/// A discharge air temperature binding
	/// </summary>
	public static readonly ExpressionField Capability_Discharge_Air_Temperature = new ExpressionField("Discharge air temperature binding", "capability_discharge_air_temperature");

	/// <summary>
	/// A discharge air temperature setpoint
	/// </summary>
	public static readonly ExpressionField Capability_Discharge_Air_Setpoint = new ExpressionField("Discharge air setpoint binding", "capability_air_setpoint");

	/// <summary>
	/// A supply fan speed
	/// </summary>
	public static readonly ExpressionField Capability_Supply_Fan_Speed = new ExpressionField("Supply fan speed binding", "capability_supply_fan_speed");

	/// <summary>
	/// A rated fan airflow
	/// </summary>
	public static readonly ExpressionField Capability_Rated_Fan_Airflow = new ExpressionField("Rated fan airflow", "capability_rated_fan_airflow");

	/// <summary>
	/// Occupancy field "Occupied"
	/// </summary>
	public static readonly ExpressionField Occupied = new ExpressionField("Occupied", "occupied_expr");

	public static readonly ExpressionField Setpoint = new ExpressionField("Setpoint", "setpoint");
	public static readonly ExpressionField Sensor = new ExpressionField("Sensor", "sensor");

	/// <summary>
	/// Cost impact, an expression using "Occupied" and "Expression" or just a constant
	/// </summary>
	public static readonly ExpressionField CostImpact = new ExpressionField("Cost impact", "cost_impact");

	/// <summary>
	/// Comfort impact, an expression using "Occupied" and "Expression" or just a constant
	/// </summary>
	public static readonly ExpressionField ComfortImpact = new ExpressionField("Comfort impact", "comfort_impact");

	/// <summary>
	/// Reliability impact, an expression using "Occupied" and "Expression" or just a constant
	/// </summary>
	public static readonly ExpressionField ReliabilityImpact = new ExpressionField("Reliability impact", "reliability_impact")
	{
		Units = "%"
	};
}
