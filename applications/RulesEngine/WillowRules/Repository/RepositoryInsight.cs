using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
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
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

public interface IRepositoryInsight : IRepositoryBase<Insight>
{
	/// <summary>
	/// Disables all insights
	/// </summary>
	/// <returns></returns>
	Task DisableAllInsights();

	/// <summary>
	/// Get rule instances that match an element in the location array in JSON
	/// </summary>
	Task<IEnumerable<Insight>> GetByLocation(string location);

	/// <summary>
	/// Get rule instances that match an element in the fedby array in JSON
	/// </summary>
	Task<IEnumerable<Insight>> GetFedBy(string twinId);

	/// <summary>
	/// Get rule instances that match an element in the feeds array in JSON
	/// </summary>
	Task<IEnumerable<Insight>> GetFeeds(string twinId);

	/// <summary>
	/// Set whether this Insight should be shown in command or not
	/// </summary>
	Task<int> SyncWithCommand(string insightId, bool enabled);

	/// <summary>
	/// Sets the remote Id used in Command
	/// </summary>
	Task<int> SetCommandInsightId(string insightId, Guid commandInsightId, InsightStatus status);

	/// <summary>
	/// Removes all insights for a given rule Id
	/// </summary>
	Task<int> RemoveAllInsightsForRule(string ruleId);

	/// <summary>
	/// Delete all insights from DB
	/// </summary>
	Task RemoveAll();

	/// <summary>
	/// Gets SiteId and count for insights
	/// </summary>
	Task<IEnumerable<(Guid siteId, int count)>> GetSiteIds();

	/// <summary>
	/// Gets a list of equipmentids with insights count
	/// </summary>
	Task<IEnumerable<(string equipmentId, int count)>> GetEquipmentIds();

	/// <summary>
	/// Gets a unique list of insight impact score names, field ids and units
	/// </summary>
	Task<List<(string FieldId, string Name, string[] Units, int Count)>> GetImpactScoreNames();

	/// <summary>
	/// Gets a unique list of insight impact score names, field ids and units by rule id
	/// </summary>
	Task<List<(string FieldId, string Name, string[] Units, int Count)>> GetImpactScoreNames(string ruleId);

	/// <summary>
	/// Gets a unique list of insight ids and it's command enabled flag
	/// </summary>
	Task<IEnumerable<(string id, bool enabled, Guid commandId, InsightStatus status, DateTimeOffset lastSyncDate)>> GetCommandValues();

	/// <summary>
	/// Delete impact scores where the LastUpdated value does not equal the insights LastUpdated
	/// </summary>
	/// <remarks>
	/// When an impact score has been removed from the rule we also have to delete from the insight
	/// </remarks>
	Task<int> DeleteOldImpactScores();

	/// <summary>
	/// Get insights for a given rule
	/// </summary>
	Task<IEnumerable<Insight>> GetInsightsForRule(string ruleId);

	/// <summary>
	/// Get insights count for a given rule
	/// </summary>
	Task<int> GetInsightCountForRule(string ruleId);

	/// <summary>
	/// Get insights with no rule instance
	/// </summary>
	IAsyncEnumerable<Insight> GetOrphanedInsights();

	/// <summary>
	/// Update insights back to invalid if rule instance is invalid
	/// </summary>
	/// <returns></returns>
	Task<int> CheckValidInsights();

	/// <summary>
	/// Delete all occrruences before a certain date
	/// </summary>
	Task<int> DeleteOccurrencesBefore(DateTimeOffset date);

	/// <summary>
	/// Prune All Occurrences to a max amount
	/// </summary>
	Task<int> DeleteOccurrences(int maxCount);

	/// <summary>
	/// Delete All Occurrences
	/// </summary>
	Task DeleteOccurrences(string ruleId = "");
}

