using System;
using System.Linq;
using System.Linq.Expressions;
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

public interface IRepositoryProgress : IRepositoryBase<Progress>
{
	/// <summary>
	/// Gets the most recent execution status
	/// </summary>
	/// <returns></returns>
	Task<Progress?> GetMostRecentExecution();
}

/// <summary>
/// A repository for <see cref="Progress"/>
/// </summary>
public class RepositoryProgress : RepositoryBase<Progress>, IRepositoryProgress
{
	public RepositoryProgress(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryProgress> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.Progress, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	protected override Expression<Func<Progress, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(Progress.Id):
				{
					return filter.CreateExpression((Progress v) => v.Id, filter.ToString(formatProvider));
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
	public override IQueryable<Progress> WithArrays(IQueryable<Progress> input)
	{
		return input;//.Include(a => a.Occurrences);
	}

	/// <inheritdoc />
	protected override IQueryable<Progress> ApplySort(IQueryable<Progress> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<Progress>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(Progress.Type):
					{
						result = AddSort(queryable, result!, first, x => x.Type, sortSpecification.sort);
						break;
					}
				case nameof(Progress.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
				default:
				case nameof(Progress.LastUpdated):
					{
						result = AddSort(queryable, result!, first, x => x.LastUpdated, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task<Progress?> GetMostRecentExecution()
	{
		return await WithArrays(GetQueryable()).Where(x => x.Id == Progress.RealtimeExecutionId)
			.OrderByDescending(x => x.EndTimeSeriesTime)
			.FirstOrDefaultAsync();
	}
}
