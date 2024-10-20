namespace Connector.XL.Infrastructure.HealthCheck;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

internal class DbHealthCheck : IHealthCheck
{
    private readonly HealthCheckSettings healthChecksSettings;

    public DbHealthCheck(IOptions<HealthCheckSettings> healthChecksSettings)
    {
        this.healthChecksSettings = healthChecksSettings.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (healthChecksSettings.Databases == null || healthChecksSettings.Databases?.Count == 0)
        {
            return HealthCheckResult.Healthy("No dependencies on databases");
        }

        // check database connection using odbc
        var databases = new Dictionary<string, HealthCheckResult>();
        foreach (var database in healthChecksSettings.Databases!)
        {
            var response = database.Type.ToLower() switch
            {
                "sqlserver" => await CheckSqlConnection(database.ConnectionString, cancellationToken),
                _ => HealthCheckResult.Unhealthy($"Database type {database.Type} not supported"),
            };
            databases[database.Name] = response;
        }

        //extract status add any additional description if available
        var metaData = new Dictionary<string, object>();

        foreach (var database in databases)
        {
            if (string.IsNullOrEmpty(database.Value.Description))
            {
                metaData.Add(database.Key, database.Value.Status.ToString());
            }
            else
            {
                metaData.Add(database.Key, database.Value.Status + ": " + database.Value.Description);
            }
        }

        return databases.Values.ToList() switch
        {
            { Count: 0 } => HealthCheckResult.Unhealthy("No dependent Databases are configured"),
            { Count: var count } when count == databases.Count(x => x.Value.Status == HealthStatus.Healthy) => HealthCheckResult.Healthy("All dependent Databases are healthy",
                data: metaData),
            { Count: var count } when count == databases.Count(x => x.Value.Status == HealthStatus.Unhealthy) => HealthCheckResult.Unhealthy(
                "All dependent Databases are unhealthy",
                data: metaData),
            _ => HealthCheckResult.Degraded("Some of the dependent Databases are not healthy", data: metaData),
        };
    }

    private static async Task<HealthCheckResult> CheckSqlConnection(string connectionString, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);

            var credential = new DefaultAzureCredential();
            var tokenRequestContext = new TokenRequestContext(["https://database.windows.net/.default"]);
            var accessToken = await credential.GetTokenAsync(tokenRequestContext, cancellationToken);

            connection.AccessToken = accessToken.Token;
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
