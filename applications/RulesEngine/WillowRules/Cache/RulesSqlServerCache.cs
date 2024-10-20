using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Logging;
using Willow.Rules.Repository;

namespace Willow.Rules.Cache
{
	/// <summary>
	/// An extended SqlServerCache that can also search cache keys
	/// </summary>
	public class RulesSqlServerCache : IRulesDistributedCache
	{
		/// <summary>
		/// Since there is no specific exception type representing a 'duplicate key' error, we are relying on
		/// the following message number which represents the following text in Microsoft SQL Server database.
		///     "Violation of %ls constraint '%.*ls'. Cannot insert duplicate key in object '%.*ls'.
		///     The duplicate key value is %ls."
		/// You can find the list of system messages by executing the following query:
		/// "SELECT * FROM sys.messages WHERE [text] LIKE '%duplicate%'"
		/// </summary>
		private const int DuplicateKeyErrorId = 2627;
		private static DateTimeOffset lastExpiryCheck;
		private const string tableName = "Cache";

		private readonly string connectionString;
		private readonly ILogger<RulesSqlServerCache> logger;
		private readonly TimeSpan deleteExpiredItemsInterval;
		private const int commandTimeout = 120;

		/// <summary>
		/// Creates a new <see cref="RulesSqlServerCache"/>
		/// </summary>
		/// <param name="connectionString">Databse connection string</param>
		/// <param name="logger">logger</param>
		/// <param name="deleteExpiredItemsInterval">The amount of time in intervals the the cache to expire deleted items</param>
		public RulesSqlServerCache(
			string connectionString,
			ILogger<RulesSqlServerCache> logger,
			TimeSpan? deleteExpiredItemsInterval = null)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
			}

