using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

public interface IRepositoryInsightChange : IRepositoryBase<InsightChange>
{
	/// <summary>
	/// Overwrites insights changes for an insight with the new list
	/// </summary>
	Task OverwriteInsightChanges(Insight insight, IList<InsightChange> changes);
}

/// <summary>
/// A repository for <see cref="InsightChange"/>
/// </summary>
public class RepositoryInsightChange : RepositoryBase<InsightChange>, IRepositoryInsightChange
{
	public RepositoryInsightChange(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryInsightChange> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.InsightChanges, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	protected override Expression<Func<InsightChange, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(InsightChange.Id):
				{
					return filter.CreateExpression((InsightChange v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(InsightChange.InsightId):
				{
					return filter.CreateExpression((InsightChange v) => v.InsightId, filter.ToString(formatProvider));
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
	public override IQueryable<InsightChange> WithArrays(IQueryable<InsightChange> input)
	{
		return input;//.Include(a => a.Occurrences);
	}

	/// <inheritdoc />
	protected override IQueryable<InsightChange> ApplySort(IQueryable<InsightChange> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<InsightChange>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(InsightChange.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
				default:
				case nameof(InsightChange.Timestamp):
					{
						result = AddSort(queryable, result!, first, x => x.Timestamp, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task OverwriteInsightChanges(Insight insight, IList<InsightChange> changes)
	{
		using (var rc = await dbContextFactory.CreateDbContextAsync())
		{
			string insightId = insight.Id;

			try
			{
				var bulkConfig = new BulkConfig();

				//delete old entries
				bulkConfig.SetSynchronizeFilter<InsightChange>(a => a.InsightId == insightId);
				
				await rc.BulkInsertOrUpdateOrDeleteAsync(changes, bulkConfig);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to bulk merge insight changes for insight {id}", insightId);
			}
		}
	}
}