/// <summary>
/// A repository for <see cref="Insight"/>s
/// </summary>
public class RepositoryInsight : RepositoryBase<Insight>, IRepositoryInsight
{
	public RepositoryInsight(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryInsight> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.Insights, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	public override IQueryable<Insight> GetQueryable()
	{
		//include child impact scores for any querying
		return dbSet
			//.Include(v => v.Occurrences)dont include occurrences. This list is too big during standard queries. Use the GetQueryable and explicitly include if needed
			.Include(v => v.ImpactScores);
	}

	protected override IQueryable<Insight> GetQueryableForSingle()
	{
		return GetQueryable()
			.Include(v => v.Occurrences);
	}

	protected override async Task BulkInsertOrUpdateAsync(RulesContext rc, IList<Insight> items, BulkConfig? config = null, bool updateOnly = false, CancellationToken cancellationToken = default)
	{
		await base.BulkInsertOrUpdateAsync(rc, items, config, updateOnly, cancellationToken);

		foreach (var insight in items.Where(v => v.Occurrences.Count > 0))
		{
			await DeleteOccurrencesAfter(insight.Id, insight.Occurrences.First().Started);
		}

		await rc.BulkInsertAsync(items.SelectMany(v => v.Occurrences).ToList(), cancellationToken: cancellationToken);

		await rc.BulkInsertOrUpdateAsync(items.SelectMany(v => v.ImpactScores).ToList(), cancellationToken: cancellationToken);
	}

	protected override async Task BulkDeleteAsync(RulesContext rc, IList<Insight> items, CancellationToken cancellationToken = default)
	{
		await base.BulkDeleteAsync(rc, items, cancellationToken);

		if (items.Any(v => v.ImpactScores != null))
		{
			await rc.BulkDeleteAsync(items.SelectMany(v => v.ImpactScores ?? []).ToList(), cancellationToken: cancellationToken);
		}

		if (items.Any(v => v.Occurrences != null))
		{
			await rc.BulkDeleteAsync(items.SelectMany(v => v.Occurrences ?? []).ToList(), cancellationToken: cancellationToken);
		}
	}

	public override async Task<Batch<Insight>> GetAll(SortSpecificationDto[] sortSpecifications, FilterSpecificationDto[] filterSpecifications,
		Expression<Func<Insight, bool>>? whereExpression = null, int? page = null, int? take = null)
	{
		var impactScoreFields = (await GetImpactScoreNames()).Select(v => v.FieldId).ToList();

		//override filtering and sorting when impact score fields are included
		//use sql pivot which EF will use as the inner select when querying Insights
		if (sortSpecifications.Any(v => impactScoreFields.Contains(v.field))
		|| filterSpecifications.Any(v => impactScoreFields.Contains(v.field)))
		{
			//valid query fields
			var allowedFields = new string[] {
				nameof(Insight.Id),
				nameof(Insight.Text),
				nameof(Insight.RuleName),
				nameof(Insight.RuleCategory),
				nameof(Insight.EquipmentId),
				nameof(Insight.PrimaryModelId),
				nameof(Insight.RuleId),
				nameof(Insight.TwinLocations),
				nameof(Insight.CommandEnabled),
				nameof(Insight.Status),
				nameof(Insight.IsValid),
				nameof(Insight.IsFaulty),
				nameof(Insight.EquipmentName),
				nameof(Insight.TimeZone),
				nameof(Insight.CommandInsightId)
			};

			//get either the sort or filter linked to the impact score
			var sortSpecification = sortSpecifications.FirstOrDefault(v => impactScoreFields.Contains(v.field));
			var filterSpecification = filterSpecifications.FirstOrDefault(v => impactScoreFields.Contains(v.field));
			var filters = new List<string>();
			var sqlParamters = new List<SqlParameter>();
			var whereSql = string.Empty;

			//When there is an impact score "Order by",
			//filters have to be applied during the pivot construction
			//and not on the where clause that EF generates
			foreach (var filter in filterSpecifications)
			{
				if (allowedFields.Contains(filter.field))
				{
					sqlParamters.Add(new SqlParameter(filter.field, filter.ToString(null)));
					filters.Add(filter.CreateSqlExpression($"ii.[{filter.field}]", $"@{filter.field}"));
				}
			}

			//the score filter has 2 filters (also applied during pivot)
			//one for the score itself and one for the score field
			if (filterSpecification is not null)
			{
				sqlParamters.Add(new SqlParameter("score", filterSpecification.ToString(null)));
				var scoreFilter = filterSpecification.CreateSqlExpression(nameof(ImpactScore.Score), "@score");

				var fieldFilter = new FilterSpecificationDto()
				{
					field = nameof(ImpactScore.FieldId),
					@operator = FilterSpecificationDto.EqualsLiteral,
					value = filterSpecification.field
				};

				sqlParamters.Add(new SqlParameter("scoreField", filterSpecification.field));
				var scoreFieldFilter = fieldFilter.CreateSqlExpression(nameof(ImpactScore.FieldId), "@scoreField");

				filters.Add($"({scoreFilter} AND {scoreFieldFilter})");
			}

			if (filters.Count > 0)
			{
				var logicalOperator = filterSpecifications.First().logicalOperator == "OR" ? "OR" : "AND";
				whereSql = $" where {string.Join($" {logicalOperator} ", filters)}";
			}

			var colSql = string.Join(",", impactScoreFields.Select(v => $"[{v}]"));

			//here is the sql to pivot impact scores
			//only select the insight id's in the end
			//which EF will use for its where clause
			var sql = @$"SELECT Id
            from
            (
              select ii.Id,FieldId,BaseScore
              from insight ii left join InsightImpactScore i on i.insightid = ii.id
			  {whereSql}
            ) x
            pivot
            (
                max(BaseScore)
                for FieldId in ({colSql})
            ) p";

			//get total count before any paging is applied.
			var total = await dbSet.FromSqlRaw(sql, sqlParamters.ToArray()).CountAsync();

			//ordering and paging happens on the pivot.
			//Sorting will then have to occur again in c#, becuase sql loses ordering of subqueries
			//but at least we got the right page set.
			if (sortSpecification is not null)
			{
				var orderBy = sortSpecification.sort?.ToUpperInvariant() == "ASC" ? "ASC" : "DESC";
				sql += $" order by [{sortSpecification.field}] {orderBy}";
				sql += $" offset {(page.GetValueOrDefault(1) - 1) * take.GetValueOrDefault()} rows fetch next {take.GetValueOrDefault(total)} rows only";
				//there might be sorting on the insight level so remove the pivot's sort spec
				sortSpecifications = sortSpecifications.Except(new SortSpecificationDto[] { sortSpecification }).ToArray();
			}
			else if (sortSpecifications.Length == 0)
			{
				//we need at least one sort so we can apply paging
				sortSpecifications = new SortSpecificationDto[] { new SortSpecificationDto(nameof(Insight.LastFaultedDate), "DESC") };
			}

			//the final raw sql for EF
			var finalSql = $"select * from insight where id in ({sql})";

			IQueryable<Insight> queryable = dbSet.FromSqlRaw(finalSql, sqlParamters.ToArray()).Include(v => v.ImpactScores);

			//if any other sorts on Inisght level, apply them now
			if (sortSpecifications.Length > 0)
			{
				queryable = ApplySort(queryable, sortSpecifications)
							.Page(page, take, out var skipped);
			}

			var batch = await GetOrCreateBatch(queryable, (v) => v.Id);

			//sql ignores ordering on inner selects so we have to re-order for the results if it was one a pivot column
			if (sortSpecification is not null)
			{
				if (sortSpecification.sort?.ToUpperInvariant() == "ASC")
				{
					//force large defaults for items which does not have a score
					var largeScore = 10000000;
					batch.Items = batch.Items.OrderBy(p => (p.ImpactScores.FirstOrDefault(v => v.FieldId == sortSpecification.field)?.Score).GetValueOrDefault(largeScore)).ToArray();
				}
				else
				{
					var smallScore = -10000000;
					batch.Items = batch.Items.OrderByDescending(p => (p.ImpactScores.FirstOrDefault(v => v.FieldId == sortSpecification.field)?.Score).GetValueOrDefault(smallScore)).ToArray();
				}
			}

			//paging is done in sql so just jimmy the batch stats
			(new string[] { }).AsQueryable().Page(page, take, out int skip);
			batch.Total = total;
			batch.Before = skip;
			batch.After = total - skip;

			return batch;
		}

		return await base.GetAll(sortSpecifications, filterSpecifications, whereExpression, page, take);
	}

	/// <summary>
	/// Add the must-carry array fields to the query
	/// </summary>
	public override IQueryable<Insight> WithArrays(IQueryable<Insight> input)
	{
		return input;//.Include(a => a.Occurrences);
	}

	protected override Expression<Func<Insight, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(Insight.Id):
				{
					return filter.CreateExpression((Insight v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(Insight.Text):
				{
					return filter.CreateExpression((Insight v) => v.Text, filter.ToString(formatProvider));
				}
			case nameof(Insight.RuleId):
				{
					return filter.CreateExpression((Insight v) => v.RuleId, filter.ToString(formatProvider));
				}
			case nameof(Insight.RuleName):
				{
					return filter.CreateExpression((Insight v) => v.RuleName, filter.ToString(formatProvider));
				}
			case nameof(Insight.RuleCategory):
				{
					return filter.CreateExpression((Insight v) => v.RuleCategory, filter.ToString(formatProvider));
				}
			case nameof(Insight.RuleTags):
				{
					return filter.CreateExpression((Insight v) => (string)(object)v.RuleTags, filter.ToString(formatProvider));
				}
			case nameof(Insight.EquipmentId):
				{
					return filter.CreateExpression((Insight v) => v.EquipmentId, filter.ToString(formatProvider));
				}
			case nameof(Insight.EquipmentName):
				{
					return filter.CreateExpression((Insight v) => v.EquipmentName, filter.ToString(formatProvider));
				}
			case nameof(Insight.PrimaryModelId):
				{
					return filter.CreateExpression((Insight v) => v.PrimaryModelId, filter.ToString(formatProvider));
				}
			case nameof(Insight.CommandEnabled):
				{
					return filter.CreateExpression((Insight v) => v.CommandEnabled, filter.ToBoolean(formatProvider));
				}
			case nameof(Insight.TwinLocations):
				{
					return filter.CreateExpression((Insight v) => (string)(object)v.TwinLocations, filter.ToString(formatProvider));
				}
			case nameof(Insight.IsFaulty):
				{
					return filter.CreateExpression((Insight v) => v.IsFaulty, filter.ToBoolean(formatProvider));
				}
			case nameof(Insight.IsValid):
				{
					return filter.CreateExpression((Insight v) => v.IsValid, filter.ToBoolean(formatProvider));
				}
			case nameof(Insight.TimeZone):
				{
					return filter.CreateExpression((Insight v) => v.TimeZone, filter.ToString(formatProvider));
				}
			case nameof(Insight.CommandInsightId):
				{
					return filter.CreateExpression((Insight v) => v.CommandInsightId, filter.ToGuid(formatProvider));
				}
			case nameof(Insight.Status):
				{
					var status = filter.ToEnum<InsightStatus>(formatProvider);

					//all rules engine insights defaults to Open so therefor non-"synced" insights should be ignored
					if (status == InsightStatus.Open)
					{
						return filter.CreateExpression((Insight v) => v.Status, filter.ToEnum<InsightStatus>(formatProvider))
						.And(v => v.CommandEnabled == true);
					}

					return filter.CreateExpression((Insight v) => v.Status, filter.ToEnum<InsightStatus>(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	/// <inheritdoc />
	protected override IQueryable<Insight> ApplySort(IQueryable<Insight> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<Insight>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(Insight.IsFaulty): // Faulty
					{
						// Sorts by is valid then by is faulty so all invalid ones sort last
						result = AddSort(queryable, result!, first, x => !x.IsValid ? 0 : x.IsFaulty ? 2 : 1, sortSpecification.sort);
						break;
					}
				case nameof(Insight.IsValid): // Valid
					{
						result = AddSort(queryable, result!, first, x => x.IsValid, sortSpecification.sort);
						break;
					}
				case nameof(Insight.Status): // Status
					{
						//first order by commandenabled so they take priority
						result = AddSort(queryable, result!, first, x => x.CommandEnabled, "DESC");
						result = AddSort(queryable, result!, false, x => x.Status, sortSpecification.sort);
						break;
					}
				case nameof(Insight.RuleName): // Rule
					{
						result = AddSort(queryable, result!, first, x => x.RuleName, sortSpecification.sort);
						break;
					}
				case nameof(Insight.EquipmentName): // Equipment
					{
						result = AddSort(queryable, result!, first, x => x.EquipmentName, sortSpecification.sort);
						break;
					}
				case nameof(Insight.CommandEnabled): // Sync
					{
						result = AddSort(queryable, result!, first, x => x.CommandEnabled, sortSpecification.sort);
						break;
					}
				case nameof(Insight.EquipmentId): // Id
					{
						result = AddSort(queryable, result!, first, x => x.EquipmentId, sortSpecification.sort);
						break;
					}
				case nameof(Insight.CommandInsightId): // Command Id
					{
						result = AddSort(queryable, result!, first, x => x.CommandInsightId.ToString(), sortSpecification.sort);
						break;
					}
				case nameof(Insight.TimeZone): // TimeZone
					{
						result = AddSort(queryable, result!, first, x => x.TimeZone, sortSpecification.sort);
						break;
					}
				case nameof(Insight.Id): // (not in table)
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
				case nameof(Insight.LastUpdatedUTC): // (not in table)
					{
						result = AddSort(queryable, result!, first, x => x.LastUpdatedUTC, sortSpecification.sort);
						break;
					}
				case nameof(Insight.LastSyncDateUTC): // (not in table)
					{
						result = AddSort(queryable, result!, first, x => x.LastSyncDateUTC, sortSpecification.sort);
						break;
					}
				case nameof(Insight.TwinLocations):
					{
						result = AddSort(queryable, result!, first, x => (string)(object)x.TwinLocations, sortSpecification.sort);
						break;
					}
				default:
				case nameof(Insight.LastFaultedDate): // Default to Last Faulted
					{
						result = AddSort(queryable, result!, first, x => x.LastFaultedDate, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	/// <summary>
	/// Get rule instances that match on location using a string search into the JSON
	/// </summary>
	/// <remarks>
	/// So, this is not ideal but having chosen to put the array in as JSON it seems to be the only way
	/// to get a query to work against it.
	/// </remarks>
	public async Task<IEnumerable<Insight>> GetByLocation(string location)
	{
		location = $"%{location}%";
		// See https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
		var items = await this.dbSet
			.FromSqlInterpolated<Insight>($"SELECT * FROM [Insight] WHERE [Locations] LIKE {location}")
			.ToListAsync();
		return items;
	}

	/// <summary>
	/// Get rule instances that match on feeds using a string search into the JSON
	/// </summary>
	/// <remarks>
	/// So, this is not ideal but having chosen to put the array in as JSON it seems to be the only way
	/// to get a query to work against it.
	/// </remarks>
	public async Task<IEnumerable<Insight>> GetFeeds(string twinId)
	{
		twinId = $"%{twinId}%";
		// See https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
		var items = await this.dbSet
			.FromSqlInterpolated<Insight>($"SELECT * FROM [Insight] WHERE [Feeds] LIKE {twinId}")
			.ToListAsync();
		return items;
	}

	/// <summary>
	/// Get rule instances that match on isFedBy using a string search into the JSON
	/// </summary>
	/// <remarks>
	/// So, this is not ideal but having chosen to put the array in as JSON it seems to be the only way
	/// to get a query to work against it.
	/// </remarks>
	public async Task<IEnumerable<Insight>> GetFedBy(string twinId)
	{
		twinId = $"%{twinId}%";
		// See https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
		var items = await this.dbSet
			.FromSqlInterpolated<Insight>($"SELECT * FROM [Insight] WHERE [FedBy] LIKE {twinId}")
			.ToListAsync();
		return items;
	}

	public async Task<int> SyncWithCommand(string insightId, bool enabled)
	{
		logger.LogInformation($"Set insight {insightId} to sync: {enabled}");
		int result = await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Insight] SET CommandEnabled={enabled} WHERE [Insight].[Id]={insightId}");
		InvalidateOne(insightId);
		return result;
	}

	/// <summary>
	/// Set the command insight id for the insight and also marks the insight as CommandEnabled
	/// and update the status from Command to Open, Ackowledged, ...
	/// </summary>
	public async Task<int> SetCommandInsightId(string insightId, Guid commandInsightId, InsightStatus status)
	{
		int result;

		result = await this.rulesContext.Database
				.ExecuteSqlInterpolatedAsync($"UPDATE [Insight] SET CommandInsightId={commandInsightId}, Status={status} WHERE [Insight].[Id]={insightId}");

		if (result > 0)
		{
			InvalidateOne(insightId);
		}

		return result;
	}

	public async Task<int> RemoveAllInsightsForRule(string ruleId)
	{
		logger.LogInformation($"Remove all insights for rule {ruleId}", ruleId);

		int result = 0;

		await ExecuteAsync(async () =>
		{
			// Clear InsightImpactScores
			await this.rulesContext.Database
				.ExecuteSqlInterpolatedAsync(
					@$"SET NOCOUNT OFF;
					DELETE [InsightImpactScore]	FROM [InsightImpactScore] iis
					INNER JOIN [Insight] i ON i.Id = iis.InsightId
					WHERE i.RuleId = {ruleId} SELECT @@ROWCOUNT");

			// Clear InsightOccurrence
			await this.rulesContext.Database
				.ExecuteSqlInterpolatedAsync(
					@$"SET NOCOUNT OFF;
					DELETE [InsightOccurrence]	FROM [InsightOccurrence] iis
					INNER JOIN [Insight] i ON i.Id = iis.InsightId
					WHERE i.RuleId = {ruleId} SELECT @@ROWCOUNT");

			// Clear InsightChanges
			await this.rulesContext.Database
				.ExecuteSqlInterpolatedAsync(
					@$"SET NOCOUNT OFF;
					DELETE [InsightChanges]	FROM [InsightChanges] iis
					INNER JOIN [Insight] i ON i.Id = iis.InsightId
					WHERE i.RuleId = {ruleId} SELECT @@ROWCOUNT");

			// No index on RuleId but that's OK
			result = await this.rulesContext.Database
				.ExecuteSqlInterpolatedAsync($"DELETE FROM [Insight] WHERE [Insight].[RuleId]={ruleId}");

			this.InvalidateCache();
		});

		return result;
	}

	public async Task RemoveAll()
	{
		logger.LogInformation($"Remove all insights");

		await ExecuteAsync(async () =>
		{
			await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [Insight]");
			await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [InsightImpactScore]");
			await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [InsightChanges]");
			await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [InsightOccurrence]");
		});

		await ExecuteAsync(async () =>
		{
			await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE [RuleMetadata] SET InsightsGenerated = 0");
		});
	}

	public async Task<IEnumerable<(Guid siteId, int count)>> GetSiteIds()
	{
		var items = await (from p in this.dbSet
						   where p.SiteId != null && p.SiteId != Guid.Empty
						   group p by p.SiteId into g
						   select new
						   {
							   key = g.Key,
							   count = g.Count()
						   })
					.ToListAsync();

		return items.Select(v => (v.key!.Value, v.count)).ToArray();
	}

	public async Task<IEnumerable<(string equipmentId, int count)>> GetEquipmentIds()
	{
		var items = await memoryCache.GetOrCreateAsync(GetCacheKey("Insights_GetEquipmentIds"), (c) =>
		{
			c.AbsoluteExpirationRelativeToNow = cacheExpiration;

			return (from p in this.dbSet
					where p.IsFaulty
					group p by p.EquipmentId into g
					select new
					{
						key = g.Key,
						count = g.Count()
					})
					.ToListAsync();
		});

		return items!.Select(v => (v.key, v.count)).ToArray();
	}

	public async Task<List<(string FieldId, string Name, string[] Units, int Count)>> GetImpactScoreNames(string ruleId)
	{
		var result = await memoryCache.GetOrCreateAsync($"ruleimpactscoresummary_{ruleId}", async (c) =>
		{
			c.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

			var results = await (
				from rule in rulesContext.Rules
				join insight in rulesContext.Insights on rule.Id equals insight.RuleId
				join impactScore in rulesContext.ImpactScores on insight.Id equals impactScore.InsightId
				where rule.Id == ruleId
				group impactScore by new { impactScore.FieldId } into grouped
				select new
				{
					grouped.Key.FieldId,
					grouped.First().Name,
					Units = grouped.Select(x => x.Unit).Distinct().ToArray(),
					Count = grouped.Count()
				}).ToListAsync();

			return results.Select(s => (s.FieldId, s.Name, s.Units, s.Count)).ToList();
		});

		return result!;
	}

	public async Task<List<(string FieldId, string Name, string[] Units, int Count)>> GetImpactScoreNames()
	{
		var result = await memoryCache.GetOrCreateAsync("impactscoresummary", async (c) =>
		{
			c.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

			var summaries = await rulesContext.ImpactScores
				.GroupBy(m => new { m.FieldId, m.Unit })
				.Select(m => new { fieldId = m.Key.FieldId, name = m.First().Name, unit = m.Key.Unit, count = m.Count() })
				.ToListAsync();

			// And now combine the ones with the same fieldId
			var summaryResult = summaries.GroupBy(s => s.fieldId)
				.Select(s => (fieldId: s.Key, s.First().name, units: s.Select(x => x.unit).Distinct().ToArray(), count: s.Sum(x => x.count)))
				.ToList();

			return summaryResult;
		});

		return result!;
	}

	public async Task<int> CheckValidInsights()
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Checking invalid insights"))
		{
			try
			{
				return await ExecuteAsync(async () =>
				{
					//flag insight to invalid if it's rule instance is failed
					int count = await this.rulesContext.Database
						.ExecuteSqlInterpolatedAsync(@$"SET NOCOUNT OFF;
								UPDATE [Insight]
									SET IsValid = 0
								FROM [Insight] i
								INNER JOIN [RuleInstance] ri ON ri.Id = i.Id
								WHERE ri.Status & 1 <> 1 and i.IsValid = 1
								SELECT @@ROWCOUNT");

					if (count > 0)
					{
						logger.LogInformation("{count} insights set to invalid", count);
					}

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to CheckInvalidInsights");
				return 0;
			}
		}
	}

	public async Task<int> DeleteOldImpactScores()
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Remove old impact scores"))
		{
			int count = 0;

			try
			{
				await ExecuteAsync(async () =>
				{
					var currentCount = 0;
					var batchSize = 100;

					do
					{
						//only consider impact scores where the insight was updated
						currentCount = await this.rulesContext.Database
							.ExecuteSqlInterpolatedAsync(@$"SET NOCOUNT OFF;
								DELETE TOP ({batchSize}) [InsightImpactScore]
								FROM [InsightImpactScore] iis
								LEFT JOIN Insight i ON i.Id = iis.InsightId
								WHERE iis.[LastUpdated] <> i.LastUpdated
								SELECT @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} impact scores deleted so far", count);
					}
					while (currentCount > 0);

					if (count > 0)
					{
						logger.LogWarning("Deleted {count} old impact scores", count);
					}

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to DeleteImpactScoresBefore");
				return 0;
			}

			return count;
		}
	}

	public async Task<IEnumerable<(string id, bool enabled, Guid commandId, InsightStatus status, DateTimeOffset lastSyncDate)>> GetCommandValues()
	{
		var items = await (from p in this.dbSet
						   select new
						   {
							   id = p.Id,
							   enabled = p.CommandEnabled,
							   commandId = p.CommandInsightId,
							   status = p.Status,
							   lastSyncDate = p.LastSyncDateUTC
						   })
				.ToListAsync();

		return items.Select(v => (v.id, v.enabled, v.commandId, v.status, v.lastSyncDate));
	}

	public async Task<IEnumerable<Insight>> GetInsightsForRule(string ruleId)
	{
		return await this.dbSet
			.Where(v => v.RuleId == ruleId)
			.ToListAsync();
	}

	public Task<int> GetInsightCountForRule(string ruleId)
	{
		return this.dbSet
			.CountAsync(v => v.RuleId == ruleId);
	}

	public IAsyncEnumerable<Insight> GetOrphanedInsights()
	{
		//dont auto delete orphaned insights too quickly just in case there was a bug introduced or ri's got deleted accidentally
		var orphanedOlderThanDate = DateTime.UtcNow.AddDays(-15);
		var invalidOlderThanDate = DateTime.UtcNow.AddHours(-24);


		var result = (from insight in this.rulesContext.Insights
					  join ruleInstance in this.rulesContext.RuleInstances
						  on insight.Id equals ruleInstance.Id into insights
					  from ruleInstance in insights.DefaultIfEmpty() //This line indicates a left join
					  where (ruleInstance == null && insight.LastUpdatedUTC < orphanedOlderThanDate) || (ruleInstance.Status != RuleInstanceStatus.Valid && insight.LastUpdatedUTC < invalidOlderThanDate)
					  select insight);

		return result.ToAsyncEnumerable();
	}

	public async Task<int> DeleteOccurrences(int maxCount)
	{
		using (var timed = logger.TimeOperationOver(TimeSpan.FromSeconds(30), "Remove old occurrences"))
		{
			int count = 0;

			try
			{
				await ExecuteAsync(async () =>
				{
					var currentCount = 0;
					var batchSize = 100;

					do
					{
						currentCount = await this.rulesContext.Database
							.ExecuteSqlInterpolatedAsync(@$"SET NOCOUNT OFF;
								WITH cte AS (
								  SELECT Id, InsightId, ROW_NUMBER() OVER (PARTITION BY InsightId ORDER BY Ended desc) as RowNumber, Ended FROM [InsightOccurrence]
								)
								DELETE TOP({batchSize}) FROM [InsightOccurrence]
								WHERE Id IN (SELECT Id FROM cte WHERE RowNumber > {maxCount});
								SELECT @@ROWCOUNT");

						count += currentCount;
					}
					while (currentCount > 0);

					if (count > 0)
					{
						logger.LogInformation("{count} Occurrences pruned for max of {max}", count, maxCount);
					}

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to DeleteOccurrences");
				return 0;
			}

			return count;
		}
	}

	public async Task<int> DeleteOccurrencesBefore(DateTimeOffset date)
	{
		using (var timed = logger.TimeOperationOver(TimeSpan.FromSeconds(30), "Remove old occurrences"))
		{
			int count = 0;

			try
			{
				await ExecuteAsync(async () =>
				{
					var currentCount = 0;
					var batchSize = 100;

					do
					{
						currentCount = await this.rulesContext.Database
							.ExecuteSqlInterpolatedAsync(@$"SET NOCOUNT OFF;
							DELETE TOP ({batchSize}) [InsightOccurrence]
							FROM [InsightOccurrence] iis
							WHERE iis.[Ended] < {date}
							SELECT @@ROWCOUNT");

						count += currentCount;
					}
					while (currentCount > 0);

					if (count > 0)
					{
						logger.LogInformation("{count} Occurrences pruned before date {date}", count, date);
					}

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to DeleteOccurrencesAfter");
				return 0;
			}

			return count;
		}

	}
	private async Task<int> DeleteOccurrencesAfter(string insightId, DateTimeOffset startDate)
	{
		using (var timed = logger.TimeOperationOver(TimeSpan.FromSeconds(30), "Remove old occurrences"))
		{
			int count = 0;

			try
			{
				await ExecuteAsync(async () =>
				{
					var currentCount = 0;
					var batchSize = 100;

					do
					{
						currentCount = await this.rulesContext.Database
							.ExecuteSqlInterpolatedAsync(@$"SET NOCOUNT OFF;
							DELETE TOP ({batchSize}) [InsightOccurrence]
							FROM [InsightOccurrence] iis
							WHERE iis.[InsightId] = {insightId} AND  iis.[Ended] > {startDate}
							SELECT @@ROWCOUNT");

						count += currentCount;
					}
					while (currentCount > 0);

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to DeleteOccurrencesAfter");
				return 0;
			}

			return count;
		}
	}

	public async Task DisableAllInsights()
	{
		int enabled = 0;

		int result = await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Insight] SET CommandEnabled={enabled}");

		result += await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Commands] SET Enabled={enabled}");

		await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Rule] SET CommandEnabled={enabled}");

		await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [RuleInstance] SET CommandEnabled={enabled}");

		this.InvalidateCache();
	}

	public async Task DeleteOccurrences(string ruleId = "")
	{
		logger.LogInformation("Deleting all Insight Occurrences ruleid {ruleId}", ruleId);

		if (!string.IsNullOrEmpty(ruleId))
		{
			await ExecuteAsync(async () =>
			{
				await this.rulesContext.Database
					.ExecuteSqlInterpolatedAsync(
						@$"SET NOCOUNT OFF;
					DELETE [InsightOccurrence]	FROM [InsightOccurrence] iis
					INNER JOIN [Insight] i ON i.Id = iis.InsightId
					WHERE i.RuleId = {ruleId} SELECT @@ROWCOUNT");
			});
		}
		else
		{
			await ExecuteAsync(async () =>
			{
				await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [InsightOccurrence]");
			});
		}
	}
}
