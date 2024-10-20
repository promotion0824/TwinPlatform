using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace Willow.Rules.Processor;

/// <summary>
/// Manages rules for execution
/// </summary>
public interface IRulesManager
{
	/// <summary>
	/// Checks whether rule instances exist
	/// </summary>
	Task<bool> HasRuleInstances();

	/// <summary>
	/// Flush metadata to db
	/// </summary>
	Task FlushMetadataToDatabase(
		Dictionary<string, Rule> rules,
		Dictionary<string, List<RuleInstance>> ruleInstanceLookup,
		ConcurrentDictionary<string, ActorState> actors);

	/// <summary>
	/// Get all rules
	/// </summary>
	Task<Dictionary<string, Rule>> GetRulesLookup(RuleExecutionRequest request);

	/// <summary>
	/// Get Version for rule instance
	/// </summary>
	int GetVersion(RuleInstance ruleInstance);

	/// <summary>
	/// Get rule instance metadata
	/// </summary>
	Task LoadMetadata(
		RuleExecutionRequest request,
		bool incrementVersions);

	/// <summary>
	/// Get rule instances
	/// </summary>
	Task<Dictionary<string, List<RuleInstance>>> GetRuleInstanceLookup(
		RuleExecutionRequest request,
		ProgressTrackerForRuleExecution progressTracker,
		RuleTemplateFactory ruleTemplateFactory,
		SystemSummary summary,
		string noTwinId);
}

/// <summary>
/// Manages rules for execution
/// </summary>
public class RulesManager : IRulesManager
{
	private readonly IRepositoryRules repositoryRules;
	private readonly IRepositoryRuleInstances repositoryRuleInstances;
	private readonly IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata;
	private readonly IRepositoryRuleMetadata repositoryRuleMetadata;
	private readonly ITimeSeriesManager timeSeriesManager;
	private readonly Dictionary<string, int> metadataVersions = new();
	private ILogger<RulesManager> logger;

