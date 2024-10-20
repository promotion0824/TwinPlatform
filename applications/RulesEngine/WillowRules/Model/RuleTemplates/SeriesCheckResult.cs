namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// A result specifying if there is enough data to evaluate the rule 
/// </summary>
public enum SeriesCheckVolume
{
	/// <summary>
	/// Not enough data yet
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Enough data to evaluate the rule
	/// </summary>
	Enough = 1
}

/// <summary>
/// The result of checking a boolean-valued time series against a TimeTemplate
/// </summary>
public enum SeriesCheckResult
{
	/// <summary>
	/// Faulty
	/// </summary>
	Fault = 1,

	/// <summary>
	/// Healthy
	/// </summary>
	Healthy = 2
}
