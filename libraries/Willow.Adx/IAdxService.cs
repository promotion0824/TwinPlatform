namespace Willow.Adx;

using System.Threading;

/// <summary>
/// Willow ADX query service.
/// </summary>
public interface IAdxService
{
    /// <summary>
    /// Execute Control query in ADX.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    Task ControlQueryAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute Control query in ADX and return result of type IEnumerable<typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The data structure to be returned.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<IEnumerable<T>> ControlQueryAsync<T>(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute Query in ADX and return result of type IEnumerable<typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The data structure to be returned.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute Query in ADX and return result of type IEnumerable<typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The data structure to be returned.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="parameters">The names and values of any string parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(string query, IDictionary<string, string> parameters, CancellationToken cancellationToken = default);
}
