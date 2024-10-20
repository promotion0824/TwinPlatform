namespace Willow.DataAccess.SqlServer;

using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

/// <summary>
/// The sql connection extensions.
/// </summary>
public static class SqlConnectionExtensions
{
    private static readonly string[] ConflictKeywords = new[] { "Password=", "pwd=", "Integrated Security=", "Trusted_Connection=" };
    private static readonly string[] Scopes = new[] { "https://database.windows.net/.default" };

    /// <summary>
    /// Use managed identity to connect to the database.
    /// </summary>
    /// <param name="sqlConnection">The SQL Connection.</param>
    /// <returns>True if the connection was successfully configured. False otherwise.</returns>
    public static bool UseManagedIdentity(this SqlConnection sqlConnection)
    {
        var connectionString = sqlConnection.ConnectionString;

        if (ConflictKeywords.Any(k => connectionString.Contains(k, StringComparison.InvariantCultureIgnoreCase)))
        {
            return false;
        }

        var credential = new DefaultAzureCredential();
        var token = credential.GetToken(new TokenRequestContext(Scopes));
        sqlConnection.AccessToken = token.Token;

        return true;
    }
}
