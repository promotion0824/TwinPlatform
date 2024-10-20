using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Sources;
namespace Willow.Rules.Repository;

/// <summary>
/// Repository for rules
///</summary>
public interface IRepositoryRules : IRepositoryBase<Rule>
{
	/// <summary>
	/// Gets the distinct categories in alpha order
	/// </summary>
	Task<List<string>> GetCategories();

	/// <summary>
	/// Gets the distinct tags in order
	/// </summary>
	Task<List<string>> GetTags();

	/// <summary>
	/// Gets all rules with a filter and an optional wehere clause
	/// </summary>
	Task<Batch<(Rule rule, RuleMetadata metadata)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<Rule, bool>>? whereExpression = null,
		int? page = null,
		int? take = null);

	/// <summary>
	/// Delete a rule by Id
	/// </summary>
	Task<int> DeleteRuleById(string ruleId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get Rules where a variable name is referenced
	/// </summary>
	/// <returns></returns>
	Task<List<Rule>> MatchRuleReferences(string variableName);

	/// <summary>
	/// Set whether instances for the Rule should be synced with ADT or
	/// </summary>
	Task<int> SyncRuleWithADT(string ruleId, bool enabled);

	/// <summary>
	/// Set whether Insights and Commands for the Rule should sync
	/// </summary>
	Task<int> EnableSyncForRule(string ruleId, bool enabled);
}

