using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace RulesEngine.Processor.Services;

/// <summary>
/// Runs diagnostics logs
/// </summary>
public interface IDiagnosticsProcessor
{
	/// <summary>
	/// Logs diagnostic information
	/// </summary>
	Task RunDiagnostics(RuleExecutionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Runs diagnostics logs
/// </summary>
public class DiagnosticsProcessor : IDiagnosticsProcessor
{
	private readonly IDataCacheFactory dataCacheFactory;
	private readonly ILogger<DiagnosticsProcessor> logger;
	private readonly CustomerOptions customerOptions;
	private readonly IRepositoryProgress repositoryProgress;

	/// <summary>
	/// Creates a new <see cref="DiagnosticsProcessor"/>
	/// </summary>
	public DiagnosticsProcessor(
		IDataCacheFactory dataCacheFactory,
		IRepositoryProgress repositoryProgress,
		IOptions<CustomerOptions> customerOptions,
		ILogger<DiagnosticsProcessor> logger)
	{
		this.dataCacheFactory = dataCacheFactory ?? throw new ArgumentNullException(nameof(dataCacheFactory));
		this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
		this.customerOptions = customerOptions?.Value ?? throw new ArgumentNullException(nameof(customerOptions));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Logs diagnostic information
	/// </summary>
	public async Task RunDiagnostics(RuleExecutionRequest request, CancellationToken cancellationToken)
	{
		logger.LogInformation("Running Diagnostics");

		var progressTracker = new ProgressTracker(repositoryProgress, Progress.RunDiagnosticsId, ProgressType.RunDiagnostics, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		await progressTracker.Start();

		int totalSteps = 2;
		await progressTracker.SetValues("Step", 0, totalSteps);

		try
		{
			try
			{
				var totalCache = await dataCacheFactory.GetTotalCacheCount();
				await progressTracker.SetValues("Step", 1, totalSteps);
				logger.LogInformation("Total cache count is {total}", totalCache);
			}
			catch (Exception ex)
			{
				await progressTracker.Failed();
				logger.LogError(ex, "Failed to get cache count");
			}

			cancellationToken.ThrowIfCancellationRequested();

			string connectionString = customerOptions.SQL.ConnectionString;
			int commandTimeout = 120;

			//just to makje sure bulk merge hasn't left residual tables
			try
			{
				int tableCount = 0;

				using (var sqlConnection = new SqlConnection(connectionString))
				{
					await sqlConnection.OpenAsync();

					using (var transaction = sqlConnection.BeginTransaction())
					{
						var query = $"SELECT COUNT(*) FROM sys.tables";

						using (var command = new SqlCommand(query, sqlConnection, transaction)
						{
							CommandTimeout = commandTimeout
						})
						{
							var result = await command.ExecuteScalarAsync();

							tableCount = Convert.ToInt32(result);
						}

						await transaction.CommitAsync();
					}
				}

				await progressTracker.SetValues("Step", 2, totalSteps);
				logger.LogInformation("Total table count is {total}", tableCount);
			}
			catch (Exception ex)
			{
				await progressTracker.Failed();
				logger.LogError(ex, "Failed to get table count");
			}
		}
		catch (OperationCanceledException)
		{
			await progressTracker.Cancelled();
			return;
		}
		catch (Exception ex)
		{
			await progressTracker.Failed();
			logger.LogError(ex, "Failed deleting command insights for rule {ruleId}", request.RuleId);
			return;
		}

		await progressTracker.Completed();

		logger.LogInformation("Diagnostic run completed");
	}
}
