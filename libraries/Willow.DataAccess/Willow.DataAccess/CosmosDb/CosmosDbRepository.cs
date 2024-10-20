namespace Willow.DataAccess.CosmosDb;

using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

/// <summary>
/// A repository for CosmosDb.
/// </summary>
/// <typeparam name="T">A type to store or access in the respository.</typeparam>
public abstract class CosmosDbRepository<T> : ICosmosDbRepository<T>
{
    private readonly CosmosClient cosmosClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosDbRepository{T}"/> class.
    /// </summary>
    /// <param name="cosmosClient">An instance of the Cosmos DB client.</param>
    protected CosmosDbRepository(CosmosClient cosmosClient)
    {
        this.cosmosClient = cosmosClient;
    }

    /// <summary>
    /// Gets the name of the database to access.
    /// </summary>
    protected abstract string DatabaseName { get; }

    /// <summary>
    /// Gets the name of the container to access.
    /// </summary>
    protected abstract string ContainerName { get; }

    /// <inheritdoc/>
    public async Task Upsert(T entity, string partitionKey, CancellationToken cancellationToken = default)
    {
        _ = await GetContainer().UpsertItemAsync(entity, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T?> Get(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        var result = await GetContainer().ReadItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
        return result.Resource ?? default;
    }

    /// <inheritdoc/>
    public T? Get(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = GetContainer().GetItemLinqQueryable<T>(true).Where(predicate).AsEnumerable().FirstOrDefault();
        return result;
    }

    /// <inheritdoc/>
    public async Task<PagedItems<T>> GetAll(Expression<Func<T, bool>>? whereClause,
                                            string? continuationToken = null,
                                            int? itemCount = 25,
                                            CancellationToken cancellationToken = default)
    {
        var query = GetContainer().GetItemLinqQueryable<T>(true, continuationToken, new QueryRequestOptions { MaxItemCount = itemCount ?? -1 })
                                .Where(whereClause ?? ((_) => true));

        FeedResponse<T>? response = null;
        var results = new List<T>();
        using var iterator = query.ToFeedIterator();
        if (iterator.HasMoreResults)
        {
            response = await iterator.ReadNextAsync(cancellationToken);
            if (response is not null)
            {
                results.AddRange(response.ToList());
            }
        }

        return new PagedItems<T>(results, response?.ContinuationToken);
    }

    /// <inheritdoc/>
    public async Task Delete(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        await GetContainer().DeleteItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get the container to access.
    /// </summary>
    /// <returns>The container.</returns>
    protected Container GetContainer()
    {
        return cosmosClient.GetContainer(DatabaseName, ContainerName);
    }
}
