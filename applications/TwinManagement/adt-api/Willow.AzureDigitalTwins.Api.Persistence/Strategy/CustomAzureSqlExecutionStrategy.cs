using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Willow.AzureDigitalTwins.Api.Persistence.Strategy;


/// <summary>
/// Custom Azure Sql Execution Strategy.
/// </summary>
public class CustomAzureSqlExecutionStrategy(ExecutionStrategyDependencies dependencies,
    int maxRetryCount,
    TimeSpan maxRetryDelay,
    IEnumerable<int>? errorNumbersToAdd,
    ILogger<CustomAzureSqlExecutionStrategy> logger)
    : SqlServerRetryingExecutionStrategy(dependencies, maxRetryCount, maxRetryDelay, errorNumbersToAdd)
{
    private int attempt = 0;

    /// <summary>
    /// Check whether to retry or not.
    /// </summary>
    /// <param name="exception">Exception that triggered the retry check.</param>
    /// <returns>True to retry, false if not.</returns>
    protected override bool ShouldRetryOn(Exception exception)
    {
        bool retry;
        // A network-related error with error: 40 is not a Sql Error code.
        // So check if the exception message contains part of the known message to retry
        if (exception is SqlException sqlException
            && sqlException.Message.Contains("error: 40 - Could not open a connection to SQL Server"))
        {
            retry = true;
        }
        else
        {
            retry = base.ShouldRetryOn(exception);
        }

        if (retry)
        {
            attempt++;
            logger.LogInformation(exception, "Attempting to reconnect to database, retry attempt : {Retry}", attempt);
        }

        return retry;
    }
}

