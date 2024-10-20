
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.ExpressionParser;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace Willow.Rules.Services;

/// <summary>
/// Manages the RuleInstances database
/// </summary>
public interface IRuleInstancesService
{
	/// <summary>
	/// Creates tuples of rule + metadata from rules, adding new metadata where necessary
	/// </summary>
	Task<IEnumerable<(Rule rule, RuleMetadata metadata)>> PrepareRulesAndMetadataForScan(bool force, string? ruleId = null, string? templateId = null);
}

/// <summary>
/// Rule instances database manager service
/// </summary>
public class RuleInstancesService : IRuleInstancesService
{
	private readonly ILogger<IRuleInstancesService> logger;
	private readonly IRepositoryRuleMetadata repositoryRuleMetadata;
	private readonly IRepositoryRules repositoryRules;

	/// <summary>
	/// Creates a new <see cref="RuleInstancesService"/>
	/// </summary>
	public RuleInstancesService(ILogger<IRuleInstancesService> logger,
		IRepositoryRuleMetadata repositoryRuleMetadata,
		IRepositoryRules repositoryRules)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.repositoryRuleMetadata = repositoryRuleMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleMetadata));
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
	}

	public async Task<IEnumerable<(Rule rule, RuleMetadata metadata)>> PrepareRulesAndMetadataForScan(bool force, string? ruleId = null, string? templateId = null)
	{
		List<(Rule rule, RuleMetadata metadata)> results = new();
		var count = 0;
		try
		{
			var rulesLookup = !string.IsNullOrWhiteSpace(templateId) ?
				this.repositoryRules.GetAll(r => r.TemplateId == templateId) : this.repositoryRules.GetAll();

			await foreach (var rule in rulesLookup)
			{
				count++;

				//Exclude rule for expansion if the Primary Model is null
				if (string.IsNullOrWhiteSpace(rule.PrimaryModelId))
				{
					logger.LogWarning("The rule {ruleId} does not contain a primary model id", rule.Id);
					continue;
				}

				// inefficient for one rule but it's not worth optimizing for just ~100 rules
				if (!string.IsNullOrEmpty(ruleId) && rule.Id != ruleId) continue;

				var ruleMetadata = await this.repositoryRuleMetadata.GetOrAdd(rule.Id);

				if (rule.IsDraft) continue;

				// Check that each rule can parse all the expressions and is also not in draft state before passing it along
				// otherwise we get a lot of expressions further down the pipeline
				try
				{
					ruleMetadata.ScanStarting();

					foreach (var p in rule.Parameters.Select(p => p.PointExpression))
					{
						var expr = Parser.Deserialize(p);
					}
					foreach (var p in rule.ImpactScores.Select(p => p.PointExpression))
					{
						var expr = Parser.Deserialize(p);
					}
					foreach (var p in rule.Filters.Select(p => p.PointExpression))
					{
						var expr = Parser.Deserialize(p);
					}
				}
				catch (ParserException ex)
				{
					//Dont stop expansion for the rule. Existing rule instances
					//should go to BindingFailed status so stats are reported properly
					ruleMetadata.ScanError = $"Failed to parse expressions. {ex.Message}";
					await repositoryRuleMetadata.UpsertOne(ruleMetadata, updateCache: false);
					logger.LogWarning("Failed to parse expressions for rule {ruleId}. {message}", rule.Id, ex.Message);
				}
				await repositoryRuleMetadata.QueueWrite(ruleMetadata);

				results.Add((rule, ruleMetadata));

				logger.LogInformation("Loaded rule {ruleId}", rule.Id);
			}

			await repositoryRuleMetadata.FlushQueue();

			logger.LogInformation("Loaded {total}/{count} rules and metadata for scan", results.Count, count);

			return results;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"PrepareRulesAndMetadaForScan failed. {ex.Message}");
			throw;
		}
	}
}
