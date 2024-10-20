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

/// <summary>
/// A repository for tracking work requested or completed by the back-end processor
/// </summary>
public interface IRepositoryRuleExecutions : IRepositoryBase<RuleExecution>
{
	string IdGen(string customerEnvironmentId, string ruleId);
	Task<RuleExecution> MergeWorkItem(RuleExecutionRequest ruleExecutionRequest);

	/// <summary>
	/// Get one rule execution by ruleId
	/// </summary>
	Task<RuleExecution> GetOneByRuleId(string ruleId);
}

/// <summary>
/// A repository for <see cref="RuleExecution"/>s
/// </summary>
public class RepositoryRuleExecutions : RepositoryBase<RuleExecution>, IRepositoryRuleExecutions
{
	public RepositoryRuleExecutions(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryRuleExecutions> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.RuleExecutions, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<RuleExecution> ApplySort(IQueryable<RuleExecution> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<RuleExecution>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(RuleExecution.StartDate):
					{
						result = AddSort(queryable, result!, first, x => x.StartDate, sortSpecification.sort);
						break;
					}
				case nameof(RuleExecution.TargetEndDate):
					{
						result = AddSort(queryable, result!, first, x => x.TargetEndDate, sortSpecification.sort);
						break;
					}
				case nameof(RuleExecution.RuleId):
					{
						result = AddSort(queryable, result!, first, x => x.RuleId, sortSpecification.sort);
						break;
					}
				default:
				case nameof(RuleExecution.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	protected override Expression<Func<RuleExecution, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(RuleExecution.Id):
				{
					return filter.CreateExpression((RuleExecution v) => v.Id, filter.ToString(formatProvider));
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
	public override IQueryable<RuleExecution> WithArrays(IQueryable<RuleExecution> input)
	{
		return input;
	}

	public string IdGen(string customerEnvironmentId, string ruleId) => $"{customerEnvironmentId}-{ruleId}";

	/// <inheritdoc />
	public async Task UpdateWorkItem(RuleExecution ruleExecution)
	{
		ruleExecution.Id = IdGen(ruleExecution.CustomerEnvironmentId, ruleExecution.RuleId);
		await this.UpsertOne(ruleExecution, cancellationToken: CancellationToken.None);
	}

	/// <inheritdoc />
	public async Task<RuleExecution> MergeWorkItem(RuleExecutionRequest ruleExecutionRequest)
	{
		string customerEnvironmentId = ruleExecutionRequest.CustomerEnvironmentId;
		string ruleId = ruleExecutionRequest.RuleId;
		var defaultFallbackDays = -1;

		DateTimeOffset startDate = ruleExecutionRequest.StartDate.GetValueOrDefault(DateTime.Today.AddDays(defaultFallbackDays).ToUniversalTime());
		DateTimeOffset endDate = ruleExecutionRequest.TargetEndDate.GetValueOrDefault(DateTime.UtcNow);

		//if no dates are provided, run from the last execution's end date up to today
		//if nothing was found then the start date equals the defaultFallbackDays
		if (ruleExecutionRequest.StartDate is null)
		{
			var existingRuleExecutions = await Get();

			var latestExecution = existingRuleExecutions
				.Where(x => string.IsNullOrEmpty(ruleId) || string.Equals(x.RuleId, ruleId))
				.OrderByDescending(x => x.CompletedEndDate)
				.FirstOrDefault();

			if (latestExecution != null)
			{
				startDate = latestExecution.CompletedEndDate;
			}

#if DEBUG
			//in dev we swap db's for different customers alot. wHen you swap to an old DB, this start time is sometimes weeks/months,
			//so fallback to 6 hours so you don't have to wait ages for the first realtime to finish.
			if (DateTimeOffset.UtcNow - startDate > TimeSpan.FromHours(6))
			{
				startDate = DateTimeOffset.UtcNow.AddHours(-6);
			}
#endif
		}

		var ruleExecution = new RuleExecution
		{
			Id = ruleExecutionRequest.CorrelationId,
			CustomerEnvironmentId = customerEnvironmentId,
			RuleId = ruleId,
			StartDate = startDate,
			TargetEndDate = endDate,
			Percentage = 0.0,
			Generation = Guid.NewGuid(),
			PercentageReported = 0.0,
			CompletedEndDate = startDate
		};

		// Find all rule executions that have the same rule id (or "") and which overlap this execution request

#if MERGE_EXECUTIONS
		var existingRuleExecutions = await this.GetAll(); // TODO: Pass order to call and read async
		var existingSameRule = existingRuleExecutions.Where(x => x.RuleId.Equals(ruleId))
			.OrderByDescending(x => x.TargetEndDate)    // latest end first
			.ThenBy(x => x.StartDate)                   // then by longest
			.ToList();

		foreach (var existing in existingSameRule.OrderByDescending(x => x.TargetEndDate))
		{
			if (existing.Consumes(ruleExecution))
			{
				logger.LogInformation($"Existing rule execution covers {startDate}-{endDate} for '{ruleId}'");
				return existing;
			}
			if (existing.Overlaps(ruleExecution))
			{
				// Merge them into the old and write it back to the database
				if (ruleExecutionRequest.TargetEndDate > existing.TargetEndDate)
				{
					logger.LogInformation($"Extending existing rule execution to cover {startDate}-{endDate} for '{ruleId}'");
					existing.TargetEndDate = ruleExecutionRequest.TargetEndDate;
					existing.Generation = Guid.NewGuid();           // and mark it as changed and needing to be requeued
					await this.UpsertOne(existing);
					return existing;
				}
				// Cannot extend backwards in time, need to requeue a new request, but extend it to cover the already calculated
				// extent so it updates all Insights with new ranges if necessary
				ruleExecution.TargetEndDate = existing.TargetEndDate; // which is equal or greater so this is an extension of time
			}
		}
#endif

		logger.LogInformation($"Created a new rule execution {startDate}-{endDate} for '{ruleId}'");
		await this.UpsertOne(ruleExecution, cancellationToken: CancellationToken.None);
		return ruleExecution;
	}

	public async Task<RuleExecution> GetOneByRuleId(string ruleId)
	{
		RuleExecution? item = null;

		await ExecuteAsync(async () =>
		{
			item = await WithArrays(dbSet).FirstOrDefaultAsync(x => x.RuleId == ruleId);
		});

		return item!;
	}
}
