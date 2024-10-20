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

/// <summary>
/// Repository for global variables
///</summary>
public interface IRepositoryGlobalVariable : IRepositoryBase<GlobalVariable>
{
	/// <summary>
	/// Gets the distinct tags in order
	/// </summary>
	Task<List<string>> GetTags();

	/// <summary>
	/// Get Globals where a variable name is referenced
	/// </summary>
	/// <returns></returns>
	Task<List<GlobalVariable>> MatchGlobalVariableReferences(string variableName);
}

/// <summary>
/// A repository for <see cref="GlobalVariable"/>s
/// </summary>
public class RepositoryGlobalVariable : RepositoryBase<GlobalVariable>, IRepositoryGlobalVariable
{
	public RepositoryGlobalVariable(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryGlobalVariable> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.GlobalVariables, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<GlobalVariable> ApplySort(IQueryable<GlobalVariable> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<GlobalVariable>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(Rule.Name):
					{
						result = AddSort(queryable, result!, first, x => x.Name, sortSpecification.sort);
						break;
					}
				default:
				case nameof(Rule.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	protected override Expression<Func<GlobalVariable, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(GlobalVariable.Name):
				{
					return filter.CreateExpression((GlobalVariable v) => v.Name, filter.ToString(formatProvider));
				}
			case nameof(GlobalVariable.Id):
				{
					return filter.CreateExpression((GlobalVariable v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(GlobalVariable.Tags):
				{
					return filter.CreateExpression((GlobalVariable v) => (string)(object)v.Tags, filter.ToString(formatProvider));
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
	public override IQueryable<GlobalVariable> WithArrays(IQueryable<GlobalVariable> input)
	{
		return input;
	}

	public async Task<List<GlobalVariable>> MatchGlobalVariableReferences(string variableName)
	{
		var result = new List<GlobalVariable>();

		await foreach (var item in GetAll())
		{
			if (item.Expression.Any(v => v.MatchVariableName(variableName)))
			{
				result.Add(item);
			}
		}

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
		"Psychro",
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
