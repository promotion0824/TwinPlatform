using System;

namespace DirectoryCore.Infrastructure.Exceptions
{
    internal class SecretNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecretNotFoundException"/> class.
        /// </summary>
        /// <param name="secretName">The name of the secret that could not be found.</param>
        public SecretNotFoundException(string secretName)
            : base($"Secret {secretName} not found") { }
    }
}
