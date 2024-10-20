using Abodit.Graph;
using Abodit.Mutable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using WillowRules.Extensions;
using WillowRules.Visitors;

namespace Willow.Rules.Services;

public partial class RulesService
{
	/// <summary>
	/// A base class for all paramter binding implementations
	/// </summary>
	private abstract class ParameterBinder
	{
		public abstract void AddToRuleInstance(RuleInstance ruleInstance, RuleParameterBound parameterBound);

		public virtual RuleParameterBound CreateParameter(
			Env env,
			RuleParameter parameter,
			TokenExpression twinVisitorExpression,
			TokenExpression rewrittenExpression,
			IEnumerable<BasicDigitalTwinPoco> referencedCapabilities,
			RuleInstanceStatus ruleInstanceStatus = RuleInstanceStatus.Valid)
		{
			return new RuleParameterBound(parameter.Name, rewrittenExpression, parameter.FieldId, rewrittenExpression.Unit, cumulativeSetting: parameter.CumulativeSetting, ruleInstanceStatus: ruleInstanceStatus);
		}

		public virtual bool RegisterReferencedCapabilities => true;

		/// <summary>
		/// Get all binders to be processed for the rule instance
		/// </summary>
		public static IEnumerable<(RuleParameter, ParameterBinder)> GetBinders(
			TwinDataContext twinContext,
			Graph<ModelData, Relation> ontology,
			Rule rule,
			ITwinSystemService twinSystemService,
			ILogger logger)
		{
			foreach (var parameter in rule.Parameters)
			{
				yield return (parameter, new CapabilityParameterBinder());
			}

			if (!rule.Parameters.Any(v => v.FieldId == Fields.Result.Id))
			{
				yield return (new RuleParameter(
					Fields.Result.Name,
					Fields.Result.Id,
					new TokenExpressionFailed(
						new TokenExpressionConstantString("Missing 'result' expression"))
					.Serialize()),
					new CapabilityParameterBinder());
			}

			foreach (var parameter in rule.ImpactScores)
			{
				yield return (parameter, new ImpactScoreParameterBinder());
			}

			foreach (var parameter in rule.Filters)
			{
				yield return (parameter, new RuleFilterParameterBinder(twinContext, rule, logger));
			}

			foreach (var trigger in rule.RuleTriggers)
			{
				var ruleTriggerBound = new RuleTriggerBound(trigger);

				yield return (trigger.Condition, new RuleTriggerConditionParameterBinder(ruleTriggerBound));
				yield return (trigger.Point, new RuleTriggerPointParameterBinder(ruleTriggerBound, ontology, twinSystemService));
				yield return (trigger.Value, new RuleTriggerValueParameterBinder(ruleTriggerBound));
			}
		}
	}

	/// <summary>
	/// Binds capability expressions
	/// </summary>
	private class CapabilityParameterBinder : ParameterBinder
	{
		public override void AddToRuleInstance(RuleInstance ruleInstance, RuleParameterBound parameterBound)
		{
			ruleInstance.RuleParametersBound.Add(parameterBound);
		}
	}

	/// <summary>
	/// Binds impact scores
	/// </summary>
	private class ImpactScoreParameterBinder : ParameterBinder
	{
		public override void AddToRuleInstance(RuleInstance ruleInstance, RuleParameterBound parameterBound)
		{
			ruleInstance.RuleImpactScoresBound.Add(parameterBound);
		}
	}

	/// <summary>
	/// Binds filters
	/// </summary>
	private class RuleFilterParameterBinder : ParameterBinder
	{
		private TwinDataContext context;
		private Rule rule;
		private ILogger logger;

		public RuleFilterParameterBinder(TwinDataContext context, Rule rule, ILogger logger)
		{
			this.context = context;
			this.rule = rule;
			this.logger = logger;
		}

		public override void AddToRuleInstance(RuleInstance ruleInstance, RuleParameterBound parameterBound)
		{
			ruleInstance.RuleFiltersBound.Add(parameterBound);
		}

