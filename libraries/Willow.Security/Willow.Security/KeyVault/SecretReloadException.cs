namespace Willow.Security.KeyVault
{
    /// <summary>
    /// An exception that is thrown when a secret fails to reload.
    /// </summary>
    public class SecretReloadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecretReloadException"/> class.
        /// </summary>
        /// <param name="message">A custom message to add into the exception.</param>
        public SecretReloadException(string message)
            : base(message)
        {
        }
    }
}
