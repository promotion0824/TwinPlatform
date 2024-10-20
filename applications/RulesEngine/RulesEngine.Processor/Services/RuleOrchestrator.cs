using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Processor;

/// <summary>
/// The rule orchestrator provides a channel between the realtime executor and requests from the web interface
/// </summary>
public interface IRuleOrchestrator
{
	/// <summary>
	/// Channel that the rules engine real-time executor listens on
	/// </summary>
	ChannelReader<RuleExecutionRequest> Listen { get; }

	/// <summary>
	/// Channel writer that the service bus listener sends to
	/// </summary>
	ChannelWriter<RuleExecutionRequest> Send { get; }

	/// <summary>
	/// Notified that a rules rebuild has completed
	/// </summary>
	Task RebuildRulesCompleted(CancellationToken cancellationToken, bool runRealtime);

	/// <summary>
	/// Notified that a cache update has completed
	/// </summary>
	Task UpdateCacheCompleted(CancellationToken cancellationToken);

	/// <summary>
	/// Notified that a search index update has completed
	/// </summary>
	Task RebuildSearchIndexCompleted(CancellationToken cancellationToken);

	/// <summary>
	/// Notified that a web-requested rule execution has completed
	/// </summary>
	Task ProcessDateRangeCompleted(CancellationToken cancellationToken);

	/// <summary>
	/// Notified that the deletion of a rule has been completed
	/// </summary>
	Task RuleDeleteCompleted(CancellationToken cancellationToken);

	/// <summary>
	/// Notified that a git sync has completed
	/// </summary>
	Task GitSyncCompleted(CancellationToken cancellationToken);

	/// <summary>
	/// Notified that calculated points process has completed
	/// </summary>
	Task ProcessCalculatedPointsCompleted(CancellationToken cancellationToken);

	/// <summary>
	/// Create a cancellation token source to be used during the task's work.
	/// </summary>
	CancellationToken GetCancellationToken(string progressId);

	/// <summary>
	/// Requests to cancel task.
	/// </summary>
	void Cancel(string progressId);
}

/// <summary>
/// The rule orchestrator provides a channel between the realtime executor and requests from the web interface
/// </summary>
/// <remarks>
/// This is just a wrapper around a Channel which provides the message bus within this application
/// but could be switched later to use ServiceBus when we split the Processor into two containers
/// </remarks>
public class RuleOrchestrator : IRuleOrchestrator
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly Channel<RuleExecutionRequest> channel;
	private readonly ILogger<RuleOrchestrator> logger;
	private readonly ConcurrentDictionary<string, CancellationTokenSource> cancellationTokens;

	/// <summary>
	/// Channel writer that the service bus listener sends to
	/// </summary>
	public ChannelWriter<RuleExecutionRequest> Send => channel.Writer;

	/// <summary>
	/// Channel reader that the real time executor listens to
	/// </summary>
	public ChannelReader<RuleExecutionRequest> Listen => channel.Reader;

	/// <summary>
	/// Creates a new <see cref="RuleOrchestrator" />
	/// </summary>
	public RuleOrchestrator(WillowEnvironment willowEnvironment, ILogger<RuleOrchestrator> logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new System.ArgumentNullException(nameof(willowEnvironment));
		this.channel = Channel.CreateBounded<RuleExecutionRequest>(1);
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.cancellationTokens = new();
	}

	/// <summary>
	/// Signals that rules have been rebuilt
	/// </summary>
	public Task RebuildRulesCompleted(CancellationToken cancellationToken, bool runRealtime)
	{
		logger.LogInformation("Orchestrator: Rebuild rules completed");

		if (runRealtime)
		{
			//send a new realtime request so that data can be updated
			var request = RuleExecutionRequest.CreateRealtimeExecutionRequest(willowEnvironment.Id, RulesOptions.ProcessorCloudRoleName);
			//TryWrite won't wait so that we don't hold up the queue
			channel.Writer.TryWrite(request);
		}

		return Task.CompletedTask;
	}

	/// <summary>
	/// Signals that the cache has been updated, does not affect continuous execution
	/// until rules have been rebuilt
	/// </summary>
	public Task UpdateCacheCompleted(CancellationToken cancellationToken)
	{
		logger.LogInformation("Orchestrator: Update cache complete");
		return Task.CompletedTask;
	}

	/// <summary>
	/// Signals that the search index has been rebuilt, does not affect continuous execution
	/// </summary>
	public Task RebuildSearchIndexCompleted(CancellationToken cancellationToken)
	{
		logger.LogInformation("Orchestrator: Rebuild search complete");
		return Task.CompletedTask;
	}

	/// <summary>
	/// Signals that the processing of a date range has been completed
	/// </summary>
	public Task ProcessDateRangeCompleted(CancellationToken cancellationToken)
	{
		logger.LogInformation("Orchestrator: Process date range completed");
		return Task.CompletedTask;
	}

	/// <summary>
	/// Signals that the deletion of a rule has been completed
	/// </summary>
	public Task RuleDeleteCompleted(CancellationToken cancellationToken)
	{
		logger.LogInformation("Orchestrator: Process rule delete completed");
		return Task.CompletedTask;
	}

	/// <summary>
	/// Signals that a git sync has been completed
	/// </summary>
	public Task GitSyncCompleted(CancellationToken cancellationToken)
	{
		logger.LogInformation("Orchestrator: Git sync forwarded to execution");
		return Task.CompletedTask;
	}

	/// <summary>
	/// Signals that calculated points processing has completed
	/// </summary>
	public Task ProcessCalculatedPointsCompleted(CancellationToken cancellationToken)
	{
		logger.LogInformation("Orchestrator: Process calculated points completed");
		return Task.CompletedTask;
	}

	/// <summary>
	/// Create a cancellation token source to be used during the task's work.
	/// </summary>
	public CancellationToken GetCancellationToken(string progressId)
	{
		if (!cancellationTokens.TryGetValue(progressId, out var source))
		{
			source = new CancellationTokenSource();

			cancellationTokens.TryAdd(progressId, source);
		}
		else if (source.IsCancellationRequested)//in case it was cancelled and never removed
		{
			cancellationTokens[progressId] = new CancellationTokenSource();
		}

		return source.Token;
	}

	/// <summary>
	/// Requests to cancel task.
	/// </summary>
	public void Cancel(string progressId)
	{
		if (cancellationTokens.TryRemove(progressId, out var source))
		{
			logger.LogInformation("Cancelling token");

			if (source.Token.IsCancellationRequested)
			{
				logger.LogInformation("Cancellation token is already in a cancelled state");
			}
			else
			{
				try
				{
					source.Cancel();
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to cancel token for {id}", progressId);
				}
			}
		}
		else
		{
			logger.LogWarning("No cancellation token found for progress {id}", progressId);
		}
	}
}
