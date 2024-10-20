using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Willow.Rules.Logging;

/// <summary>
/// Records operation timings
/// </summary>
/// <remarks>
/// Inspired by https://github.com/nblumhardt/serilog-timings
/// </remarks>
public class Operation : IDisposable
{
	const string OutcomeCompleted = "completed", OutcomeAbandoned = "abandoned";
	static readonly long StopwatchToTimeSpanTicks = Stopwatch.Frequency / TimeSpan.TicksPerSecond;

	ILogger logger;
	readonly string messageTemplate;
	readonly object[] args;
	readonly long start;
	long? stop;

	TimedLogCompletion completionBehaviour;
	readonly LogLevel completionLevel;
	readonly LogLevel abandonmentLevel;
	private readonly TimeSpan? warningThreshold;
	Exception? exception;

	internal Operation(ILogger logger, string messageTemplate, object[] args,
		TimedLogCompletion completionBehaviour, LogLevel completionLevel, LogLevel abandonmentLevel,
		TimeSpan? warningThreshold = null)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.messageTemplate = messageTemplate ?? throw new ArgumentNullException(nameof(messageTemplate));
		this.args = args ?? throw new ArgumentNullException(nameof(args));
		this.completionBehaviour = completionBehaviour;
		this.completionLevel = completionLevel;
		this.abandonmentLevel = abandonmentLevel;
		this.warningThreshold = warningThreshold;
		start = GetTimestamp();
	}

	static long GetTimestamp()
	{
		return Stopwatch.GetTimestamp() / StopwatchToTimeSpanTicks;
	}

	/// <summary>
	/// Returns the elapsed time of the operation. This will update during the operation, and be frozen once the
	/// operation is completed or canceled.
	/// </summary>
	private TimeSpan Elapsed
	{
		get
		{
			var stop = this.stop ?? GetTimestamp();
			var elapsedTicks = stop - start;

			if (elapsedTicks < 0)
			{
				// When measuring small time periods the StopWatch.Elapsed*  properties can return negative values.
				// This is due to bugs in the basic input/output system (BIOS) or the hardware abstraction layer
				// (HAL) on machines with variable-speed CPUs (e.g. Intel SpeedStep).
				return TimeSpan.Zero;
			}

			return TimeSpan.FromTicks(elapsedTicks);
		}
	}

	/// <summary>
	/// Complete the timed operation. This will write the event and elapsed time to the log.
	/// </summary>
	public void Complete()
	{
		if (completionBehaviour == TimedLogCompletion.Silent)
			return;

		Write(logger, completionLevel, OutcomeCompleted);
	}

	/// <summary>
	/// Abandon the timed operation. This will write the event and elapsed time to the log.
	/// </summary>
	public void Abandon()
	{
		Write(logger, abandonmentLevel, OutcomeAbandoned);
	}

	/// <summary>
	/// Cancel the timed operation. After calling, no event will be recorded either through
	/// completion or disposal.
	/// </summary>
	public void Cancel()
	{
		completionBehaviour = TimedLogCompletion.Silent;
	}

	/// <summary>
	/// If not already completed or canceled, a log will be written with timing information.
	/// </summary>
	public void Dispose()
	{
		switch (completionBehaviour)
		{
			case TimedLogCompletion.Silent:
				break;

			case TimedLogCompletion.Abandon:
				Write(logger, abandonmentLevel, OutcomeAbandoned);
				break;

			default:
			case TimedLogCompletion.Complete:
				Write(logger, completionLevel, OutcomeCompleted);
				break;
		}
	}


	void Write(ILogger target, LogLevel level, string outcome)
	{
		stop ??= GetTimestamp();
		completionBehaviour = TimedLogCompletion.Silent;

		var elapsed = Elapsed.TotalMilliseconds;
		level = Elapsed > warningThreshold ? LogLevel.Warning : level;

		// Operation that only logs when over threshold
		if (level == LogLevel.None) return;

		if (elapsed < 3 * 1000)  // less than 3 seconds, log in ms
			target.Log(level, exception, $"{messageTemplate} {{outcome}} in {{elapsed:0.0}}ms", args.Concat(new object[] { outcome, elapsed }).ToArray());
		else if (elapsed < 2 * 60 * 1000)  // less than 2 minutes, log in seconds
			target.Log(level, exception, $"{messageTemplate} {{outcome}} in {{elapsed:0.0}}s", args.Concat(new object[] { outcome, elapsed / 1000.0 }).ToArray());
		else // more than 2 minutes, log in minutes
			target.Log(level, exception, $"{messageTemplate} {{outcome}} in {{elapsed:0.0}}min", args.Concat(new object[] { outcome, elapsed / 60.0 / 1000.0 }).ToArray());
	}

	/// <summary>
	/// Sets the exception that broke this timed log.
	/// </summary>
	public Operation Failed(Exception exception)
	{
		this.exception = exception;
		return this;
	}
}
