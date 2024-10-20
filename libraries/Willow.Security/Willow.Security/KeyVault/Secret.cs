namespace Willow.Security.KeyVault
{
    using Azure.Security.KeyVault.Secrets;

    /// <summary>
    /// A cache entry for a secret that is stored in the secret manager.
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Secret"/> class.
        /// </summary>
        /// <param name="secretName">The name of the secret.</param>
        public Secret(string secretName)
        {
            Name = secretName;
        }

        /// <summary>
        /// Gets or sets the primary value for the secret.
        /// </summary>
        public KeyVaultSecret? PrimaryValue { get; set; }

        /// <summary>
        /// Gets or sets the secondary value for the secret.
        /// </summary>
        public KeyVaultSecret? SecondaryValue { get; set; }

        /// <summary>
        /// Gets the name of the secret.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the number of times the secret has been reloaded.
        /// </summary>
        public int ReloadAttempts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the primary value is active.
        /// </summary>
        public bool IsPrimaryActive { get; set; }

        /// <summary>
        /// Gets the active value for the secret.
        /// </summary>
        /// <returns>The currently active secret value.</returns>
        public KeyVaultSecret? GetActiveValue()
        {
            if (IsPrimaryActive)
            {
                return PrimaryValue;
            }

            return SecondaryValue;
        }
    }
}