			this.connectionString = connectionString;
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.deleteExpiredItemsInterval = deleteExpiredItemsInterval ?? TimeSpan.FromDays(1);
		}

		static RulesSqlServerCache()
		{
			lastExpiryCheck = DateTimeOffset.UtcNow;
		}

		/// <inheritdoc />
		public async Task RemoveAsync(string key)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				await sqlConnection.OpenAsync();

				using (var transaction = sqlConnection.BeginTransaction())
				{
					var query = $"delete from {tableName} where Id = @id";

					using (var command = new SqlCommand(query, sqlConnection, transaction)
					{
						CommandTimeout = commandTimeout
					})
					{
						command.Parameters.AddWithValue("@id", key);

						await command.ExecuteNonQueryAsync();
					}

					await transaction.CommitAsync();
				}
			}
		}

		public async Task<int> RemoveAsync(string startsWith, DateTimeOffset lastUpdated)
		{
			int totalDeleted = 0;

			try
			{
				var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(60));

				using (var timedLogger = logger.TimeOperation(TimeSpan.FromMinutes(5), "Deleting cache keys {keys} before {date}", startsWith, lastUpdated))
				{
					int batchSize = 100;
					int deleteCount = 0;
					int timeout = 0;

					await RepositoryBase.RetryPolicy.ExecuteAsync(async (c, t) =>
					{
						timeout += commandTimeout;
						try
						{
							deleteCount = await DeleteItemsAsync(batchSize, lastUpdated: lastUpdated, startsWith: startsWith, timeout: timeout);
						}
						finally
						{
							batchSize = 10;
						}
					}, new Dictionary<string, object>() { ["logger"] = logger }, CancellationToken.None);

					totalDeleted += deleteCount;

					while (deleteCount > 0)
					{
						batchSize = 100;
						timeout = 0;

						await RepositoryBase.RetryPolicy.ExecuteAsync(async (c, t) =>
						{
							timeout += commandTimeout;
							try
							{
								deleteCount = await DeleteItemsAsync(batchSize, lastUpdated: lastUpdated, startsWith: startsWith, timeout: timeout);
							}
							finally
							{
								batchSize = 10;
							}
						}, new Dictionary<string, object>() { ["logger"] = logger }, CancellationToken.None);

						totalDeleted += deleteCount;

						throttledLogger.LogInformation("Deleted {count} old cache entries", totalDeleted);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to remove keys starting with {key} and date {date}", startsWith, lastUpdated);
				return 0;
			}

			return totalDeleted;
		}

		/// <inheritdoc />
		public async Task<byte[]?> GetAsync(string key)
		{
			return await RepositoryBase.RetryPolicy.ExecuteAsync(async (c, t) =>
			{
				using var timed = logger.TimeOperationOver(TimeSpan.FromSeconds(15), "Getting cache entry {key}", key);

				byte[]? result = null;

				using (var sqlConnection = new SqlConnection(connectionString))
				{
					await sqlConnection.OpenAsync();

					using (var transaction = sqlConnection.BeginTransaction())
					{
						var query = $"select AbsoluteExpiration, Value from {tableName} where Id = @id";

						using (var command = new SqlCommand(query, sqlConnection, transaction)
						{
							CommandTimeout = commandTimeout
						})
						{
							command.Parameters.AddWithValue("@id", key);

							using (var reader = await command.ExecuteReaderAsync())
							{
								if (await reader.ReadAsync())
								{
									DateTimeOffset? absoluteExpiration = !reader.IsDBNull(0) ? reader.GetDateTimeOffset(0) : null;

									if (!absoluteExpiration.HasValue || (absoluteExpiration.Value > DateTimeOffset.UtcNow))
									{
										if (!reader.IsDBNull(1))
										{
											result = await reader.GetFieldValueAsync<byte[]>(1);
										}
									}
								}
							}
						}

						await transaction.CommitAsync();
					}
				}

				var utcNow = DateTimeOffset.UtcNow;

				if ((utcNow - lastExpiryCheck) > deleteExpiredItemsInterval)
				{
					lastExpiryCheck = utcNow;

					await DeleteItemsAsync(5, expiresAtTime: utcNow);
				}

				return result;
			}, new Dictionary<string, object>() { ["logger"] = logger }, CancellationToken.None);
		}

		/// <inheritdoc />
		public async Task SetAsync(string key, byte[] binary, TimeSpan? expirationRelativeToNow)
		{
			//if no expiry provided default to a year long
			expirationRelativeToNow = expirationRelativeToNow ?? TimeSpan.FromDays(365);

			var absoluteExpiration = DateTimeOffset.UtcNow.Add(expirationRelativeToNow.Value);

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				await sqlConnection.OpenAsync();

				using (var command = new SqlCommand(setCacheSql, sqlConnection)
				{
					CommandTimeout = commandTimeout
				})
				{
					command.Parameters.AddWithValue("@absoluteExpiration", absoluteExpiration);
					command.Parameters.AddWithValue("@value", binary);
					command.Parameters.AddWithValue("@id", key);

					try
					{
						await command.ExecuteNonQueryAsync();
					}
					catch (SqlException ex)
					{
						if (IsDuplicateKeyException(ex))
						{
							// There is a possibility that multiple requests can try to add the same item to the cache, in
							// which case we receive a 'duplicate key' exception on the primary key column.
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		/// <inheritdoc />
		public async IAsyncEnumerable<byte[]> GetAllValuesAsync(string startsWith)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				await sqlConnection.OpenAsync();

				using (var transaction = sqlConnection.BeginTransaction())
				{
					var query = $"select AbsoluteExpiration, Value from {tableName} where Id like @startsWith";

					using (var command = new SqlCommand(query, sqlConnection, transaction)
					{
						CommandTimeout = commandTimeout
					})
					{
						command.Parameters.AddWithValue("@startsWith", $"{startsWith}%");

						using (var reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								DateTimeOffset? absoluteExpiration = !reader.IsDBNull(0) ? reader.GetDateTimeOffset(0) : null;

								if (!absoluteExpiration.HasValue || (absoluteExpiration.Value > DateTimeOffset.UtcNow))
								{
									if (!reader.IsDBNull(1))
									{
										yield return await reader.GetFieldValueAsync<byte[]>(1);
									}
								}
							}
						}
					}

					await transaction.CommitAsync();
				}
			}
		}

		public async IAsyncEnumerable<string> GetAllKeysAsync(string? startsWith)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				await sqlConnection.OpenAsync();

				using (var transaction = sqlConnection.BeginTransaction())
				{
					var query = $"select Id from {tableName}";

					if (!string.IsNullOrEmpty(startsWith))
					{
						query = $"{query} where Id like @startsWith";
					}

					using (var command = new SqlCommand(query, sqlConnection, transaction)
					{
						CommandTimeout = commandTimeout
					})
					{
						if (!string.IsNullOrEmpty(startsWith))
						{
							command.Parameters.AddWithValue("@startsWith", $"{startsWith}%");
						}

						using (var reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								yield return reader.GetString(0);
							}
						}
					}

					await transaction.CommitAsync();
				}
			}
		}

		public async Task<int> CountAsync(string? startsWith)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				await sqlConnection.OpenAsync();

				var query = $"select count(*) from {tableName}";

				if (!string.IsNullOrEmpty(startsWith))
				{
					query = $"{query} where Id like @startsWith";
				}

				using (var command = new SqlCommand(query, sqlConnection)
				{
					CommandTimeout = commandTimeout
				})
				{
					if (!string.IsNullOrEmpty(startsWith))
					{
						command.Parameters.AddWithValue("@startsWith", $"{startsWith}%");
					}

					var result = await command.ExecuteScalarAsync();

					return Convert.ToInt32(result);
				}
			}
		}

		private async Task<int> DeleteItemsAsync(
			int batchSize,
			DateTimeOffset? expiresAtTime = null,
			DateTimeOffset? lastUpdated = null,
			string? startsWith = null,
			int timeout = 0)
		{
			int result = 0;

			try
			{
				using (var sqlConnection = new SqlConnection(connectionString))
				{
					await sqlConnection.OpenAsync();

					using (var transaction = sqlConnection.BeginTransaction())
					{
						var whereClause = new List<string>();

						if (lastUpdated.HasValue)
						{
							whereClause.Add($"LastUpdated <= @lastUpdated");
						}

						if (expiresAtTime.HasValue)
						{
							whereClause.Add($"ExpiresAtTime <= @maxDate");
						}

						if (!string.IsNullOrEmpty(startsWith))
						{
							whereClause.Add($"Id like @startsWith");
						}

						if (whereClause.Count == 0)
						{
							throw new InvalidOperationException("At least one filter required");
						}

						var sql = $"DELETE TOP ({batchSize}) FROM {tableName} WHERE {string.Join(" AND ", whereClause)}";

						using (var command = new SqlCommand(sql, sqlConnection, transaction)
						{
							CommandTimeout = timeout > 0 ? timeout : commandTimeout
						})
						{
							if (lastUpdated.HasValue)
							{
								command.Parameters.AddWithValue("@lastUpdated", lastUpdated.Value);
							}

							if (expiresAtTime.HasValue)
							{
								command.Parameters.AddWithValue("@maxDate", expiresAtTime);
							}

							if (!string.IsNullOrEmpty(startsWith))
							{
								command.Parameters.AddWithValue("@startsWith", $"{startsWith}%");
							}

							result = await command.ExecuteNonQueryAsync();
						}

						await transaction.CommitAsync();
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"Failed to execute expired items sql {ex.Message}");
			}

			return result;
		}

		private static bool IsDuplicateKeyException(SqlException ex)
		{
			if (ex.Errors != null)
			{
				return ex.Errors.Cast<SqlError>().Any(error => error.Number == DuplicateKeyErrorId);
			}

			return false;
		}

		private const string setCacheSql =
			$@"UPDATE {tableName} SET Value = @value, ExpiresAtTime = @absoluteExpiration, AbsoluteExpiration = @absoluteExpiration, LastUpdated = getutcdate()
			WHERE Id = @id
			IF (@@ROWCOUNT = 0)
			BEGIN
				INSERT INTO {tableName}
				(Id, Value, ExpiresAtTime, AbsoluteExpiration, LastUpdated)
				VALUES (@id, @value, @absoluteExpiration, @absoluteExpiration, getutcdate());
			END";
	}
}
