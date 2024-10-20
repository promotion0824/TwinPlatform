using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Willow.Infrastructure.Database
{
    public class CustomAzureSqlExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        ILogger<CustomAzureSqlExecutionStrategy> logger
    ) : SqlServerRetryingExecutionStrategy(dependencies)
    {
        private int _attempt = 0;

        /// <summary>
        /// Check whether to retry or not.
        /// </summary>
        /// <param name="exception">Exception that triggered the retry check.</param>
        /// <returns>True to retry, false if not.</returns>
        protected override bool ShouldRetryOn(Exception exception)
        {
            // A network-related error with error: 40 is not a Sql Error code.
            // So check if the exception message contains part of the known message to retry
            var retry =
                (
                    exception is SqlException sqlException
                    && sqlException.Message.Contains(
                        "error: 40 - Could not open a connection to SQL Server"
                    )
                ) || base.ShouldRetryOn(exception);

            if (retry)
            {
                _attempt++;
                logger.LogInformation(
                    exception,
                    "Attempting to reconnect to database, retry attempt : {Retry}",
                    _attempt
                );
            }

            return retry;
        }
    }
}
