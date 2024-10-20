namespace Microsoft.Data.SqlClient
{
    using System;
    using ConnectorCore.Database;
    using Microsoft.Azure.Services.AppAuthentication;

    internal static class SqlConnectionExtensionsForAzure
    {
        private static readonly string[] ConflictKeywords = new[] { "Password=", "pwd=", "Integrated Security=", "Trusted_Connection=" };

        public static bool UseManagedIdentity(this SqlConnection sqlConnection)
        {
            var connectionString = sqlConnection.ConnectionString;
            foreach (var keyword in ConflictKeywords)
            {
                if (connectionString.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            var tokenProvider = new AzureServiceTokenProvider();
            sqlConnection.AccessToken = tokenProvider.GetAccessTokenAsync("https://database.windows.net/").Result;
            return true;
        }

        //TODO: Once tested, sql connection management for rest of the code base needs to refactored to use this method
        internal static async Task UseManagedIdentityAsync(this SqlConnection sqlConnection,
            ManagedIdentityTokenManager tokenManager,
            CancellationToken cancellationToken = default)
        {
            var connectionString = sqlConnection.ConnectionString;
            if (ConflictKeywords.Any(keyword => connectionString.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            sqlConnection.AccessToken = await tokenManager.GetTokenAsync(cancellationToken);
        }
    }
}