		public override RuleParameterBound CreateParameter(
			Env env,
			RuleParameter parameter,
			TokenExpression twinVisitorExpression,
			TokenExpression rewrittenExpression,
			IEnumerable<BasicDigitalTwinPoco> referencedCapabilities,
			RuleInstanceStatus ruleInstanceStatus = RuleInstanceStatus.Valid)
		{
			if (ruleInstanceStatus == RuleInstanceStatus.Valid)
			{
				try
				{
					var result = rewrittenExpression.EvaluateDirectUsingEnv(env);

					if (result.Value.ToBoolean(null) == false)
					{
						ruleInstanceStatus = RuleInstanceStatus.FilterApplied;
					}
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Failed to apply filter to twin {twinId} for rule {ruleId}", context.Twin.Id, rule.Id);

					ruleInstanceStatus = RuleInstanceStatus.FilterFailed;
				}
			}

			return base.CreateParameter(env, parameter, twinVisitorExpression, rewrittenExpression, referencedCapabilities, ruleInstanceStatus: ruleInstanceStatus);
		}

		public override bool RegisterReferencedCapabilities => false;
	}

	/// <summary>
	/// Base class for binding trigger expressions
	/// </summary>
	private abstract class RuleTriggerParameterBinder : ParameterBinder
	{
		protected RuleTriggerBound ruleTriggerBound;

		protected RuleTriggerParameterBinder(RuleTriggerBound ruleTriggerBound)
		{
			this.ruleTriggerBound = ruleTriggerBound;
		}

		public override RuleParameterBound CreateParameter(
			Env env,
			RuleParameter parameter,
			TokenExpression twinVisitorExpression,
			TokenExpression rewrittenExpression,
			IEnumerable<BasicDigitalTwinPoco> referencedCapabilities,
			RuleInstanceStatus ruleInstanceStatus = RuleInstanceStatus.Valid)
		{
			var fieldId = $"{ruleTriggerBound.Id}_{parameter.FieldId}";

			var boundParameter = new RuleParameterBound(parameter.Name, rewrittenExpression, fieldId, rewrittenExpression.Unit, cumulativeSetting: parameter.CumulativeSetting, ruleInstanceStatus: ruleInstanceStatus);

			return boundParameter;
		}

		public override void AddToRuleInstance(RuleInstance ruleInstance, RuleParameterBound parameterBound)
		{
			if (!ruleInstance.RuleTriggersBound.Contains(ruleTriggerBound))
			{
				ruleInstance.RuleTriggersBound.Add(ruleTriggerBound);
			}
		}
	}

	/// <summary>
	/// Binds the trigger condition expression
	/// </summary>
	private class RuleTriggerConditionParameterBinder : RuleTriggerParameterBinder
	{
		public RuleTriggerConditionParameterBinder(RuleTriggerBound ruleTriggerBound)
			: base(ruleTriggerBound)
		{
		}

		public override RuleParameterBound CreateParameter(Env env,
			RuleParameter parameter,
			TokenExpression twinVisitorExpression,
			TokenExpression rewrittenExpression,
			IEnumerable<BasicDigitalTwinPoco> referencedCapabilities,
			RuleInstanceStatus ruleInstanceStatus = RuleInstanceStatus.Valid)
		{
			var boundParameter = base.CreateParameter(env, parameter, twinVisitorExpression, rewrittenExpression, referencedCapabilities, ruleInstanceStatus);

			ruleTriggerBound.Condition = boundParameter;

			return boundParameter;
		}
	}

	/// <summary>
	/// Binds the trigger value expression
	/// </summary>
	private class RuleTriggerValueParameterBinder : RuleTriggerParameterBinder
	{
		public RuleTriggerValueParameterBinder(RuleTriggerBound ruleTriggerBound)
			: base(ruleTriggerBound)
		{
		}

		public override RuleParameterBound CreateParameter(Env env,
			RuleParameter parameter,
			TokenExpression twinVisitorExpression,
			TokenExpression rewrittenExpression,
			IEnumerable<BasicDigitalTwinPoco> referencedCapabilities,
			RuleInstanceStatus ruleInstanceStatus = RuleInstanceStatus.Valid)
		{
			if (twinVisitorExpression is TokenExpressionArray)
			{
				ruleInstanceStatus = RuleInstanceStatus.BindingFailed;
				rewrittenExpression = new TokenExpressionFailed("Value cannot be an array", rewrittenExpression);
			}

			if (string.IsNullOrEmpty(parameter.Units))
			{
				//unit is required for integration call
				ruleInstanceStatus = RuleInstanceStatus.BindingFailed;
				rewrittenExpression = new TokenExpressionFailed("Units is required for the value expression", rewrittenExpression);
			}

			var boundParameter = base.CreateParameter(env, parameter, twinVisitorExpression, rewrittenExpression, referencedCapabilities, ruleInstanceStatus);

			ruleTriggerBound.Value = boundParameter;

			return boundParameter;
		}
	}

