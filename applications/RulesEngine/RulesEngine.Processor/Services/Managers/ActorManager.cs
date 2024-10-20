using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace Willow.Rules.Processor;

/// <summary>
/// Manages actor state
/// </summary>
public interface IActorManager
{

	/// <summary>
	/// Flush actors to the database
	/// </summary>
	Task FlushActorsToDatabase(ConcurrentDictionary<string, ActorState> actors, Dictionary<string, List<RuleInstance>> instanceLookup, DateTime nowUtc, ProgressTrackerForRuleExecution progressTracker);

	/// <summary>
	/// Load the actor state
	/// </summary>
	Task<ConcurrentDictionary<string, ActorState>> LoadActorState(Dictionary<string, List<RuleInstance>> ruleInstanceLookup, DateTimeOffset earliest, ProgressTrackerForRuleExecution progressTracker);

	/// <summary>
	/// Applies limits to an actor's timeseries
	/// </summary>
	(int removed, int totalTracked) ApplyLimits(ActorState actor, RuleInstance ruleInstance, DateTime nowUtc);
}


/// <summary>
/// Manages actor state
/// </summary>
public class ActorManager : IActorManager
{
	private bool writingActors = false;
	private readonly IRepositoryActorState repositoryActorState;
	private readonly ILogger<ActorManager> logger;
	/// <summary>
	/// for expression buffers
	/// </summary>
	private readonly int maxDaysToKeep;
	/// <summary>
	/// for output values
	/// </summary>
	private readonly int maxOutputValuesToKeep;
	private readonly ITelemetryCollector telemetryCollector;

	/// <summary>
	/// Creates a new <see cref="ActorManager" />
	/// </summary>
	public ActorManager(
		IRepositoryActorState repositoryActorState,
		ITelemetryCollector telemetryCollector,
		ILogger<ActorManager> logger,
		int maxDaysToKeep = 90,
		//we only need keep 1 output (saves on memory).
		//Occurrences copy happens before actor flush occurs
		int maxOutputValuesToKeep = 1
		)
	{
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
		this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.maxDaysToKeep = maxDaysToKeep;
		this.maxOutputValuesToKeep = maxOutputValuesToKeep;
	}

	/// <summary>
	/// Load the actor state, removing any values after start date
	/// </summary>
	public async Task<ConcurrentDictionary<string, ActorState>> LoadActorState(
		Dictionary<string, List<RuleInstance>> instanceLookup,
		DateTimeOffset startDate,
		ProgressTrackerForRuleExecution progressTracker)
	{
		var actors = new ConcurrentDictionary<string, ActorState>();

		var sw2 = Stopwatch.StartNew();
		try
		{
			logger.LogInformation("Getting list of distinct rule instances");
			var ruleInstances = instanceLookup.SelectMany(x => x.Value).Distinct().ToDictionary(v => v.Id);

			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));

			using (var timed = logger.TimeOperation("Load actors for {count:N0} rule instances", ruleInstances.Count()))
			{
				int actorCount = 0;
				bool firstRun = !actors.Any();

				int actorTotal = await this.repositoryActorState.Count(x => true);
				int totalTimedValues = 0;
				int totalOutputValues = 0;
				int totalPoints = 0;

				var actorsFromDb = this.repositoryActorState.GetAllActors();
				await foreach (var actorFromDb in actorsFromDb)
				{
					try
					{
						//if an actor was not able to deserialize properly, ignore it
						if (actorFromDb is null)
						{
							continue;
						}

						if (!ruleInstances.TryGetValue(actorFromDb.Id, out var ruleInstance))
						{
							throttledLogger.LogWarning("Actor {actor} not in rule instances list, should delete", actorFromDb.Id);
							//cannot delete here, active data reader or single rule execution
							//await this.repositoryActorState.DeleteOne(actorFromDb);
							continue;
						}

						if (actors.TryAdd(actorFromDb.Id, actorFromDb))
						{
							actorCount++;
							throttledLogger.LogInformation("Loading actor state {actorCount:N0}/{actorTotal:N0} {speed:0.0}/s", actorCount, actorTotal, actorCount / (sw2.ElapsedMilliseconds + 0.1) * 1000);
						}
						else
						{
							if (firstRun)
							{
								logger.LogWarning($"Actor Id {actorFromDb.Id} already loaded - internal error");
							}
						}

						actorFromDb.RefreshValuesFromRuleInstance(ruleInstance);
						actorFromDb.RemoveOldCommandOutputs(ruleInstance);
						//This method can be removed once Insight Occurrence data has moved to it's own table
						actorFromDb.SetDefaultOutputValues();

						foreach (var timeseries in actorFromDb.TimedValues.Values)
						{
							timeseries.RemovePointsAfter(startDate);
						}

						totalPoints += actorFromDb.TimedValues.Sum(v => v.Value.Points.Count());
						totalTimedValues += actorFromDb.TimedValues.Count;
						totalOutputValues += actorFromDb.OutputValues.Count;

						await progressTracker.ReportLoadingActors(actorCount, actorTotal);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Error loading actor {actor}", actorFromDb.Id);
					}
				}
				await progressTracker.ReportLoadingActors(actorTotal, actorTotal);

				logger.LogInformation("Actors loaded. Total actors {totalActors}. Total TimeSeries {totalTimeSeries}. Total Points {totalPoints}. Total Outputs {totalOutputs}",
					actors.Count,
					totalTimedValues,
					totalPoints,
					totalOutputValues);
			}

