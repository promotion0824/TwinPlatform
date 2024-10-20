using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Willow.Expressions;
using WillowRules.Extensions;

// POCO class, serialized to DB
#nullable disable

namespace Willow.Rules.Model;

public static class OutputValueExtensions
{
	// Define the pattern to match FAULTY and NONFAULTY sections
	private static readonly Regex FunctionRegex = new(@"(?<status>FAULTYTEXT|NONFAULTYTEXT)\((?<content>((?>[^)(]+|(?<o>)\(|(?<-o>\)))*))\)", RegexOptions.IgnoreCase);

	public static (string text, Dictionary<string, object> variables) ParseText(this OutputValue output, string value, ActorState actor, Env env)
	{
		var variables = new Dictionary<string, object>();

		if (string.IsNullOrEmpty(value)) { return (value, variables); }

		var isFaulty = output.Faulted;

		// Use Regex.Replace to replace FAULTY() with NONFAULTY() and vice versa
		value = FunctionRegex.Replace(value, match =>
		{
			// Extract status and content groups
			var statusText = match.Groups["status"].Value;
			var content = match.Groups["content"].Value;

			if(!output.IsValid)
			{
				return string.Empty;
			}

			// Determine whether to replace or remove based on the status check
			var replacement =
				(isFaulty && string.Equals(statusText, "FAULTYTEXT", StringComparison.OrdinalIgnoreCase)) ||
				(!isFaulty && string.Equals(statusText, "NONFAULTYTEXT", StringComparison.OrdinalIgnoreCase))
				? content : string.Empty;

			return replacement;
		});

		foreach (var placeholder in StringExtensions.ExtractExpressionsFromText(value))
		{
			var boundValue = env.GetBoundValue(placeholder);

			if (boundValue is not null)
			{
				var unit = actor.TimedValues.GetValueOrDefault(placeholder)?.UnitOfMeasure;

				var variable = boundValue.Value.Value;

				variables[placeholder] = variable;

				string textValue = "";

				if(variable is IConvertible convertible)
				{
					if(convertible.GetTypeCode() == TypeCode.DateTime)
					{
						textValue = convertible.ToDateTime(null).ToString("yyyy-MM-ddTHH:mm:ss");
					}
					else
					{
						textValue = $"{variable:0.00}";
					}					
				}
				else
				{
					textValue = variable?.ToString();
				}

				value = StringExtensions.ReplaceExpressionsFromText(value, placeholder, $"{textValue} {unit}".Trim());
			}
		}

		//Cleanup unwanted white spaces due to manipulation
		return (Regex.Replace(value, @"\s+", " ").Replace(" .", ".").Trim(), variables);
	}

	/// <summary>
	/// Render an output value as an insight occurrence
	/// </summary>
	public static string GetOutputText(this OutputValue x, ActorState actor, string value)
	{
		var env = Env.Empty.Push();

		if (x.Variables is not null)
		{
			foreach ((var key, var variable) in x.Variables)
			{
				env.Assign(key, variable);
			}
		}

		var (text, _) = x.ParseText(value, actor, env);

		return text;
	}

	/// <summary>
	/// Render an output value as an insight occurrence
	/// </summary>
	public static InsightOccurrence ToInsightOccurrence(this OutputValue x, Insight insight, ActorState actor, RuleInstance ruleInstance)
	{
		string text = x.IsValid ? x.GetOutputText(actor, ruleInstance.Description) : x.Text;
		return new InsightOccurrence(insight, x.IsValid, x.Faulted, x.StartTime, x.EndTime, text ?? "");
	}

	/// <summary>
	/// Render an output value as a command occurrence
	/// </summary>
	public static CommandOccurrence ToCommandOccurrence(this CommandOutputValue x)
	{
		return new CommandOccurrence(x.Triggered, x.Value, x.StartTime, x.EndTime, x.TriggerStartTime, x.TriggerEndTime);
	}

	/// <summary>
	/// Render output values as insight occurrences
	/// </summary>
	public static IEnumerable<InsightOccurrence> ToInsightOccurrences(this IEnumerable<OutputValue> outputValues, Insight insight, ActorState actor, RuleInstance ruleInstance)
	{
		int count = outputValues.Count();
		return outputValues
			.Select((x, i) => x.ToInsightOccurrence(insight, actor, ruleInstance));
	}

	/// <summary>
	/// Render output values as command occurrences
	/// </summary>
	public static IEnumerable<CommandOccurrence> ToCommandOccurrences(this IEnumerable<CommandOutputValue> outputValues, int maxItems)
	{
		int count = outputValues.Count();
		int startIndex = count <= maxItems ? 0 : (count - maxItems);

		return outputValues
			.SkipWhile((v, i) => i < startIndex)
			.Select((x, i) => x.ToCommandOccurrence());
	}
}
