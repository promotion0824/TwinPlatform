namespace Willow.Security.KeyVault;

using System.Collections.Concurrent;
using Azure.Security.KeyVault.Secrets;

/// <summary>
/// An implementation of a key vault service.
/// </summary>
public class KeyVaultService : IKeyVaultService
{
    private const int DefaultDegreeOfParallelism = 10;
    private readonly SecretClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyVaultService"/> class.
    /// </summary>
    /// <param name="client">An instance of a KeyVault secret client.</param>
    public KeyVaultService(SecretClient client)
    {
        this.client = client;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<KeyVaultSecret>> GetAll(CancellationToken cancellationToken = default) =>
        await GetAll(string.Empty, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<KeyVaultSecret>> GetAll(string prefix,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default)
    {
        var secrets = new ConcurrentBag<KeyVaultSecret>();
        var allSecrets = client.GetPropertiesOfSecretsAsync(cancellationToken);

        await Parallel.ForEachAsync(allSecrets,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? DefaultDegreeOfParallelism },
            async (properties, token) =>
            {
                if (properties.Name.StartsWith(prefix, StringComparison.InvariantCulture) || string.IsNullOrEmpty(prefix))
                {
                    var response = await client.GetSecretAsync(properties.Name, cancellationToken: token);

                    if (response is not null)
                    {
                        secrets.Add(response.Value);
                    }
                }
            });

        return secrets.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, string>> GetAllAsDictionary(CancellationToken cancellationToken = default)
    {
        var secrets = new Dictionary<string, string>();
        var allSecrets = client.GetPropertiesOfSecretsAsync(cancellationToken);

        await foreach (var secretProperties in allSecrets)
        {
            var response = await client.GetSecretAsync(secretProperties.Name, cancellationToken: cancellationToken);

            if (response is not null)
            {
                secrets.Add(response.Value.Properties.Name, response.Value.Value);
            }
        }

        return secrets;
    }

    /// <inheritdoc />
    public async Task<Uri> Upsert(string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        var response = await client.SetSecretAsync(key, value, cancellationToken);
        return response.Value.Properties.Id;
    }
}
