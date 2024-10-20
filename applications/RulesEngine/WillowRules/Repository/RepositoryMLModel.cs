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
using WillowRules.Extensions;

namespace Willow.Rules.Repository;

/// <summary>
/// Repository for ml models
///</summary>
///<remarks>
/// Many of these methods are sync instead of async. There are seemingly issues with async when large binary blobs are involved. The queries never return.
/// </remarks>
public interface IRepositoryMLModel : IRepositoryBase<MLModel>
{
	/// <summary>
	/// Search models without retrieving model binary
	/// </summary>
	Task<Batch<MLModel>> GetAllModels(SortSpecificationDto[] sortSpecifications, FilterSpecificationDto[] filterSpecifications, Expression<Func<MLModel, bool>>? whereExpression = null, int? page = null, int? take = null);

	/// <summary>
	/// Search models without retrieving model binary
	/// </summary>
	IAsyncEnumerable<MLModel> GetModelsWithoutBinary();

	/// <summary>
	/// Gets a single model synchronously. Async gives trouble with large binary files
	/// </summary>
	/// <remarks>
	/// https://github.com/dotnet/efcore/issues/18221 and https://github.com/dotnet/efcore/issues/885
	/// </remarks>
	MLModel? GetModel(string id);

	/// <summary>
	/// Gets a single model without binary
	/// </summary>
	MLModel? GetModelWithoutBinary(string id);

	/// <summary>
	/// Gets all model names from DB
	/// </summary>
	/// <returns></returns>
	Task<IEnumerable<(string id, string name)>> GetModelNames();

	/// <summary>
	/// Get All Models with Binary
	/// </summary>
	/// <returns></returns>
	IEnumerable<MLModel> GetAllModels();
}

/// <summary>
/// A repository for <see cref="MLModel"/>s
/// </summary>
public class RepositoryMLModel : RepositoryBase<MLModel>, IRepositoryMLModel
{
	public RepositoryMLModel(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryMLModel> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.MLModels, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<MLModel> ApplySort(IQueryable<MLModel> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<MLModel>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(MLModel.FullName):
					{
						result = AddSort(queryable, result!, first, x => x.FullName, sortSpecification.sort);
						break;
					}
				default:
				case nameof(MLModel.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task<Batch<MLModel>> GetAllModels(SortSpecificationDto[] sortSpecifications, FilterSpecificationDto[] filterSpecifications, Expression<Func<MLModel, bool>>? whereExpression = null, int? page = null, int? take = null)
	{
		whereExpression = BuildWhereClause(filterSpecifications, whereExpression);

		var queryable = whereExpression is null ? dbSet : dbSet.Where(whereExpression);

		queryable = ApplyBatchParams(queryable, sortSpecifications);
		//exclude binary from search
		var batch = from cp in queryable
					select new MLModel
					{
						ExtensionData = cp.ExtensionData,
						FullName = cp.FullName,
						Id = cp.Id,
						ModelName = cp.ModelName,
						ModelVersion = cp.ModelVersion
					};

		var result = await GetOrCreateBatch(batch, (v) => v.Id, page, take);

		return result;
	}

	public IEnumerable<MLModel> GetAllModels()
	{
		return GetQueryable().ToList();
	}

	protected override Expression<Func<MLModel, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(MLModel.Id):
				{
					return filter.CreateExpression((MLModel v) => v.Id, filter.ToString(formatProvider));
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
	public override IQueryable<MLModel> WithArrays(IQueryable<MLModel> input)
	{
		return input;
	}

	public MLModel? GetModel(string id)
	{
		return GetQueryable().FirstOrDefault(v => v.Id == id);
	}

	public async Task<IEnumerable<(string id, string name)>> GetModelNames()
	{
		var batch = await (from cp in dbSet
					select new
					{
						cp.FullName,
						cp.Id,
					}).ToListAsync();

		return batch.Select(v => (v.Id, v.FullName));
	}

	public IAsyncEnumerable<MLModel> GetModelsWithoutBinary()
	{
		return GetModelsWithoutBinary(GetQueryable()).ToAsyncEnumerable();
	}

	private static IQueryable<MLModel> GetModelsWithoutBinary(IQueryable<MLModel> queryable)
	{
		return from cp in queryable
			   select new MLModel
			   {
				   ExtensionData = cp.ExtensionData,
				   FullName = cp.FullName,
				   Id = cp.Id,
				   ModelName = cp.ModelName,
				   ModelVersion = cp.ModelVersion
			   };
	}

	public MLModel? GetModelWithoutBinary(string id)
	{
		return GetModelsWithoutBinary(GetQueryable().Where(v => v.Id == id)).FirstOrDefault();
	}
}
