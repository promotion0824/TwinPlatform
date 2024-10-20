using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Model.RuleTemplates;

namespace Willow.Rules.Model
{
	/// <summary>
	/// Extension methods for <see cref="ActorState"/>
	/// </summary>
	public static class ActorStateExtensions
	{
		/// <summary>
		/// Creates an external id to be sent to ADX per impact score
		/// </summary>
		public static string GenerateExternalId(this ActorState actor, RuleParameterBound scoreParam)
		{
			if(actor.Version > 0)
			{
				return $"{actor.Id}_{scoreParam.FieldId}_V{actor.Version}";
			}

			return $"{actor.Id}_{scoreParam.FieldId}";
		}

		/// <summary>
		/// Gets commands for the actor and rule instance
		/// </summary>
		public static IEnumerable<Command> CreateCommands(this ActorState actor, RuleInstance ruleInstance)
		{
			foreach (var trigger in ruleInstance.RuleTriggersBound)
			{
				yield return new Command(trigger, ruleInstance, actor);
			}
		}

		/// <summary>
		/// Removes command outputs not in rule instance anymore
		/// </summary>
		public static void RemoveOldCommandOutputs(this ActorState actor, RuleInstance ruleInstance)
		{
			foreach (var key in actor.OutputValues.Commands.Keys.ToList())
			{
				if (!ruleInstance.RuleTriggersBound.Any(v => v.Id == key))
				{
					actor.OutputValues.Commands.Remove(key);
				}
			}
		}

		/// <summary>
		/// Populates an environment with the latest value for each named point
		/// </summary>
		public static Env RecentValues(this ActorState state, Env env, RuleInstance ruleInstance, IRuleTemplateDependencies dependencies)
		{
			var addVariable = (string key, TimedValue tpv, string? units) =>
			{
				if (tpv.ValueDouble is double d)
				{
					if(!string.IsNullOrEmpty(tpv.ValueText))
					{
						env.Assign(key, new EnvValue(d, tpv.ValueText), units);
					}
					else
					{
						env.Assign(key, d, units);
					}
				}
				else if (tpv.ValueBool is bool b)
				{
					if (!string.IsNullOrEmpty(tpv.ValueText))
					{
						env.Assign(key, new EnvValue(b, tpv.ValueText), units);
					}
					else
					{
						env.Assign(key, b, units);
					}
				}
				else if (!string.IsNullOrEmpty(tpv.ValueText))
				{
					env.Assign(key, tpv.ValueText, units);
				}
			};

			foreach ((var key, var timeSeries) in state.TimedValues.Where(v => v.Value.Count > 0))
			{
				addVariable(key, timeSeries.Last(), timeSeries.UnitOfMeasure);
			}

			foreach (var point in ruleInstance.PointEntityIds)
			{
				if (dependencies.TryGetTimeSeriesByTwinId(point.Id, out var timeSeries))
				{
					if (timeSeries!.Points.Any())
					{
						var value = timeSeries!.Last();
						addVariable(point.Id, value, point.Unit);
					}
				}
			}

			return env;
		}

		/// <summary>
		/// If the gap between newValue and the list is more than validLimit, starts a whole new sequence
		/// removes anything older than the prune value
		/// </summary>
		/// <remarks>
		/// This is to handle missing data
		/// </remarks>
		public static TimeSeriesBuffer PruneAndCheckValid(
			Dictionary<string, TimeSeriesBuffer> sequence,
			in TimedValue newValue,
			string name,
			string unit,
			bool applyCompression = true,
			bool optimizeCompression = true,
			double? compression = null)
		{
			// Start a new list of these if there isn't one already
			if (!sequence.TryGetValue(name, out var values))
			{
				values = new TimeSeriesBuffer()
				{
					UnitOfMeasure = unit
				};

				sequence[name] = values;
			}

			values.UnitOfMeasure = !string.IsNullOrEmpty(unit) ? unit : values.UnitOfMeasure; // TEMP

			// Add point with trajectory compression
			values.AddPoint(newValue, applyCompression: applyCompression, reApplyCompression: optimizeCompression, compression: compression);

			if (!values.Points.Any())
			{
				//remove list if empty
				sequence.Remove(name);
			}

			return values;
		}

		/// <summary>
		/// Finds the first index for the given predicate
		/// </summary>
		public static int IndexOf<T>(this IList<T> value, Func<T, bool> predicate)
		{
			var index = -1;

			for (var i = 0; i < value.Count - 1; i++)
			{
				if (predicate(value[i]))
				{
					index = i;
					break;
				}
			}

			return index;
		}

		/// <summary>
		/// Removes all items for a given predicate
		/// </summary>
		public static int RemoveAll<T>(this IList<T> value, Func<T, bool> predicate)
		{
			int count = 0;

			for (var i = value.Count - 1; i >= 0; i--)
			{
				if (predicate(value[i]))
				{
					count++;
					value.RemoveAt(i);
				}
			}

			return count;
		}
	}
}
