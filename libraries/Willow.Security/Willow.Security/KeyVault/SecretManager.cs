namespace Willow.Security.KeyVault
{
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An implementation of a secret manager.
    /// </summary>
    public class SecretManager : ISecretManager
    {
        // The key for the keyed service.
        private const string ServiceKey = nameof(SecretManager);

        private readonly SecretClient secretClient;
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
        private readonly Dictionary<string, Secret> secrets = [];
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly
        private readonly Semaphore semaphore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretManager"/> class.
        /// Use keyed service for semaphore dependency injection.
        /// </summary>
        /// <param name="secretClient">An instance of a KeyVault Secret client.</param>
        /// <param name="semaphore">A shared semaphore for the entire process to ensure that if multiple instances of the secret manager are instantiated, only one will update the cache at any time.</param>
        public SecretManager(SecretClient secretClient, [FromKeyedServices(ServiceKey)] Semaphore semaphore)
        {
            this.secretClient = secretClient;
            this.semaphore = semaphore;
        }

        /// <inheritdoc />
        public int MaxReloadAttempts { get; set; } = 3;

        /// <inheritdoc />
        public async Task<KeyVaultSecret?> GetSecretAsync(string key)
        {
            semaphore.WaitOne();

            Secret? secret;
            try
            {
                if (!secrets.TryGetValue(key, out secret))
                {
                    secret = await CreateSecretAsync(key);
                    secrets.Add(key, secret);
                }
            }
            finally
            {
                semaphore.Release();
            }

            return secret.GetActiveValue();
        }

        /// <inheritdoc />
        public bool? IsPrimaryActive(string key)
        {
            if (secrets.TryGetValue(key, out var secret))
            {
                return secret.IsPrimaryActive;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task IncrementFailureAsync(string key)
        {
            semaphore.WaitOne();

            try
            {
                if (secrets.TryGetValue(key, out var secret))
                {
                    if (secret.IsPrimaryActive)
                    {
                        secret.IsPrimaryActive = false;
                    }
                    else
                    {
                        secret.ReloadAttempts++;

                        if (secret.ReloadAttempts > MaxReloadAttempts)
                        {
                            throw new SecretReloadException("Maximum number of secret reload attempts exceeded for secret " + secret.Name);
                        }

                        await ReloadSecretAsync(secret);
                    }
                }
                else
                {
                    secrets.Add(key, await CreateSecretAsync(key));
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task ResetFailureAsync(string key)
        {
            semaphore.WaitOne();

            try
            {
                if (secrets.TryGetValue(key, out var secret))
                {
                    secret.IsPrimaryActive = true;
                    secret.ReloadAttempts = 0;
                }
                else
                {
                    secrets.Add(key, await CreateSecretAsync(key));
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Get the semaphore and key to inject into the SecretManager.
        /// </summary>
        /// <returns>The semaphore and key required by the secret manager.</returns>
        public static (object Key, Semaphore Semaphore) GetKeyedSingletonDependencies()
        {
            return (ServiceKey, new Semaphore(1, 1));
        }

        private async Task<Secret> CreateSecretAsync(string key)
        {
            var secret = new Secret(key);
            await ReloadSecretAsync(secret);
            return secret;
        }

        private async Task ReloadSecretAsync(Secret secret)
        {
            try
            {
                secret.PrimaryValue = (await secretClient.GetSecretAsync(secret.Name + "-Primary")).Value;
                secret.IsPrimaryActive = true;
            }
            catch
            {
                secret.IsPrimaryActive = false;
            }

            try
            {
                secret.SecondaryValue = (await secretClient.GetSecretAsync(secret.Name + "-Secondary")).Value;
            }
            catch
            {
            }
        }
    }
}
