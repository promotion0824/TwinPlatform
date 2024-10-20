using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

/// <summary>
/// Repository for global variables
///</summary>
public interface IRepositoryADTSummary : IRepositoryBase<ADTSummary>
{
	/// <summary>
	/// Get the most recent ADT summary only
	/// </summary>
	public Task<ADTSummary> GetLatest();

	/// <summary>
	/// Only update ADT related fields on the summary
	/// </summary>
	/// <remarks>Happens during cache update</remarks>
	public Task UpdateADTRelatedSummary(ADTSummary summary);

	/// <summary>
	/// Only update system related fields on the summary
	/// </summary>
	/// <remarks>Happens during expans and execution</remarks>
	public Task UpdateSystemRelatedSummary(ADTSummary summary);
}

/// <summary>
/// A repository for <see cref="GlobalVariable"/>s
/// </summary>
public class RepositoryADTSummary : RepositoryBase<ADTSummary>, IRepositoryADTSummary
{
	public RepositoryADTSummary(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryADTSummary> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.ADTSummaries, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<ADTSummary> ApplySort(IQueryable<ADTSummary> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<ADTSummary>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(ADTSummary.AsOfDate):
					{
						result = AddSort(queryable, result!, first, x => x.AsOfDate, sortSpecification.sort);
						break;
					}
				default:
				case nameof(ADTSummary.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	protected override Expression<Func<ADTSummary, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(ADTSummary.Id):
				{
					return filter.CreateExpression((ADTSummary v) => v.Id, filter.ToString(formatProvider));
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
	public override IQueryable<ADTSummary> WithArrays(IQueryable<ADTSummary> input)
	{
		return input;
	}

	public async Task<ADTSummary> GetLatest()
	{
		var summary = await WithArrays(GetQueryable()).OrderByDescending(x => x.AsOfDate).FirstOrDefaultAsync();

		if (summary is not null)
		{
			summary.SystemSummary ??= new SystemSummary();
		}

		return summary ?? new ADTSummary() { Id = "Summary" };
	}

	public async Task UpdateADTRelatedSummary(ADTSummary summary)
	{
		var config = new BulkConfig()
		{
			PropertiesToExcludeOnUpdate = new List<string>()
			{
				nameof(ADTSummary.SystemSummary),
			}
		};

		await BulkMerge(new List<ADTSummary>
		{
			summary
		}, config: config);
	}

	public async Task UpdateSystemRelatedSummary(ADTSummary summary)
	{
		var config = new BulkConfig()
		{
			PropertiesToIncludeOnUpdate = new List<string>()
			{
				nameof(ADTSummary.SystemSummary),
			}
		};

		await BulkMerge(new List<ADTSummary>
		{
			summary
		}, config: config);
	}
}
