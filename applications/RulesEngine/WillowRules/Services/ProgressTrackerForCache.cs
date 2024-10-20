using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;

/// <summary>
/// Tracks progress on long running operations
/// </summary>
public interface IProgressTrackerForCache
{
	Task ReportStarting();
	Task ReportComplete();
	Task ReportForwardBuildCount(int count, int total);
	Task ReportBackwardBuildCount(int count, int total);
	Task ReportGraphBuildCount(string stage, int count, int total);
	Task ReportModelCount(int count, int total);
	Task ReportTwinCount(string uri, int count, int total);
	Task ReportRelationshipCount(string uri, int count, int total);
	Task ReportTwinLocationUpdateCount(int count, int total);
	Task ReportMappedCount(string uri, int count, int total);

	/// <summary>
	/// Gets the counts from the earlier stages
	/// </summary>
	(long twinCount, long relationshipCount) GetCounts();
}


public class ProgressTrackerForCache : ProgressTrackerBase, IProgressTrackerForCache
{
	// Define the stages for progress, the relative cost of 1 unit of work for each and the
	// estimated total work to do on each stage

	private readonly ProgressTrackerStage ModelsStage = new("Models", 1.0, 1000);
	private readonly ProgressTrackerStage TwinsStage = new("Twins", 6.0, 500000);
	private readonly ProgressTrackerStage TwinLocationsStage = new("Locations", 6.0, 500000);
	private readonly ProgressTrackerStage RelationshipsStage = new("Relationships", 10.0, 300000);
	private readonly ProgressTrackerStage MappedStage = new("Mapped", 0.1, 30000);
	private readonly ProgressTrackerStage GraphsStage = new("Graphs", 2.5, 300000);

	public ProgressTrackerForCache(
		IRepositoryProgress repositoryProgress,
		ILogger logger,
		string correlationId,
		string reqestedBy,
		DateTimeOffset dateRequested)
			: base(Progress.CacheId, ProgressType.Cache, correlationId, repositoryProgress, reqestedBy, dateRequested, logger)
	{
		this.stages["Models"] = this.ModelsStage;
		this.stages["Twins"] = this.TwinsStage;
		this.stages["Mapped"] = this.MappedStage;
		this.stages["Relationships"] = this.RelationshipsStage;
		this.stages["Graphs"] = this.GraphsStage;
		this.stages["Locations"] = this.TwinLocationsStage;
	}

	public (long twinCount, long relationshipCount) GetCounts() => (this.TwinsStage.Total, this.RelationshipsStage.Total);

	ConcurrentDictionary<string, (int totalTwins, int totalRelationships, int twins, int relationships)> counts = new();

	public async Task ReportStarting()
	{
		await ReportStats(true);
	}

	public async Task ReportModelCount(int count, int total)
	{
		this.ModelsStage.Track(count, total);
		await ReportStats(false);
	}

	public async Task ReportTwinCount(string uri, int count, int total)
	{
		this.TwinsStage.Track(uri, count, total);
		await ReportStats(false);
	}

	public async Task ReportTwinLocationUpdateCount(int count, int total)
	{
		this.TwinLocationsStage.Track(count, total);
		await ReportStats(false);
	}

	public async Task ReportRelationshipCount(string uri, int count, int total)
	{
		this.RelationshipsStage.Track(uri, count, total);
		await ReportStats(false);
	}

	public async Task ReportMappedCount(string uri, int count, int total)
	{
		this.MappedStage.Track(uri, count, total);
		await ReportStats(false);
	}

	public async Task ReportForwardBuildCount(int count, int total)
	{
		this.GraphsStage.Track("Forward", count, total);
		await ReportStats(false);
	}

	public async Task ReportBackwardBuildCount(int count, int total)
	{
		this.GraphsStage.Track("Backward", count, total);
		await ReportStats(false);
	}

	public async Task ReportGraphBuildCount(string stage, int count, int total)
	{
		this.GraphsStage.Track(stage, count, total);
		await ReportStats(false);
	}

	public async Task ReportComplete()
	{
		completed = true;
		await ReportStats(true);
	}
}
