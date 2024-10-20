using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using WillowRules.Services;
using Willow.Rules.Logging;

namespace RulesEngine.Processor.Services
{
	/// <summary>
	/// Processor to manage ADT Twin management using the Willow ADT Api
	/// </summary>
	public interface ICalculatedPointsProcessor
	{
		/// <summary>
		/// Process calculated points to create, update or delete capability twins in ADT
		/// </summary>
		Task ProcessCalculatedPoints(RuleExecutionRequest request, CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// Processor to manage ADT Twin management using the Willow ADT Api
	/// </summary>
	public class CalculatedPointsProcessor : ICalculatedPointsProcessor
	{
		private readonly IRepositoryCalculatedPoint repositoryCalculatedPoints;
		private readonly IRepositoryProgress repositoryProgress;
		private readonly ICalculatedPointsService calculatedPointsService;
		private readonly ITelemetryCollector telemetryCollector;
		private readonly ILogger<CalculatedPointsProcessor> logger;

		/// <summary>
		/// Creates a new <see cref="CalculatedPointsProcessor"/>
		/// </summary>
		public CalculatedPointsProcessor(
			IRepositoryCalculatedPoint repositoryCalculatedPoints,
			ICalculatedPointsService calculatedPointsService,
			IRepositoryProgress repositoryProgress,
			ITelemetryCollector telemetryCollector,
			ILogger<CalculatedPointsProcessor> logger)
		{
			this.repositoryCalculatedPoints = repositoryCalculatedPoints ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoints));
			this.calculatedPointsService = calculatedPointsService ?? throw new ArgumentNullException(nameof(calculatedPointsService));
			this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
			this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Process calculated points to create, update or delete capability twins in ADT
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		public async Task ProcessCalculatedPoints(RuleExecutionRequest request, CancellationToken cancellationToken = default)
		{
			var tracker = new ProgressTracker(repositoryProgress, Progress.ProcessCalculatedPointsId, ProgressType.ProcessCalculatedPoints, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

			var ruleId = request.RuleId;

			try
			{
				await tracker.Start();

				var calculatedPointsLookup = await repositoryCalculatedPoints.Get(cp => cp.Source == CalculatedPointSource.RulesEngine);

				var calculatedPointsToProcess = !string.IsNullOrWhiteSpace(ruleId) ?
					calculatedPointsLookup.Where(cp => cp.RuleId == ruleId) : calculatedPointsLookup;

				var totalCount = calculatedPointsToProcess.Count();

				if (calculatedPointsToProcess.Any())
				{
					using (var timed = logger.TimeOperation("Processing {count} calculated points...", calculatedPointsToProcess.Count()))
					{
						(int updated, int deleted) = await calculatedPointsService.ProcessCalculatedPoints(calculatedPointsToProcess, tracker, cancellationToken);

						await tracker.SetValues("Upserted", updated, updated);
						await tracker.SetValues("Deleted", deleted, deleted);

						telemetryCollector.TrackCalculatedPoints(updated, deleted);
					}
				}

				await tracker.Completed();
			}
			catch (OperationCanceledException e)
			{
				logger.LogError(e, "Cancelled ADT sync process for rule {rule}", ruleId);

				if (tracker != null)
				{
					await tracker.Cancelled();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "ADT sync process failed for {ruleId}", ruleId);
				await tracker.Failed();
			}
		}
	}
}
