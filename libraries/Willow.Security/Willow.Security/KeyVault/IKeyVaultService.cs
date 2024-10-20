namespace Willow.Security.KeyVault;

using Azure.Security.KeyVault.Secrets;

/// <summary>
/// An interface for a key vault service.
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Gets all the secrets from the key vault.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IReadOnlyList<KeyVaultSecret>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the secrets from the key vault with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to search for.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of threads to use to get the secrets.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IReadOnlyList<KeyVaultSecret>> GetAll(string prefix,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the secrets from the key vault as a dictionary.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IReadOnlyDictionary<string, string>> GetAllAsDictionary(CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts a secret into the key vault.
    /// </summary>
    /// <param name="key">The name of the secret.</param>
    /// <param name="value">The value of the secret.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Uri> Upsert(string key,
        string value,
        CancellationToken cancellationToken = default);
}
