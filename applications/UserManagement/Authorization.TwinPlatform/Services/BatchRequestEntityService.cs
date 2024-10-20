using Authorization.TwinPlatform.Persistence.Contexts;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Willow.Batch;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Base Class for all Entity Service.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class BatchRequestEntityService<TEntity, TModel>  where TEntity : class where TModel : class
{
    protected readonly TwinPlatformAuthContext _authContext;
    protected readonly IMapper _mapper;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="authContext">Twin Platform Auth Context.</param>
    /// <param name="mapper">IMapper Configuration.</param>
    protected BatchRequestEntityService(TwinPlatformAuthContext authContext, IMapper mapper)
    {
        _authContext = authContext;
        _mapper = mapper;
    }

    /// <summary>
    /// Get Batch Entities 
    /// </summary>
    /// <param name="batchRequestDto">Batch Request DTO.</param>
    /// <param name="systemFilter">Default System Filter to apply.</param>
    /// <param name="includeTotalCount">Get the total count before pagination and include in the response.</param>
    /// <returns>BatchDTO of TModel.</returns>
    public async Task<BatchDto<TModel>> GetBatchAsync(BatchRequestDto batchRequestDto, Expression<Func<TEntity, bool>>? systemFilter = null, bool includeTotalCount = false)
    {
        var queryable = _authContext.Set<TEntity>().AsQueryable().AsNoTracking();

        // Apply System Filter Expression to where
        queryable = systemFilter != null ? queryable.Where(systemFilter) : queryable;

        // Apply where filter
        queryable = ApplyWhere(queryable, batchRequestDto.FilterSpecifications);

        // Apply sort
        queryable = ApplySort(queryable, batchRequestDto.SortSpecifications);

        // Get Total Count before Pagination
        int totalCount = 0;
        if(includeTotalCount)
        {
            totalCount = await queryable.CountAsync();
        }

        // Apply pagination
        queryable = ApplyPagination(queryable, batchRequestDto.Page, batchRequestDto.PageSize, out _);

        // Project to Model Entity
        var projectedQueryable = queryable.ProjectTo<TModel>(_mapper.ConfigurationProvider);

        var response = await projectedQueryable.ToArrayAsync();

        if (!includeTotalCount)
        {
            totalCount = response.Length;
        }

        return new BatchDto<TModel>()
        {
            Items = response,
            Total = totalCount,
        };
    }

    /// <summary>
    /// Get Count of Entity
    /// </summary>
    /// <param name="filter">Filter to apply.</param>
    /// <returns>Count of entities.</returns>
    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        var queryable = _authContext.Set<TEntity>().AsQueryable().AsNoTracking();

        // Apply Filter Expression to where
        queryable = filter != null ? queryable.Where(filter) : queryable;

        return await queryable.CountAsync();
    }

    protected IQueryable<TEntity> ApplyWhere(IQueryable<TEntity> queryable, FilterSpecificationDto[] filterSpecifications)
    {
        if (filterSpecifications == null || filterSpecifications.Length == 0)
        {
            return queryable;
        }

        return queryable.FilterBy(filterSpecifications);

    }

    private static IQueryable<TEntity> ApplySort(IQueryable<TEntity> queryable, SortSpecificationDto[] sortSpecifications)
    {
        if (sortSpecifications == null || sortSpecifications.Length == 0)
            return queryable;
        return queryable.SortBy(sortSpecifications);
    }

    /// <summary>
    /// Adds skip and take to an Expression
    /// </summary>
    private static IQueryable<T> ApplyPagination<T>(IQueryable<T> queryable, int? page, int? take, out int skipped)
    {
        skipped = 0;

        if (page.HasValue && take.HasValue && page.Value > 0)
        {
            skipped = (page.Value - 1) * take.Value;

            queryable = queryable.Skip(skipped);
        }

        if (take.HasValue && take.Value > 0 && take.Value < 1000000)
        {
            queryable = queryable.Take(take.Value);
        }

        return queryable;
    }

}
