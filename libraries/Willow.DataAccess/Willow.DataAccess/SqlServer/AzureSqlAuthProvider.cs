namespace Willow.DataAccess.SqlServer;

using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

/// <summary>
/// An authentication provider for Azure Sql.
/// </summary>
public class AzureSqlAuthProvider : Microsoft.Data.SqlClient.SqlAuthenticationProvider
{
    private static readonly string[] AzureSqlScopes =
    {
        "https://database.windows.net//.default",
    };

    private readonly TokenCredential credential;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSqlAuthProvider"/> class.
    /// </summary>
    /// <param name="credential">The token credential for the connection.</param>
    public AzureSqlAuthProvider(TokenCredential? credential = null)
    {
        this.credential = credential ?? new DefaultAzureCredential();
    }

    /// <summary>
    /// Acquire a token for the connection.
    /// </summary>
    /// <param name="parameters">The SQL authentication parameters.</param>
    /// <returns>A asynchronous task with the SQL Authentication Token.</returns>
    public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
    {
        var tokenRequestContext = new TokenRequestContext(AzureSqlScopes);
        var tokenResult = await credential.GetTokenAsync(tokenRequestContext, default);
        return new SqlAuthenticationToken(tokenResult.Token, tokenResult.ExpiresOn);
    }

    /// <summary>
    /// Checks if the authentication method is supported.
    /// </summary>
    /// <param name="authenticationMethod">The requested authentication method.</param>
    /// <returns>True is the method is supported. False otherwise.</returns>
    public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
        => authenticationMethod.Equals(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity);
}
