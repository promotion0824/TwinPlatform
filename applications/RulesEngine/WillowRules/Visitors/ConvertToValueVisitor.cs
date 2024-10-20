using System;
using System.Collections.Generic;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Visitors;

/// <summary>
/// A Rules engine convert value vistior that extends the willow lib one
/// </summary>
internal class ConvertToValueVisitor : Willow.Expressions.Visitor.ConvertToValueVisitor<Env>
{
	private readonly IRuleTemplateDependencies dependencies;
	private readonly Func<string, ITemporalObject?> temporalObjectGetter;
	private readonly Func<string, IMLRuntime?> mlModelGetter;
	private readonly Env source;
	private readonly Func<TimeSeries, bool> isValidTimeseries;
	private readonly double allowedTolerance;
	private readonly Func<Env, string, object> variableGetter;

	private static IConvertible InvalidCapabilityResult => double.NaN;

	/// <summary>
	/// Creates a new instance of the <see cref="ConvertToValueVisitor"/> class
	/// </summary>
	public ConvertToValueVisitor(
		Env source,
		Func<Env, string, object> variableGetter,
		Func<string, ITemporalObject?> temporalObjectGetter,
		Func<string, IMLRuntime?> mlModelGetter,
		IRuleTemplateDependencies dependencies,
		Func<TimeSeries, bool> isValidTimeseries,
		double allowedTolerance = 1)
		: base(source, variableGetter, temporalObjectGetter, mlModelGetter)
	{
		this.source = source;
		this.variableGetter = variableGetter;
		this.dependencies = dependencies;
		this.isValidTimeseries = isValidTimeseries;
		this.allowedTolerance = allowedTolerance;
		this.temporalObjectGetter = temporalObjectGetter;
		this.mlModelGetter = mlModelGetter;
	}

	public bool InvalidCapability { get; set; }

	private bool HasTolerance => allowedTolerance < 1;

	public override IConvertible DoVisit(TokenExpressionAverage input)
	{
		if (HasTolerance)
		{
			var expression = WithTolerance(input, input.Child, (children) => new TokenExpressionAverage(children)
			{
				Unit = input.Unit,
			});

			if (InvalidCapability)
			{
				return InvalidCapabilityResult;
			}

			return base.DoVisit(expression);
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionSum input)
	{
		if (HasTolerance)
		{
			var expression = WithTolerance(input, input.Child, (children) => new TokenExpressionSum(children)
			{
				Unit = input.Unit,
			});

			if (InvalidCapability)
			{
				return InvalidCapabilityResult;
			}

			return base.DoVisit(expression);
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionMax input)
	{
		if (HasTolerance)
		{
			var expression = WithTolerance(input, input.Child, (children) => new TokenExpressionMax(children)
			{
				Unit = input.Unit,
			});

			if (InvalidCapability)
			{
				return InvalidCapabilityResult;
			}

			return base.DoVisit(expression);
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionMin input)
	{
		if (HasTolerance)
		{
			var expression = WithTolerance(input, input.Child, (children) => new TokenExpressionMin(children)
			{
				Unit = input.Unit,
			});

			if (InvalidCapability)
			{
				return InvalidCapabilityResult;
			}

			return base.DoVisit(expression);
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionAll input)
	{
		if (HasTolerance)
		{
			var expression = WithTolerance(input, input.Child, (children) => new TokenExpressionAll(children)
			{
				Unit = input.Unit,
			});

			if (InvalidCapability)
			{
				return InvalidCapabilityResult;
			}

			return base.DoVisit(expression);
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionCount input)
	{
		if (HasTolerance)
		{
			var expression = WithTolerance(input, input.Child, (children) => new TokenExpressionCount(children)
			{
				Unit = input.Unit,
			});

			if (InvalidCapability)
			{
				return InvalidCapabilityResult;
			}

			return base.DoVisit(expression);
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionVariableAccess input)
	{
		if (!IsValidTimeSeries(input.VariableName))
		{
			//exit out immediately when finding invalid buffers
			InvalidCapability = true;

			return InvalidCapabilityResult;
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionFunctionCall input)
	{
		if (input.FunctionName == "TOLERANCE")
		{
			if (input.Children.Length == 2)
			{
				var expression = input.Children[0];

				double toleranceValue = Visit(input.Children[1]).ToDouble(null);

				toleranceValue = Math.Min(1, Math.Abs(toleranceValue));

				var visitor = RecurseInto(tolerance: toleranceValue);

				var result = expression.Accept(visitor);

				InvalidCapability = InvalidCapability || visitor.InvalidCapability;

				return result;
			}
		}
		else if (input.FunctionName == "TOLERANTOPTION")
		{
			foreach (var child in input.Children)
			{
				var visitor = RecurseInto();

				var result = child.Accept(visitor);

				if (!visitor.InvalidCapability)
				{
					return result;
				}
			}

			InvalidCapability = true;

			return InvalidCapabilityResult;
		}

		return base.DoVisit(input);
	}

	public override IConvertible DoVisit(TokenExpressionTemporal input)
	{
		if (input.FunctionName == "STND")
		{
			if (HasTolerance)
			{
				var expression = WithTolerance(input, input.Child, (children) => new TokenExpressionTemporal(input.FunctionName, children, timePeriod: input.TimePeriod, timeFrom: input.TimeFrom, unitOfMeasure: input.UnitOfMeasure)
				{
					Unit = input.Unit,
				});

				if (InvalidCapability)
				{
					return InvalidCapabilityResult;
				}

				return base.DoVisit(expression);
			}
		}

		return base.DoVisit(input);
	}

	private bool IsValidTimeSeries(string varibale)
	{
		if (dependencies.CapabilityTwinExistsInRule(varibale))
		{
			if (dependencies.TryGetTimeSeriesByTwinId(varibale, out var ts))
			{
				return isValidTimeseries(ts!);
			}

			return false;
		}
		
		return true;
	}

	private ConvertToValueVisitor RecurseInto(double tolerance = 1)
	{
		return new ConvertToValueVisitor(
					source,
					variableGetter,
					temporalObjectGetter,
					mlModelGetter,
					dependencies,
					isValidTimeseries,
					tolerance);
	}

	/// <summary>
	/// For tolerance only a certain percentage of capabilities need to be valid
	/// </summary>
	private T WithTolerance<T>(T expression, TokenExpression childExpression, Func<TokenExpressionArray, T> create)
		where T : TokenExpression
	{
		if (childExpression is TokenExpressionArray array)
		{
			var validChildren = new List<TokenExpression>();

			foreach (var child in array.Children)
			{
				bool ok = true;

				if (child is TokenExpressionVariableAccess variableAccess)
				{
					ok = IsValidTimeSeries(variableAccess.VariableName);
				}

				if (ok)
				{
					validChildren.Add(child);
				}
			}

			double invalidCount = array.Children.Length - validChildren.Count;

			if (invalidCount > 0)
			{
				double tolerantCount = array.Children.Length - array.Children.Length * allowedTolerance;

				if (invalidCount > tolerantCount)
				{
					//over tolerance, cant execute
					InvalidCapability = true;
				}
				else
				{
					if(validChildren.Count == 0)
					{
						//return Nan for this scenario which could potentially be handled using IFNAN to continue execution
						return create(new TokenExpressionArray([TokenExpressionConstant.Create(double.NaN)]));
					}

					return create(new TokenExpressionArray(validChildren.ToArray()));
				}
			}
		}

		return expression;
	}
}
