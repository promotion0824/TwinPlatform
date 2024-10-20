namespace Willow.LiveData.TelemetryStreaming.Infrastructure;

using System.Text;
using Azure.Identity;

/// <summary>
/// Gets Azure access tokens for Event Grid.
/// </summary>
internal static class TokenProvider
{
    /// <summary>
    /// Gets a token synchronously.
    /// </summary>
    /// <returns>The token as a byte array.</returns>
    public static byte[] GetToken()
    {
        var credential = new DefaultAzureCredential();

        var token = credential.GetToken(new Azure.Core.TokenRequestContext(["https://eventgrid.azure.net/.default"]));

        return Encoding.UTF8.GetBytes(token.Token);
    }

    /// <summary>
    /// Gets a token asynchronously.
    /// </summary>
    /// <returns>The token as a byte array.</returns>
    public static async Task<byte[]> GetTokenAsync()
    {
        var credential = new DefaultAzureCredential();

        var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(["https://eventgrid.azure.net/.default"]), default);

        return Encoding.UTF8.GetBytes(token.Token);
    }
}
