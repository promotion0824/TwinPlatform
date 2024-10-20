using System;
using System.Linq;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using WillowRules.Extensions;

namespace WillowRules.Visitors;

/// <summary>
/// A visitor to bind a text blob
/// </summary>
internal class BindTwinToTextVisitor : TokenExpressionVisitor
{
	private readonly BindToTwinsVisitor twinsVisitor;
	private readonly Env env;
	public bool Success { get; private set; }

	public BindTwinToTextVisitor(BindToTwinsVisitor twinsVisitor, Env env)
	{
		this.twinsVisitor = twinsVisitor ?? throw new ArgumentNullException(nameof(twinsVisitor));
		this.env = env ?? throw new ArgumentNullException(nameof(env));
	}

	public override TokenExpression DoVisit(TokenExpressionVariableAccess input)
	{
		//env variables are param field ids, ignore them
		if(env.Variables.Any(v => string.Equals(v, input.VariableName, StringComparison.OrdinalIgnoreCase)))
		{
			return input;
		}

		return HandleResult(twinsVisitor.DoVisit(input));
	}

	public override TokenExpression DoVisit(TokenExpressionPropertyAccess input)
	{
		return HandleResult(twinsVisitor.DoVisit(input));
	}

	public override TokenExpression DoVisit(TokenExpressionFunctionCall input)
	{
		if (input.FunctionName.Equals("OPTION", StringComparison.InvariantCultureIgnoreCase))
		{
			return HandleResult(twinsVisitor.DoVisit(input));
		}

		return new TokenExpressionFailed("Only the OPTION function is allowed", input);
	}

	private TokenExpression HandleResult(TokenExpression result)
	{
		Success &= twinsVisitor.Success;
		return result;
	}

	public static string ReplaceExpressionsInText(string text, Env env, BindToTwinsVisitor twinsVisitor)
	{
		var expressions = StringExtensions.ExtractExpressionsFromText(text);

		string result = text;

		foreach (var expression in expressions)
		{
			var tokenExpression = Parser.Deserialize(expression);

			var visitor = new BindTwinToTextVisitor(twinsVisitor, env);

			var expressionResult = visitor.Visit(tokenExpression);

			if (expressionResult is TokenExpressionVariableAccess)
			{
				if(env.TryGet<TokenExpressionConstantString>(expression, out var stringConstant))
				{
					result = StringExtensions.ReplaceExpressionsFromText(result, expression, stringConstant!.ValueString);
				}

				//variable access we assume will be from env during execution
				continue;
			}

			if(expressionResult is TokenExpressionConstant tokenExpressionConstant)
			{
				//this should be a twin propery constant
				result = StringExtensions.ReplaceExpressionsFromText(result, expression, tokenExpressionConstant.Value.ToString(null));
			}
		}

		return result;
	}
}
