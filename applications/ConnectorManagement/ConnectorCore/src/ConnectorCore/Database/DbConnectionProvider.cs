namespace ConnectorCore.Database;

using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using ConnectorCore.Infrastructure.HealthCheck;
using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;
using Willow.Telemetry;

/// <summary>
/// Provides a database connection for executing queries and interacting with a database.
/// </summary>
internal class DbConnectionProvider(
    IDbConnectionStringProvider connectionStringProvider,
    HealthCheckSql healthCheckSql,
    ILogger<DbConnectionProvider> logger,
    ManagedIdentityTokenManager tokenManager,
    IMetricsCollector metricsCollector)
    : IDbConnectionProvider
{
    private AsyncRetryPolicy SqlRetryPolicy =>
        Policy.Handle<SqlException>().WaitAndRetryAsync(3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, _, retryCount, _) =>
            {
                logger.LogWarning("Retry {RetryCount} failed due to: {Exception}", retryCount, exception.Message);
                metricsCollector.TrackMetric("SqlConnectionRetry",
                    retryCount,
                    MetricType.Counter,
                    "The number of retries for SQL connection attempts");
            });

    /// <summary>
    /// Retrieves a database connection for executing queries and interacting with a database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IDbConnection"/> object.</returns>
    public async Task<IDbConnection> GetConnection()
    {
        healthCheckSql.Current = HealthCheckSql.Healthy;
        Stopwatch managedIdentityTimer = null;
        Stopwatch openConnectionTimer = null;
        try
        {
            var connection = new SqlConnection(connectionStringProvider.GetConnectionString());

            managedIdentityTimer = Stopwatch.StartNew();
            await connection.UseManagedIdentityAsync(tokenManager);
            managedIdentityTimer.Stop();

            openConnectionTimer = Stopwatch.StartNew();
            await SqlRetryPolicy.ExecuteAsync(connection.OpenAsync);
            openConnectionTimer.Stop();

            metricsCollector.TrackMetric("SuccessfulSqlConnection",
                1,
                MetricType.Counter,
                "The number of successful SQL connections");

            return connection;
        }
        catch (Exception)
        {
            healthCheckSql.Current = HealthCheckSql.ConnectionFailed;

            //Stop the timers if they are still running when an exception occurs
            managedIdentityTimer?.Stop();
            openConnectionTimer?.Stop();

            metricsCollector.TrackMetric("FailedSqlConnection",
                1,
                MetricType.Counter,
                "The number of failed SQL connections");
            throw;
        }
        finally
        {
            //Ensure the timers are stopped before checking elapsed time
            if (managedIdentityTimer?.IsRunning ?? false)
            {
                managedIdentityTimer.Stop();
            }

            if (openConnectionTimer?.IsRunning ?? false)
            {
                openConnectionTimer.Stop();
            }

            metricsCollector.TrackMetric("ManagedIdentityTokenDuration",
                managedIdentityTimer?.ElapsedMilliseconds ?? 0,
                MetricType.Histogram,
                "The time taken to retrieve managed identity token for the database connection");
            metricsCollector.TrackMetric("DbConnectionOpenDuration",
                openConnectionTimer?.ElapsedMilliseconds ?? 0,
                MetricType.Histogram,
                "The time taken to open a database connection");
        }
    }
}
