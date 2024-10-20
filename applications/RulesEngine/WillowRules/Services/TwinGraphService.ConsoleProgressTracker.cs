using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Willow.Rules.Services;

public partial class TwinGraphService
{
	private class ConsoleProgressTracker : IProgressTrackerForCache
	{
		private readonly ILogger logger;

		public ConsoleProgressTracker(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public Task ReportStarting()
		{
			return Task.CompletedTask;
		}
		public Task ReportComplete()
		{
			logger.LogInformation("Complete");
			return Task.CompletedTask;
		}

		public Task ReportGraphBuildCount(string stage, int count, int count2)
		{
			return Task.CompletedTask;
		}

		public Task ReportModelCount(int count, int total)
		{
			return Task.CompletedTask;
		}

		public Task ReportRelationshipCount(string uri, int count, int total)
		{
			if (count % 1000 == 0) logger.LogInformation($"Relationships {count}");
			return Task.CompletedTask;
		}

		public Task ReportTwinCount(string uri, int count, int total)
		{
			if (count % 1000 == 0) logger.LogInformation($"Twins {count}");
			return Task.CompletedTask;
		}

		public Task ReportForwardBuildCount(int count, int total)
		{
			return Task.CompletedTask;
		}

		public Task ReportBackwardBuildCount(int count, int total)
		{
			return Task.CompletedTask;
		}

		public (long twinCount, long relationshipCount) GetCounts() => (0, 0);

		public Task ReportMappedCount(string uri, int count, int total)
		{
			return Task.CompletedTask;
		}

		public Task ReportTwinLocationUpdateCount(int count, int total)
		{
			return Task.CompletedTask;
		}
	}
}
