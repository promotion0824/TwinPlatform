namespace Willow.DataAccess.CosmosDb;

using System.Linq.Expressions;

/// <summary>
/// A repository for CosmosDb.
/// </summary>
/// <typeparam name="T">A type to store or access in the respository.</typeparam>
public interface ICosmosDbRepository<T>
{
    /// <summary>
    /// Updates or inserts an entity.
    /// </summary>
    /// <param name="entity">The entity to update or insert.</param>
    /// <param name="partitionKey">The partition key of the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Upsert(T entity, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an entity by id.
    /// </summary>
    /// <param name="id">The id of the entity to retrieve.</param>
    /// <param name="partitionKey">The partition key of the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Standard practice for repository pattern.")]
    Task<T?> Get(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an entity by predicate.
    /// </summary>
    /// <param name="predicate">The predicate to use to retrieve an entity.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>A nullable instance of the type requested.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Standard practice for repository pattern.")]
    T? Get(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities matching a predicate.
    /// </summary>
    /// <param name="whereClause">The predicate to use in the search.</param>
    /// <param name="continuationToken">A token representing the next record in the search to return.</param>
    /// <param name="itemCount">The maximum number of items to return in the result set. Defaults to 25.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<PagedItems<T>> GetAll(Expression<Func<T, bool>> whereClause,
                                string? continuationToken = null,
                                int? itemCount = 25,
                                CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an entity by id.
    /// </summary>
    /// <param name="id">The id of the entity to delete.</param>
    /// <param name="partitionKey">The partition key of the entity in storage.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(string id, string partitionKey, CancellationToken cancellationToken = default);
}
