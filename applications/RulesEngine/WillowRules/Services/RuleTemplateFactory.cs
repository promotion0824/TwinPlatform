using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;

namespace Willow.Rules.Services;

/// <summary>
/// Maps rule instances to rule templates and rules
/// </summary>
public class RuleTemplateFactory
{
	private readonly RuleTemplateProducerBase[] ruleTemplates;
	private readonly IRepositoryRules repositoryRules;

	/// <summary>
	/// Maps rule id to a rule (likely 50+ of these)
	/// </summary>
	private Dictionary<string, Rule> rulesDictionary;

	private ConcurrentDictionary<string, RuleTemplate> cachedLookup = new ConcurrentDictionary<string, RuleTemplate>();

	/// <summary>
	/// Creates a new <see cref="RuleTemplateFactory" />
	/// </summary>
	/// <remarks>
	/// Must call initialize on it
	/// </remarks>
	public RuleTemplateFactory(IRepositoryRules repositoryRules, RuleTemplateRegistry ruleTemplateRegistry)
	{
		// Get the rule templates (TODO: Inject these intead)
		this.ruleTemplates = ruleTemplateRegistry.GetAll().ToArray();
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.rulesDictionary = new();
	}

	/// <summary>
	/// Initialize the template factory
	/// </summary>
	public async Task Initialize()
	{
		this.rulesDictionary = new();

		// Load all the rules ahead of time
		await foreach (var rule in this.repositoryRules.GetAll())
		{
			rulesDictionary[rule.Id] = rule;
		}
		// On startup with no rules
		//if (!rulesDictionary.Any()) throw new Exception("No rules to process");
	}

	/// <summary>
	/// Check that the rule still exists
	/// </summary>
	/// <remarks>
	/// May happen if a cleanup operation after deleting a rule misses deleting some
	/// </remarks>
	public bool CheckRuleForRuleInstanceStillExists(RuleInstance ruleInstance, ILogger logger)
	{
		if (rulesDictionary.TryGetValue(ruleInstance.RuleId, out var rule)) return true;
		if (ruleInstance.RuleTemplate == RuleTemplateCalculatedPoint.ID) return true;
		return false;
	}

	private static Lazy<RuleTemplateCalculatedPoint> cachedRuleTemplateForCalculatedPoints = new Lazy<RuleTemplateCalculatedPoint>(() => new RuleTemplateCalculatedPoint());

	/// <summary>
	/// Gets a populated template with the rule parameters
	/// </summary>
	public RuleTemplate? GetRuleTemplateForRuleInstance(RuleInstance ruleInstance, ILogger logger)
	{
		if (ruleInstance.RuleTemplate is null) return null;

		if (ruleInstance.RuleTemplate == RuleTemplateCalculatedPoint.ID)
		{
			// Calulated points all use the same rule instance because they have no parameters
			return cachedRuleTemplateForCalculatedPoints.Value;
		}

		var ruleTemplate = cachedLookup.GetOrAdd(ruleInstance.RuleId, (k) =>
		{
			// Get the rule from the rule instance
			if (!rulesDictionary.TryGetValue(ruleInstance.RuleId, out var rule))
			{
				// No such rule
				// TODO: Log warning for missing rules
				return null!;
			}

			var template = ruleTemplates
				.Single(x => x.Id == ruleInstance.RuleTemplate)
				.Factory(rule.Elements);

			return template;
		});

		return ruleTemplate;
	}
}
