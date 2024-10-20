using System.Threading.Tasks;
using DirectoryCore.Infrastructure.Exceptions;
using Willow.Security.KeyVault;

namespace DirectoryCore.Services.AzureB2C
{
    /// <summary>
    /// A wrapper around a particular secret, facilitating the following pattern:
    ///
    /// <code>
    /// var wrapper = new SecretWrapper("my-secret", _secretManager);
    /// try
    /// {
    ///     await resiliencePipeline.ExecuteAsync(async cancellationToken =&gt;
    ///     {
    ///         var response = GetSomethingDependingOnTheSecret(await wrapper.GetCurrentValue());
    ///         if (response.Succeeded())
    ///         {
    ///            return response;
    ///         }
    ///         else if (response.IndicatesThatSecretShouldBeReloaded())
    ///         {
    ///            await wrapper.Failed();
    ///            throw new SomeException();
    ///         }
    ///         else
    ///         {
    ///            throw new SomeMaybeOtherExceptionWhoKnows();
    ///         }
    ///     });
    /// }
    /// finally
    /// {
    ///     await secretWrapper.Reset();
    /// }
    /// </code>
    /// See
    /// https://willow.atlassian.net/wiki/spaces/RFC/pages/2624618728/Secret+Rotation+Improvements
    /// for more information.
    /// </summary>
    public class SecretWrapper
    {
        private readonly string _secretName;
        private string _currentValue;
        private readonly ISecretManager _secretManager;

        public SecretWrapper(string secretName, ISecretManager secretManager)
        {
            _secretName = secretName;
            _secretManager = secretManager;
        }

        /// <summary>
        /// Retrieves the current value of the secret from the secret manager if not cached
        /// </summary>
        /// <returns>The current value of the secret</returns>
        public async Task<string> GetCurrentValue()
        {
            return _currentValue ??= await RetrieveSecret();
        }

        /// <summary>
        /// Marks the secret as failed
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public async Task Failed()
        {
            await _secretManager.IncrementFailureAsync(_secretName);
            this._currentValue = null;
        }

        /// <summary>
        /// Reset the key state. This method is called after the operation on the key
        /// is complete (successful or not) to reset the key state such that the
        /// primary key is used at the start of the next operation
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public async Task Reset()
        {
            await _secretManager.ResetFailureAsync(_secretName);
        }

        private async Task<string> RetrieveSecret()
        {
            var clientSecretKvs = await _secretManager.GetSecretAsync(_secretName);
            if (clientSecretKvs == null)
            {
                throw new SecretNotFoundException(_secretName);
            }
            return clientSecretKvs.Value;
        }
    }
}
