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

public interface IRepositoryRuleExecutionRequest : IRepositoryBase<RuleExecutionRequest>
{
	/// <summary>
	/// Prevent stacking of same requests by command and specific or empty rule id
	/// </summary>
	/// <param name="request"></param>
	Task<bool> IsDuplicateRequest(RuleExecutionRequest request);
}

/// <summary>
/// A repository for <see cref="RuleExecutionRequest"/>
/// </summary>
public class RepositoryRuleExecutionRequest : RepositoryBase<RuleExecutionRequest>, IRepositoryRuleExecutionRequest
{
	public RepositoryRuleExecutionRequest(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryRuleExecutionRequest> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.RuleExecutionRequests, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	protected override Expression<Func<RuleExecutionRequest, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(RuleExecutionRequest.Id):
				{
					return filter.CreateExpression((RuleExecutionRequest v) => v.Id, filter.ToString(formatProvider));
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
	public override IQueryable<RuleExecutionRequest> WithArrays(IQueryable<RuleExecutionRequest> input)
	{
		return input;//.Include(a => a.Occurrences);
	}

	/// <inheritdoc />
	protected override IQueryable<RuleExecutionRequest> ApplySort(IQueryable<RuleExecutionRequest> queryable, SortSpecificationDto[] sortSpecifications)
	{
		return queryable;
	}

	/// <summary>
	/// Prevent stacking of same requests by command and specific or empty rule id
	/// </summary>
	/// <param name="request"></param>
	public async Task<bool> IsDuplicateRequest(RuleExecutionRequest request)
	{
		var sameRequestByCommand = await Get(r => r.Command == request.Command);

		var isDuplicate = sameRequestByCommand.Any(r => r.RuleId == request.RuleId);

		return isDuplicate;
	}
}
