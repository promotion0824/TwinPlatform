using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

public interface IRepositoryRuleMetadata : IRepositoryBase<RuleMetadata>
{
	/// <summary>
	/// Updates rule metadata with audit details
	/// </summary>
	Task<RuleMetadata> RuleUpdated(string ruleId, string user);

	/// <summary>
	/// Get or Add a rule metadata instance
	/// </summary>
	Task<RuleMetadata> GetOrAdd(string ruleId, string? user = null);

	/// <summary>
	/// Delete metadata by rule id
	/// </summary>
	Task<int> DeleteMetadataByRuleId(string ruleId, CancellationToken cancellationToken);
}

/// <summary>
/// A repository for <see cref="RuleMetadata"/>s
/// </summary>
public class RepositoryRuleMetadata : RepositoryBase<RuleMetadata>, IRepositoryRuleMetadata
{
	public RepositoryRuleMetadata(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryRuleMetadata> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.RuleMetadatas, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	/// <inheritdoc />
	public async Task<RuleMetadata> GetOrAdd(string ruleId, string? user = null)
	{
		var existing = await this.GetOne(ruleId, updateCache: false);

		if (existing is null)
		{
			existing = new RuleMetadata(ruleId, user);
			existing.AddLog("Skill created", user ?? "No user");
		}
		return existing;
	}

	/// <inheritdoc />
	protected override IQueryable<RuleMetadata> ApplySort(IQueryable<RuleMetadata> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<RuleMetadata>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(RuleMetadata.InsightsGenerated):
					{
						result = AddSort(queryable, result!, first, x => x.InsightsGenerated, sortSpecification.sort);
						break;
					}
				case nameof(RuleMetadata.RuleInstanceCount):
					{
						result = AddSort(queryable, result!, first, x => x.RuleInstanceCount, sortSpecification.sort);
						break;
					}
				default:
				case nameof(RuleMetadata.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	protected override Expression<Func<RuleMetadata, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(RuleMetadata.Id):
				{
					return filter.CreateExpression((RuleMetadata v) => v.Id, filter.ToString(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	/// <summary>
	/// Add the must-carry array fields to the query
	/// </summary>
	public override IQueryable<RuleMetadata> WithArrays(IQueryable<RuleMetadata> input)
	{
		return input;
	}

	/// <summary>
	/// Delete rule metadata by rule id
	/// </summary>
	public async Task<int> DeleteMetadataByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		int count = 0;

		try
		{
			cancellationToken.ThrowIfCancellationRequested();

			await ExecuteAsync(async () =>
			{
				count = await rulesContext.Database
					.ExecuteSqlInterpolatedAsync($"SET NOCOUNT OFF; DELETE FROM [RuleMetadata] WHERE [RuleMetadata].[Id]={ruleId} select @@ROWCOUNT");

				this.InvalidateCache();

				logger.LogInformation("Deleted metadata for rule {ruleId}", ruleId);
			});
		}
		catch (OperationCanceledException ex)
		{
			logger.LogError(ex, "Delete rule {ruleId} metadata cancelled", ruleId);
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed rule {ruleId} metadata", ruleId);
			throw;
		}

		return count;
	}

	public async Task<RuleMetadata> RuleUpdated(string ruleId, string user)
	{
		var metadata = await GetOrAdd(ruleId, user);
		metadata.ModifiedBy = user;
		metadata.LastModified = DateTime.UtcNow;
		metadata.AddLog("Skill updated", user);

		await UpsertOne(metadata);
		return metadata;
	}
}
