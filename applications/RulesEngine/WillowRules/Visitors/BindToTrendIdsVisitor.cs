using System;
using System.Collections.Generic;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace WillowRules.Visitors;

/// <summary>
/// Converts an expression using TokenExpressionTwins to one using only twin Ids
/// </summary>
public class BindToTrendIdsVisitor : TokenExpressionVisitor
{
	private readonly List<BasicDigitalTwinPoco> mappingVariableToTrendId = new();
	private readonly IModelService modelService;

	/// <summary>
	/// This is used to pass the mappings out to the caller
	/// </summary>
	public IList<BasicDigitalTwinPoco> Mapping => this.mappingVariableToTrendId;

	/// <summary>
	/// Was the binding successful
	/// </summary>
	/// <remarks>
	/// All twins were converted to trendIds
	/// </remarks>
	public bool Success { get; private set; } = true;

	/// <summary>
	/// Creates a new <see cref="BindToTrendIdsVisitor"/>
	/// </summary>
	public BindToTrendIdsVisitor(IModelService modelService)
	{
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
	}

	//if we fail here, the converttovaluevisitor cant use expressions
	//public override TokenExpression DoVisit(TokenExpressionPropertyAccess input)
	//{
	//	// By this point all property access should have been converted to constant values
	//	Success = false;
	//	//return new TokenExpressionFailed("Property access should be constant by now", input);
	//	return input;
	//}
	public override TokenExpression DoVisit(TokenExpressionVariableAccess input)
	{
		// These are still allowed but they should refer to global variables
		return input;
	}

	public override TokenExpression DoVisit(TokenExpressionWrapped input)
	{
		if (input is TokenExpressionTwin twinExpression)
		{
			//has to be a capability twin
			if (IsTelemetry(twinExpression.Value))
			{
				var twin = twinExpression.Value;
				// Twins don't need trendIds any more

				var expr = new TokenExpressionVariableAccess(twin.Id, typeof(double));

				expr.Unit = twin.unit;

				this.mappingVariableToTrendId.Add(twin);

				return expr;
			}
		}
		else if (input is TokenExpressionJsObject)
		{
			return new TokenExpressionFailed("FAILED", input);
		}
		return base.DoVisit(input);
	}

	private bool IsTelemetry(BasicDigitalTwinPoco twin)
	{
		return modelService.IsCapability(twin.Metadata.ModelId) ||
			modelService.IsTextBasedTelemetry(twin.Metadata.ModelId);
	}
}
