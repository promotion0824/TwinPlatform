using System.Linq.Expressions;
using Willow.Batch;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Interface for retrieving entities using Willow.Pagination batch.
/// </summary>
/// <typeparam name="TEntity">Type of DBSet Entity.</typeparam>
/// <typeparam name="TModel">Type of DTO Model to Project to.</typeparam>
public interface IBatchRequestEntityService<TEntity,TModel> where TEntity : class where TModel : class
{
    /// <summary>
    /// Get Batch Entities 
    /// </summary>
    /// <param name="batchRequestDto">Batch Request DTO.</param>
    /// <param name="systemFilter">Default System Filter to apply.</param>
    /// <param name="includeTotalCount">Get the total count before pagination and include in the response.</param>
    /// <returns>BatchDTO of TModel.</returns>
    Task<BatchDto<TModel>> GetBatchAsync(BatchRequestDto batchRequestDto, Expression<Func<TEntity, bool>>? systemFilter = null, bool includeTotalCount = false);

    /// <summary>
    /// Get Count of Entity
    /// </summary>
    /// <param name="filter">Filter to apply.</param>
    /// <returns>Count of entities.</returns>
    Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter = null);
}