/// <summary>
/// A repository for <see cref="Rule"/>s
/// </summary>
public class RepositoryRules : RepositoryBase<Rule>, IRepositoryRules
{
	public RepositoryRules(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryRules> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.Rules, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	protected override Expression<Func<Rule, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(Rule.Id):
				{
					return filter.CreateExpression((Rule v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(Rule.Name):
				{
					return filter.CreateExpression((Rule v) => v.Name, filter.ToString(formatProvider));
				}
			case nameof(Rule.Category):
				{
					return filter.CreateExpression((Rule v) => v.Category, filter.ToString(formatProvider));
				}
			case nameof(Rule.PrimaryModelId):
				{
					return filter.CreateExpression((Rule v) => v.PrimaryModelId, filter.ToString(formatProvider));
				}
			case nameof(Rule.CommandEnabled):
				{
					return filter.CreateExpression((Rule v) => v.CommandEnabled, filter.ToBoolean(formatProvider));
				}
			case nameof(Rule.TemplateId):
				{
					return filter.CreateExpression((Rule v) => v.TemplateId, filter.ToString(formatProvider));
				}
			case nameof(Rule.Tags):
				{
					return filter.CreateExpression((Rule v) => v.Tags.ToString(), filter.ToString(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	/// <inheritdoc />
	protected override IQueryable<Rule> ApplySort(IQueryable<Rule> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<Rule>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(Rule.Name): // Name
					{
						result = AddSort(queryable, result!, first, x => x.Name, sortSpecification.sort);
						break;
					}
				case nameof(Rule.Category): // Category
					{
						result = AddSort(queryable, result!, first, x => x.Category, sortSpecification.sort);
						break;
					}
				case nameof(Rule.PrimaryModelId): // Equipment
					{
						result = AddSort(queryable, result!, first, x => x.PrimaryModelId, sortSpecification.sort);
						break;
					}
				case nameof(Rule.CommandEnabled): // Sync
					{
						result = AddSort(queryable, result!, first, x => x.CommandEnabled, sortSpecification.sort);
						break;
					}
				case nameof(Rule.TemplateId): // Template id
					{
						result = AddSort(queryable, result!, first, x => x.TemplateId, sortSpecification.sort);
						break;
					}
				default:
				case nameof(Rule.Id): // Default to id
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	/// <summary>
	/// Add the must-carry array fields to the query
	/// </summary>
	public override IQueryable<Rule> WithArrays(IQueryable<Rule> input)
	{
		return input
			.Include(a => a.ParentIds)
			;
	}

	public async Task<Batch<(Rule rule, RuleMetadata metadata)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<Rule, bool>>? whereExpression = null,
		int? page = null,
		int? take = null)
	{
		var queryable = whereExpression is null ? dbSet : dbSet.Where(whereExpression);

		queryable = WithArrays(queryable);

		var batch = (from t in queryable
					 join il in rulesContext.RuleMetadatas on t.Id equals il.Id into j1
					 from j in j1.DefaultIfEmpty()
					 select new RuleMetadataTuple { rule = t, metadata = j });

		//due to multiple filtering we have to apply both rule and rulemetadata at the same time
		var expression = batch.AddFilter(filterSpecifications, nameof(Rule.Name), (v) => v.rule.Name, (f) => f.ToString(null));
		expression = batch.AddFilter(filterSpecifications, nameof(Rule.Category), (v) => v.rule.Category, (f) => f.ToString(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(Rule.PrimaryModelId), (v) => v.rule.PrimaryModelId, (f) => f.ToString(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(Rule.CommandEnabled), (v) => v.rule.CommandEnabled, (f) => f.ToBoolean(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(Rule.Id), (v) => v.rule.Id, (f) => f.ToString(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(Rule.TemplateId), (v) => v.rule.TemplateId, (f) => f.ToString(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(Rule.Tags), (v) => (string)(object)v.rule.Tags, (f) => f.ToString(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleMetadata.ValidInstanceCount), (v) => v.metadata.ValidInstanceCount, (f) => f.ToInt32(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleMetadata.ValidInstanceCount), (v) => v.metadata.ValidInstanceCount, (f) => f.ToInt32(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleMetadata.RuleInstanceCount), (v) => v.metadata.RuleInstanceCount, (f) => f.ToInt32(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleMetadata.CommandsGenerated), (v) => v.metadata.CommandsGenerated, (f) => f.ToInt32(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleMetadata.InsightsGenerated), (v) => v.metadata.InsightsGenerated, (f) => f.ToInt32(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleMetadata.ScanError), (v) => v.metadata.ScanError, (f) => f.ToString(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleMetadata.RuleInstanceStatus), (v) => (int)v.metadata.RuleInstanceStatus, (f) => (int)f.ToEnum<RuleInstanceStatus>(null), expression, isFlag: true);

		if (expression is not null)
		{
			batch = batch.Where(expression);
		}

		// Sort joined rule/metadata batch
		IOrderedQueryable<RuleMetadataTuple> ordered = null!;

		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(Rule.Name): // Name
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.rule.Name);
						break;
					}
				case nameof(Rule.Category): // Category
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.rule.Category);
						break;
					}
				case nameof(RuleMetadata.LastModified): // LastModified
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.LastModified);
						break;
					}
				case nameof(RuleMetadata.ModifiedBy): // ModifiedBy
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.ModifiedBy);
						break;
					}
				case nameof(Rule.PrimaryModelId): // Equipment
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.rule.Name);
						break;
					}
				case nameof(RuleMetadata.RuleInstanceCount): // Instances
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.RuleInstanceCount);
						break;
					}
				case nameof(RuleMetadata.CommandsGenerated):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.CommandsGenerated);
						break;
					}
				case nameof(RuleMetadata.ValidInstanceCount): // Valid
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.ValidInstanceCount);
						break;
					}
				case nameof(RuleMetadata.ScanError): // Scan error
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.ScanError);
						break;
					}
				case nameof(Rule.CommandEnabled): // Sync
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.rule.CommandEnabled);
						break;
					}
				case nameof(RuleMetadata.InsightsGenerated): // Insights
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.InsightsGenerated);
						break;
					}
				case nameof(RuleMetadata.RuleInstanceStatus): // RuleInstanceStatus
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.metadata.RuleInstanceStatus);
						break;
					}
				case nameof(Rule.TemplateId): // Template id
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.rule.TemplateId);
						break;
					}
				default:
				case nameof(Rule.Id): // Default to id
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.rule.Id);
						break;
					}
			}
		}

		batch = ordered ?? batch;

		var result = await GetOrCreateBatch(batch, (v) => v.rule.Id, page, take);

		return result.Transform(v => (v.rule, v.metadata));
	}

	/// <summary>
	/// A tuple containing a rule and its rule metadata. Represents a join between their queries.
	/// </summary>
	private class RuleMetadataTuple
	{
		public Rule rule { get; set; } = null!;
		public RuleMetadata metadata { get; set; } = null!;
	}

	/// <summary>
	/// These categories plus any in-use populate the drop-down
	/// </summary>
	private static readonly string[] DefaultCategories = [
		"Alarm",
		"Alert",
		"Comfort",
		"Commissioning",
		"Data quality",
		"Diagnostic",
		"Energy",
		"Fault",
		"Note",
		"Predictive"
		];

	/// <inheritdoc />
	public async Task<List<string>> GetCategories()
	{
		var dbCategories = await this.dbSet
			.Select(x => x.Category)
			.Distinct()  // only fetch each once from DB
			.ToListAsync();

		// Separate these two queries because EF can't figure that out

		var categories = dbCategories
			.Concat(DefaultCategories)
			.Where(x => x is not null)
			.Distinct()
			.OrderBy(x => x);

		return categories.ToList();
	}

	/// <summary>
	/// Delete a rule by Id
	/// </summary>
	public async Task<int> DeleteRuleById(string ruleId, CancellationToken cancellationToken = default)
	{
		int count = 0;

		try
		{
			cancellationToken.ThrowIfCancellationRequested();

			await ExecuteAsync(async () =>
			{
				count = await this.rulesContext.Database
					.ExecuteSqlInterpolatedAsync($"SET NOCOUNT OFF; DELETE FROM [Rule] WHERE [Rule].[Id]={ruleId} select @@ROWCOUNT");

				this.InvalidateCache();

				logger.LogInformation("Deleted rule {ruleId}", ruleId);
			});
		}
		catch (OperationCanceledException ex)
		{
			logger.LogError(ex, "Delete rule {ruleId} cancelled", ruleId);
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to delete rule {ruleId}", ruleId);
			throw;
		}

		return count;
	}

	public async Task<int> EnableSyncForRule(string ruleId, bool enabled)
	{
		logger.LogInformation($"Enable syncing for rule {ruleId} to sync: {enabled}");

		int result = await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Insight] SET CommandEnabled={enabled} WHERE [Insight].[RuleId]={ruleId}");

		result += await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Commands] SET Enabled={enabled} WHERE [Commands].[RuleId]={ruleId}");

		await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Rule] SET CommandEnabled={enabled} WHERE [Rule].[Id]={ruleId}");

		await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [RuleInstance] SET CommandEnabled={enabled} WHERE [RuleInstance].[RuleId]={ruleId}");

		this.InvalidateCache();

		return result;
	}

	public async Task<List<Rule>> MatchRuleReferences(string variableName)
	{
		var result = new List<Rule>();

		await foreach (var item in GetAll())
		{
			if (item.Parameters.Any(v => v.MatchVariableName(variableName))
			|| item.ImpactScores.Any(v => v.MatchVariableName(variableName))
			|| item.Filters.Any(v => v.MatchVariableName(variableName)))
			{
				result.Add(item);
			}
		}

		return result;
	}

	/// <summary>
	/// Set whether instances for the Rule should be synced with ADT or
	/// </summary>
	public async Task<int> SyncRuleWithADT(string ruleId, bool enabled)
	{
		logger.LogInformation($"Set rule {ruleId} to sync: {enabled}");

		int result;

		result = await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Rule] SET [ADTEnabled]={enabled} WHERE [Rule].[Id]={ruleId}");

		this.InvalidateCache();

		return result;
	}

	/// <summary>
	/// Some Defaults Tags
	/// </summary>
	private static readonly string[] DefaultTags = [
		"ActiveEfficiency",
		"Aviation",
		"BE&Ops",
		"Conveyance",
		"Healthcare",
		"Monitoring",
		"Occupancy",
		"OnsiteFoodPrep",
		"Retail",
		"SpatialandStatic",
		"Sustainability",
		"WillowStandard"
		];

	public async Task<List<string>> GetTags()
	{
		var tags = new List<string>();

		// Fetch distinct tag lists from the database
		var dbTags = await dbSet
			.Select(x => x.Tags)
			.Distinct()
			.ToListAsync();

		// Iterate over each tag list
		foreach (var tagList in dbTags)
		{
			if (tagList != null)
			{
				// Tags added manually could be a single string with comma delimited values
				var splitTags = tagList
					.SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
					.Where(t => !string.IsNullOrWhiteSpace(t));

				// Add the filtered tags to the list
				tags.AddRange(splitTags.Select(tag => tag.Trim()));
			}
		}

		// Add default tags
		tags.AddRange(DefaultTags);

		return [.. tags.OrderBy(x => x).Distinct()];
	}
}
