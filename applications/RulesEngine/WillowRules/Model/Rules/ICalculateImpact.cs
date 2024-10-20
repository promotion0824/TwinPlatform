using System;

namespace Willow.Rules.Model
{
	/// <summary>
	/// A rule template supplies functions for calculating impacts
	/// </summary>
	/// <remarks>
	/// These may be time based, or based on the min, max or average value, or both time and value based.
	/// For example, average x time might be degree-hours above setpoint
	/// 
	/// </remarks>
	public interface ICalculateImpact
	{
		/// <summary>
		/// Gets the cost impact
		/// </summary>
		double CostImpact(DateTimeOffset start, DateTimeOffset end, double minValue, double maxValue, double average);

		/// <summary>
		/// Gets the commfort impact
		/// </summary>
		double ComfortImpact(DateTimeOffset start, DateTimeOffset end, double minValue, double maxValue, double average);

		/// <summary>
		/// Gets the reliability impact
		/// </summary>
		double ReliabilityImpact(DateTimeOffset start, DateTimeOffset end, double minValue, double maxValue, double average);

		/// <summary>
		/// Gets a text description for this interval
		/// </summary>
		string Text(bool isValid, bool isFailed, DateTimeOffset start, DateTimeOffset end, double minValue, double maxValue, double average);
	}

}
