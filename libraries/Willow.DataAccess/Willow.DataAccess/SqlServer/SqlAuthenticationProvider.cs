namespace Willow.DataAccess.SqlServer;

using Azure.Core;
using Microsoft.Data.SqlClient;

/// <summary>
/// An authentication provider for Azure Sql.
/// </summary>
public class SqlAuthenticationProvider
{
    /// <summary>
    /// Set the SQL authentication provider for the SQL authentication.
    /// </summary>
    /// <param name="credential">Specifies a custom token credential. Defaults to DefaultAzureCredential.</param>
    public static void UseIdentityProvider(TokenCredential? credential = null)
    {
        Microsoft.Data.SqlClient.SqlAuthenticationProvider.SetProvider(
            SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
            new AzureSqlAuthProvider(credential));
    }
}