			if (ruleInstances.Count != actors.Count)
			{
				logger.LogInformation("Loaded {countActors:N0} actors", actors.Count);
			}

		}
		catch (Exception ex)
		{
			//skip loading if there are any deserialization issues. It will overwrite at the end to latest structure
			logger.LogError(ex, $"An error occurred loading actors from the db: {ex.Message}. Actors will be re-written once execution is finished");
		}

		return actors;
	}



	/// <summary>
	/// Flush all the actors to the database
	/// </summary>
	/// <remarks>
	/// This can no longer be fire and forget since if rule execution
	/// keeps going with the same actor and modifies a list inside the
	/// actor state we get a collection modified exception in serialization.
	/// </remarks>
	public async Task FlushActorsToDatabase(
		ConcurrentDictionary<string, ActorState> actors,
		Dictionary<string, List<RuleInstance>> instanceLookup,
		DateTime nowUtc,
		ProgressTrackerForRuleExecution progressTracker
		)
	{
		// Investigate if flush actors is running too often / concurrently
		if (writingActors) logger.LogError("Attempting to flush actors when already flushing actors");
		writingActors = true;

		int actorCount = 0;
		int removed = 0;
		Stopwatch sw3 = Stopwatch.StartNew();
		try
		{
			int totalActors = actors.Count;
			int totalTimedValues = 0;
			int totalOutputValues = 0;
			int totalOutputVariables = 0;
			int totalPoints = 0;
			int totalTracked = 0;
			int totalAlias = 0;

			using (logger.TimeOperation("Flush {count:N0} actors", totalActors))
			{
				var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
				var validationLogger = logger.Throttle(TimeSpan.FromSeconds(5));
				var maxPoints = 0;
				var maxPointsId = "";
				var ruleInstances = instanceLookup.SelectMany(x => x.Value).Distinct().ToDictionary(v => v.Id);

				foreach ((var key, var actor) in actors)
				{
					try
					{
						if(!ruleInstances.TryGetValue(key, out var ruleInstance))
						{
							logger.LogWarning("Actor {id} not in rule instance list", key);
							continue;
						}

						//dont flush alias entries (less to write to DB). They will get re-added on next run
						totalAlias += actor.RemoveAliasTimeSeries();

						int actorTotalPoints = actor.TimedValues.Sum(v => v.Value.Points.Count());

						totalPoints += actorTotalPoints;

						if (actorTotalPoints > maxPoints)
						{
							maxPoints = actorTotalPoints;
							maxPointsId = actor.Id;
						}

						totalTimedValues += actor.TimedValues.Count;
						totalOutputValues += actor.OutputValues.Count;
						totalOutputVariables += actor.OutputValues.Points.Sum(v => v.Variables.Length);

						(int removedValues, int tracked) = ApplyLimits(actor, ruleInstance, nowUtc);

						removed += removedValues;

						totalTracked += tracked;

						foreach (var tsKey in actor.TimedValues.Keys.ToArray())
						{
							var timeseries = actor.TimedValues[tsKey];

							if (timeseries.Count > 0)
							{
								if (!timeseries.CheckTimeSeriesIsInOrder())
								{
									validationLogger.LogWarning("Actor state was not in order on save, reordering {actorId}", actor.Id);
									timeseries.Sort();
								}
							}
							else
							{
								// Remove old variables that are not used anymore.
								actor.TimedValues.Remove(tsKey);
							}
						}

						// Prune the output values to keep them from growing too large
						removed += actor.OutputValues.ApplyLimits(maxOutputValuesToKeep, DateTimeOffset.MinValue);
						//Commnad outputs are still in memory. It doesn't behave like insights yet which has its own table for outputs (occurrences)
						removed += actor.OutputValues.ApplyCommandLimits(50, DateTimeOffset.MinValue);

						if (!actor.OutputValues.IsInOrder())
						{
							validationLogger.LogWarning("Actor state output values was not in order on save, reordering {actorId}", actor.Id);
							actor.OutputValues.Points.Sort();
						}

						if (actor.HasOverlappingOutputValues())
						{
							validationLogger.LogWarning("Actor state for {actorId} has overlapping Output Values.", actor.Id);
						}

						// VERY SLOW, ACTOR STATE IS TOO LARGE, 1000 is failing for some sites
						await this.repositoryActorState.QueueWrite(actor, queueSize: 500, updateCache: false);
						actorCount++;

						throttledLogger.LogInformation($"Upsert actor state {actorCount} {actorCount / (sw3.ElapsedMilliseconds + 0.1) * 1000:0.0}/s {removed} removed");
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Problem writing actor state");
					}

					await progressTracker.ReportFlushingActors(actorCount, totalActors);
				}

				telemetryCollector.TrackActors(totalActors, totalPoints, totalOutputValues, totalOutputVariables);

				logger.LogInformation("Actors flushed. Total {totalActors}. TimeSeries {totalTimeSeries} ({totalTracked} Tracked, {totalAlias} Aliases). Points {totalPoints}. Outputs {totalOutputs}. Output variables {totalOutputVariables}. Values Removed {removedCount}",
					totalActors,
					totalTimedValues,
					totalTracked,
					totalAlias,
					totalPoints,
					totalOutputValues,
					totalOutputVariables,
					removed);

				logger.LogInformation("Actor with most points {total} id {id}", maxPoints, maxPointsId);

				await this.repositoryActorState.FlushQueue(updateCache: false);

				//House cleaning
				await this.repositoryActorState.DeleteOrphanActors();

				await progressTracker.ReportFlushingActors(totalActors, totalActors);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Flush actors to storage failed");
		}

		writingActors = false;
	}

	/// <summary>
	/// Applies limits to an actor's timeseries
	/// </summary>
	public (int removed, int totalTracked) ApplyLimits(ActorState actor, RuleInstance ruleInstance, DateTime nowUtc)
	{
		return actor.ApplyLimits(ruleInstance, nowUtc, TimeSpan.FromDays(maxDaysToKeep));
	}
}
