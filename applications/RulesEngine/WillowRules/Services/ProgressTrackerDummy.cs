using System.Threading.Tasks;

namespace Willow.Rules.Services;

/// <summary>
/// Dummy progress tracker
/// </summary>
public class ProgressTrackerDummy : IProgressTrackerForCache
{
	public Task ReportStarting() { return Task.CompletedTask; }
	public Task ReportComplete() { return Task.CompletedTask; }

	public Task ReportGraphBuildCount(string stage, int count, int count2) { return Task.CompletedTask; }

	public Task ReportModelCount(int count, int total) { return Task.CompletedTask; }

	public Task ReportTwinCount(string uri, int count, int total) { return Task.CompletedTask; }

	public Task ReportRelationshipCount(string uri, int count, int total) { return Task.CompletedTask; }

	public Task ReportForwardBuildCount(int count, int total) { return Task.CompletedTask; }

	public Task ReportBackwardBuildCount(int count, int total) { return Task.CompletedTask; }

	public (long twinCount, long relationshipCount) GetCounts() => (0, 0);

	public Task ReportMappedCount(string uri, int count, int total) { return Task.CompletedTask; }

	public Task ReportTwinLocationUpdateCount(int count, int total)
	{
		return Task.CompletedTask;
	}
}
