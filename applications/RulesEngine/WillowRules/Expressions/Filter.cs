using Willow.Expressions;

namespace Willow.Rules;


/// <summary>
/// A filter applies to a Twin and filters twins and models according to what the user can see
/// or selects equipment for a rule application
/// </summary>
/// <example>
/// MS-PS-121 OR MS-PS-122
/// </example>
/// <example>
/// HVACEquipment
/// </example>
/// <example>
/// (MS-PS-121 OR MS-PS-122) AND NOT SecurityEquipment
/// </example>
/// <remarks>
/// </remarks>
public class TwinFilterExpression
{
	/// <summary>
	/// A TokenExpression for the filter
	/// </summary>
	public TokenExpression Expression { get; }

	public TwinFilterExpression(TokenExpression expression)
	{
		this.Expression = expression;
	}
}
