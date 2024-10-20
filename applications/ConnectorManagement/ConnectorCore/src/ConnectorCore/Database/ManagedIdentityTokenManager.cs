namespace ConnectorCore.Database;

using Azure.Core;

/// <summary>
/// Class for managing the tokens obtained from a Managed Identity.
/// </summary>
internal class ManagedIdentityTokenManager(TokenCredential tokenCredential)
{
    private AccessToken currentToken;

    /// <summary>
    /// Retrieves an access token asynchronously using Managed Identity.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the access token.</returns>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        //Refreshing 5 minutes before expiry
        if (currentToken.ExpiresOn < DateTimeOffset.UtcNow.AddMinutes(5))
        {
            currentToken = await tokenCredential.GetTokenAsync(new TokenRequestContext([
                "https://database.windows.net/.default"
            ]),
                cancellationToken);
        }

        return currentToken.Token;
    }
}
