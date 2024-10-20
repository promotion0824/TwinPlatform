using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A summary of current progress
/// </summary>
public class ProgressSummaryDto
{
	/// <summary>
	/// Creates a <see cref="ProgressSummaryDto" /> from a <see cref="Progress" /> list
	/// </summary>
	public ProgressSummaryDto(IEnumerable<Progress> all)
	{
		var progress = all.FirstOrDefault(v => v.Id == Progress.CacheId) ?? new Progress();
		CacheProgress = new ProgressDto(progress);

		progress = all.FirstOrDefault(v => v.Id == Progress.RuleExpansionId) ?? new Progress();
		RuleExpansionProgress = new ProgressDto(progress);

		progress = all.FirstOrDefault(v => v.Id == Progress.RuleExecutionId) ?? new Progress();
		RuleExecutionProgress = new ProgressDto(progress);

		progress = all.FirstOrDefault(v => v.Id == Progress.RealtimeExecutionId) ?? new Progress();
		RealtimeExecutionProgress = new ProgressDto(progress);
	}

	/// <summary>
	/// The current Cache Progress
	/// </summary>
	public ProgressDto CacheProgress { get; init; }

	/// <summary>
	/// The current Rule Expansion Progress
	/// </summary>
	public ProgressDto RuleExpansionProgress { get; init; }

	/// <summary>
	/// The current Batch Execution Progress
	/// </summary>
	public ProgressDto RuleExecutionProgress { get; init; }

	/// <summary>
	/// The current Realtime Execution Progress
	/// </summary>
	public ProgressDto RealtimeExecutionProgress { get; init; }
}
