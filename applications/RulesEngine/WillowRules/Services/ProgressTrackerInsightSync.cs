using System;
using Microsoft.Extensions.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace Willow.Rules.Services;

/// <summary>
/// Generic progress tracker for any number of metrics
/// </summary>
public class ProgressTracker : ProgressTrackerBase
{
	public ProgressTracker(
			IRepositoryProgress repositoryProgress,
			string id,
			ProgressType progressType,
			string correlationId,
			string requestedBy,
			DateTimeOffset dateRequested,
			string ruleId,
			ILogger logger)
		: base(id, progressType, correlationId, repositoryProgress, requestedBy, dateRequested, logger, ruleId: ruleId)
	{
	}
}
