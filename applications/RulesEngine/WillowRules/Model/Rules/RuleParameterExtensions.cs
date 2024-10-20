using System.Text.RegularExpressions;

namespace Willow.Rules.Model;

/// <summary>
/// Extensions on <see cref="RuleParameter"/>
/// </summary>
public static class RuleParameterExtensions
{
	/// <summary>
	/// Checks if a point expression contains a given variable name
	/// </summary>
	/// <returns></returns>
	public static bool MatchVariableName(this RuleParameter parameter, string variableName)
	{
		string regex = $"\\b{variableName}\\b";
		string expression = parameter.PointExpression;
		return Regex.IsMatch(expression, regex);
	}
}
