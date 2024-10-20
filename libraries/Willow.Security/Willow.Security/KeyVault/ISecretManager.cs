namespace Willow.Security.KeyVault
{
    using Azure.Security.KeyVault.Secrets;

    /// <summary>
    /// An interface for a secret manager.
    /// </summary>
    public interface ISecretManager
    {
        /// <summary>
        /// Gets or sets the maximum number of times to attempt to reload a secret.
        /// </summary>
        int MaxReloadAttempts { get; set; }

        /// <summary>
        /// Get the secrets from the secret manager.
        /// </summary>
        /// <param name="key">The key for the secret.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<KeyVaultSecret?> GetSecretAsync(string key);

        /// <summary>
        /// When the client has encountered an error loading a secret, this method is called to determine if the manager should attempt to reload the secret.
        /// </summary>
        /// <param name="key">The name of the secret.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task IncrementFailureAsync(string key);

        /// <summary>
        /// Determines if the primary key is active.
        /// </summary>
        /// <param name="key">The name of the secret.</param>
        /// <returns>True if the primary secret is active. False otherwise.</returns>
        bool? IsPrimaryActive(string key);

        /// <summary>
        /// Resets the failure count for the secret after a successful usage of the secret.
        /// </summary>
        /// <param name="key">The secret name.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task ResetFailureAsync(string key);
    }
}