	/// <summary>
	/// Constructor
	/// </summary>
	public RulesManager(
		IRepositoryRules repositoryRules,
		IRepositoryRuleInstances repositoryRuleInstances,
		IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata,
		IRepositoryRuleMetadata repositoryRuleMetadata,
		ITimeSeriesManager timeSeriesManager,
		ILogger<RulesManager> logger)
	{
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
		this.repositoryRuleInstanceMetadata = repositoryRuleInstanceMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleInstanceMetadata));
		this.repositoryRuleMetadata = repositoryRuleMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleMetadata));
		this.timeSeriesManager = timeSeriesManager ?? throw new ArgumentNullException(nameof(timeSeriesManager));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Checks whether rule instances exist
	/// </summary>
	public Task<bool> HasRuleInstances()
	{
		return repositoryRuleInstances.Any(x => true);
	}

	/// <summary>
	/// Flush metadata to db
	/// </summary>
	public async Task FlushMetadataToDatabase(
		Dictionary<string, Rule> rules,
		Dictionary<string, List<RuleInstance>> ruleInstanceLookup,
		ConcurrentDictionary<string, ActorState> actors)
	{
		try
		{
			using (var timedLogger = logger.TimeOperation("Flush metadata to database"))
			{
				var ruleInstanceMetadataConfig = new BulkConfig()
				{
					PropertiesToIncludeOnUpdate = new List<string>()
					{
						nameof(RuleInstanceMetadata.TriggerCount),
						nameof(RuleInstanceMetadata.Version),
						nameof(RuleInstanceMetadata.LastTriggered),
					}
				};

				foreach (var ruleInstance in ruleInstanceLookup.SelectMany(v => v.Value).Distinct())
				{
					if(actors.TryGetValue(ruleInstance.Id, out var actor))
					{
						var metadata = new RuleInstanceMetadata()
						{
							Id = ruleInstance.Id,
							TriggerCount = actor.TriggerCount,
							Version = actor.Version,
							LastTriggered = actor.Timestamp,
						};

						await this.repositoryRuleInstanceMetadata.QueueWrite(metadata, config: ruleInstanceMetadataConfig, updateCache: false);
					}
				}

				await this.repositoryRuleInstanceMetadata.FlushQueue(config: ruleInstanceMetadataConfig, updateCache: false);

				await this.repositoryRuleInstanceMetadata.DeleteOrphanMetadata();

				var ruleInstances = ruleInstanceLookup.SelectMany(v => v.Value).Distinct().GroupBy(v => v.RuleId).ToDictionary(v => v.Key, v => v);

				foreach (var rule in rules.Values)
				{
					var metadata = await this.repositoryRuleMetadata.GetOrAdd(rule.Id);

					//dont update rules that hasn't done rule expansion yet
					if (metadata.ScanState != ScanState.Unknown)
					{
						int count = 0;

						if (ruleInstances.TryGetValue(rule.Id, out var instances))
						{
							foreach (var ruleInstance in instances)
							{
								if (actors.TryGetValue(ruleInstance.Id, out var actor))
								{
									if (actor.OutputValues.Faulted)
									{
										count++;
									}
								}
							}
						}

						metadata.InsightsGenerated = count;

						//await ruleMetadataService.UpdateMetadata(willowEnvironment, metadata);
						await this.repositoryRuleMetadata.QueueWrite(metadata, updateCache: false);
					}
				}

				await this.repositoryRuleMetadata.FlushQueue(updateCache: false);
			}

			logger.LogInformation($"Completed rule metadata update");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to flush metadata to database");
		}
	}

	/// <summary>
	/// Get Version for rule instance
	/// </summary>
	public int GetVersion(RuleInstance ruleInstance)
	{
		return metadataVersions.GetValueOrDefault(ruleInstance.RuleId);
	}

	/// <summary>
	/// Get rules lookup
	/// </summary>
	public async Task<Dictionary<string, Rule>> GetRulesLookup(RuleExecutionRequest request)
	{
		if (!string.IsNullOrEmpty(request.RuleId))
		{
			var rule = await repositoryRules.GetOne(request.RuleId);

			if (rule is not null)
			{
				return new Dictionary<string, Rule>()
				{
					[request.RuleId] = rule
				};
			}

			return new Dictionary<string, Rule>();
		}

		return (await repositoryRules.GetAll().ToListAsync(CancellationToken.None)).Where(r => !r.IsDraft).ToDictionary(v => v.Id);
	}

	/// <summary>
	/// Get rule metadata lookup
	/// </summary>
	public async Task LoadMetadata(
		RuleExecutionRequest request,
		bool incrementVersions)
	{
		metadataVersions.Clear();

		try
		{
			using (var timedLogger = logger.TimeOperation("Loading metadata"))
			{
				var metadataLookup = (await repositoryRuleMetadata.Get(v => true)).ToDictionary(v => v.Id);

				foreach (var metadata in metadataLookup.Values)
				{
					if (string.IsNullOrEmpty(request.RuleId) || request.RuleId == metadata.Id)
					{
						if (incrementVersions)
						{
							metadata.IncrementVersion(request.RequestedBy, $"Batch run from {request.StartDate}");
							metadata.EarliestExecutionDate = request.StartDate ?? DateTime.MinValue;
							await this.repositoryRuleMetadata.QueueWrite(metadata, updateCache: false);
						}

						metadataVersions[metadata.Id] = metadata.Version;
					}
				}

				await this.repositoryRuleMetadata.FlushQueue(updateCache: false);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to load rule metadata");
		}
	}

	/// <summary>
	/// Get rule instances by twinId
	/// </summary>
	public async Task<Dictionary<string, List<RuleInstance>>> GetRuleInstanceLookup(
		RuleExecutionRequest request,
		ProgressTrackerForRuleExecution progressTracker,
		RuleTemplateFactory ruleTemplateFactory,
		SystemSummary summary,
		string noTwinId)
	{
		var ruleInstanceLookup = new Dictionary<string, List<RuleInstance>>(StringComparer.OrdinalIgnoreCase);
		summary.RuleInstanceSummary = new RuleInstanceSummary();

		using (var timedLogger = logger.TimeOperation(TimeSpan.FromMinutes(1), "Loading all rule instances"))
		{
			int allRuleInstancesCount = await this.repositoryRuleInstances.Count(x => true);
			int countRuleInstances = await this.repositoryRuleInstances.Count(RuleInstance.ExecutableInstanceFilter);

			logger.LogInformation("Loading {count:N0} valid rule instance records out of {all:N0}", countRuleInstances, allRuleInstancesCount);
			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(5));

			await progressTracker.ReportLoadingRuleInstances(0, countRuleInstances);
			int c = 0;
			await foreach (var ruleInstance in this.repositoryRuleInstances.GetAll(RuleInstance.ExecutableInstanceFilter))
			{
				if (ruleInstance.Disabled) continue;

				if (!string.IsNullOrEmpty(request.RuleId) && ruleInstance.RuleId != request.RuleId)
				{
					//for single rule execution, handle calc points after db read
					if (ruleInstance.RuleTemplate != RuleTemplateCalculatedPoint.ID)
					{
						continue;
					}
				}

				summary.AddToSummary(ruleInstance);

				// Check rule still exists
				if (!ruleTemplateFactory.CheckRuleForRuleInstanceStillExists(ruleInstance, logger))
				{
					logger.LogDebug("Rule instance {instance} should have been deleted with rule {rule}", ruleInstance.Id, ruleInstance.RuleId);
					continue;
				}

				if (ruleInstance.PointEntityIds.Any())
				{
					foreach (var pointEntityId in ruleInstance.PointEntityIds)
					{
						ruleInstanceLookup.AddOrUpdate(pointEntityId.Id, ruleInstance);
					}
				}
				else
				{
					// Rules and calc points are allowed to execute without capabilities
					// but they need a special sentinel value in the pipeline to trigger them
					ruleInstanceLookup.AddOrUpdate(noTwinId, ruleInstance);
				}

				c++;

				throttledLogger.LogInformation("Loaded {count:N0}/{total:N0} rule instance records", c, countRuleInstances);

				await progressTracker.ReportLoadingRuleInstances(c, countRuleInstances);
			}

			if (!string.IsNullOrEmpty(request.RuleId))
			{
				await FilterUnwantedInstancesForRule(ruleInstanceLookup, request.RuleId);
			}

			int count = ruleInstanceLookup.SelectMany(v => v.Value).Distinct().Count();

			await progressTracker.ReportLoadingRuleInstances(count, count);

			logger.LogInformation("Loaded {count:N0}/{total:N0} rule instance records", count, countRuleInstances);
		}

		foreach (var value in ruleInstanceLookup.Values)
		{
			value.TrimExcess();
		}

		return ruleInstanceLookup;
	}

	private async Task FilterUnwantedInstancesForRule(Dictionary<string, List<RuleInstance>> lookup, string ruleId)
	{
		var allInstances = lookup.SelectMany(v => v.Value).Distinct();

		var instances = await GetDependentRuleInstances(allInstances, ruleId, new HashSet<string>());

		var result = new Dictionary<string, List<RuleInstance>>();

		foreach (var key in lookup.Keys.ToList())
		{
			var ruleInstances = lookup[key]
				.Where(v => v.RuleId == ruleId || instances.Contains(v))
				.ToList();

			if (ruleInstances.Any())
			{
				lookup[key] = ruleInstances;
			}
			else
			{
				lookup.Remove(key);
			}
		}
	}

	private async Task<List<RuleInstance>> GetDependentRuleInstances(IEnumerable<RuleInstance> lookup, string ruleId, HashSet<string> ignoredIds)
	{
		var ruleInstancePointIds = lookup
							.Where(v => v.RuleId == ruleId)
							.SelectMany(v => v.PointEntityIds)
							.Select(v => v.Id)
							.ToHashSet();

		var calcPointRuleInstances = lookup.Where(v => v.RuleId != ruleId && !ignoredIds.Contains(v.RuleId) && v.RuleTemplate == RuleTemplateCalculatedPoint.ID);

		var updatedList = new List<RuleInstance>();
		var dependantRuleIds = new HashSet<string>();

		foreach (var ruleInstance in calcPointRuleInstances)
		{
			//find any calculated points that are inputs for the selected rule
			var outputTimeSeries = await timeSeriesManager.GetOrAdd(ruleInstance.OutputTrendId, EventHubSettings.RulesEngineConnectorId, ruleInstance.OutputExternalId);

			if (!string.IsNullOrEmpty(outputTimeSeries?.DtId) && ruleInstancePointIds.Contains(outputTimeSeries?.DtId))
			{
				dependantRuleIds.Add(ruleInstance.RuleId);
				updatedList.Add(ruleInstance);
			}
		}

		foreach (var id in dependantRuleIds)
		{
			ignoredIds.Add(id);
			var instances = await GetDependentRuleInstances(lookup, id, ignoredIds);
			updatedList.AddRange(instances);
		}

		return updatedList;
	}
}
