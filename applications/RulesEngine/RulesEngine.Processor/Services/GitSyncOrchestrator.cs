using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RulesEngine.Processor.Services;
using Willow.Rules.Model;

namespace Willow.Rules.Processor;

/// <summary>
/// The git sync orchestrator provides a channel between the 
/// <see cref="ProcessorQueueServiceBackgroundService"/> and the <see cref="GitSyncExecutionService"/>
/// </summary>
public interface IGitSyncOrchestrator
{
    /// <summary>
	/// Channel reader that the <see cref="GitSyncExecutionService"/> listens to
	/// </summary>
	ChannelReader<GitSyncRequest> Listen { get; }

	/// <summary>
	/// Channel writer that the <see cref="ProcessorQueueServiceBackgroundService"/> sends to
	/// </summary>
	ChannelWriter<GitSyncRequest> Send { get; }

    /// <summary>
	/// Notified that a git sync has been completed
	/// </summary>
	Task Completed(CancellationToken cancellationToken);
}

/// <summary>
/// The git sync orchestrator provides a channel between the 
/// <see cref="ProcessorQueueServiceBackgroundService"/> and the <see cref="GitSyncExecutionService"/>
/// </summary>
public class GitSyncOrchestrator : IGitSyncOrchestrator
{
    private readonly Channel<GitSyncRequest> channel;
    private readonly ILogger<GitSyncOrchestrator> logger;

    /// <summary>
    /// Channel writer that the <see cref="ProcessorQueueServiceBackgroundService"/> sends to
    /// </summary>
    public ChannelWriter<GitSyncRequest> Send => channel.Writer;

    /// <summary>
    /// Channel reader that the <see cref="GitSyncExecutionService"/> listens to
    /// </summary>
    public ChannelReader<GitSyncRequest> Listen => channel.Reader;

    /// <summary>
    /// Creates a new <see cref="GitSyncOrchestrator"/>
    /// </summary>
    public GitSyncOrchestrator(ILogger<GitSyncOrchestrator> logger)
    {
        this.channel = Channel.CreateBounded<GitSyncRequest>(1);
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
	/// Signals that a git sync has been completed
	/// </summary>
	public Task Completed(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