	/// <summary>
	/// Binds the trigger point expression
	/// </summary>
	private class RuleTriggerPointParameterBinder : RuleTriggerParameterBinder
	{
		private readonly Graph<ModelData, Relation> ontology;
		private readonly ITwinSystemService twinSystemService;

		public RuleTriggerPointParameterBinder(RuleTriggerBound ruleTriggerBound, Graph<ModelData, Relation> ontology, ITwinSystemService twinSystemService)
			: base(ruleTriggerBound)
		{
			this.ontology = ontology;
			this.twinSystemService = twinSystemService;
		}

		public override RuleParameterBound CreateParameter(
			Env env,
			RuleParameter parameter,
			TokenExpression twinVisitorExpression,
			TokenExpression rewrittenExpression,
			IEnumerable<BasicDigitalTwinPoco> referencedCapabilities,
			RuleInstanceStatus ruleInstanceStatus = RuleInstanceStatus.Valid)
		{
			var twin = (twinVisitorExpression as TokenExpressionTwin)?.Value;

			if (twinVisitorExpression is TokenExpressionVariableAccess variableAccess)
			{
				if (env.TryGet(variableAccess.VariableName, out TokenExpressionVariableAccess? twinIdAccess))
				{
					twin = referencedCapabilities.FirstOrDefault(v => v.Id == twinIdAccess!.VariableName);

					if (twin is not null)
					{
						//Re-assign to twin id expression. This is more for the UI when looking at binding outputs to see the twin id rather than the variable name
						rewrittenExpression = twinIdAccess!;
					}
				}
			}

			if (twin is null)
			{
				ruleInstanceStatus = RuleInstanceStatus.BindingFailed;
				rewrittenExpression = new TokenExpressionFailed("Expected a single twin", rewrittenExpression);
			}
			else
			{
				var modelIds = new string[]
				{
					"dtmi:com:willowinc:Setpoint;1",
					"dtmi:com:willowinc:Actuator;1",
				};

				bool inherits = modelIds.Any(modelId => ontology.IsAncestorOrEqualTo(twin.Metadata.ModelId, modelId));

				if (!inherits)
				{
					ruleInstanceStatus = RuleInstanceStatus.BindingFailed;
					rewrittenExpression = new TokenExpressionFailed($"Point must inherit from {string.Join(" or ", modelIds)}", rewrittenExpression);
				}
			}

			var boundParameter = base.CreateParameter(env, parameter, twinVisitorExpression, rewrittenExpression, referencedCapabilities, ruleInstanceStatus);

			if (twin is not null)
			{
				ruleTriggerBound.ExternalId = twin.externalID;
				ruleTriggerBound.ConnectorId = twin.connectorID;
				ruleTriggerBound.TwinId = twin.Id;
				ruleTriggerBound.TwinName = twin.name;

				var twinGraph = twinSystemService.GetTwinSystemGraph([twin.Id]).GetAwaiter().GetResult();

				var startNode = twinGraph.Nodes.FirstOrDefault(x => x.Id == twin.Id);

				if (startNode is not null)
				{
					var relationships = new List<RuleTriggerBoundRelationship>();
					var relTypes = new string[] { "isCapabilityOf", "hostedBy", "locatedIn", "isPartOf" };

					Graph<BasicDigitalTwinPoco, WillowRelation> nodes = twinGraph
						.Successors<BasicDigitalTwinPoco>(startNode, (s, r, e) => relTypes.Any(x => x == r.Name));

					foreach (var node in nodes.TopologicalSortApprox())
					{
						var edge = nodes.Edges.FirstOrDefault(v => v.End == node);

						if(edge.End is not null)
						{
							relationships.Add(new RuleTriggerBoundRelationship()
							{
								ModelId = edge.End.Metadata.ModelId,
								RelationshipType = edge.Predicate.Name,
								TwinId = edge.End.Id,
								TwinName = edge.End.name
							});
						}
					}

					ruleTriggerBound.Relationships = relationships;
				}
			}

			ruleTriggerBound.Point = boundParameter;

			return boundParameter;
		}
	}
}
