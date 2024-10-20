using System.Threading.Tasks;
using System;
using System.Threading;

namespace Willow.Rules.Services;

/// <summary>
/// Exponential backoff - donated code from Ian
/// </summary>
public class ExponentialBackOff
{
	private int count = 0;
	public double Startmilliseconds { get; set; }
	public double Multiplier { get; set; }
	public int MaxMilliseconds { get; set; }

	public ExponentialBackOff(int startmilliseconds, double multiplier, int maxMilliseconds)
	{
		this.Startmilliseconds = startmilliseconds;
		this.Multiplier = multiplier;
		this.MaxMilliseconds = maxMilliseconds;
	}

	public Task Delay(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested) return Task.FromResult(0);
		double millisecondDelay = this.Startmilliseconds * Math.Pow(this.Multiplier, count);
		if (millisecondDelay <= this.MaxMilliseconds) this.count++;     // advance to next
		var task = Task.Delay((int)millisecondDelay, cancellationToken);
		task.ConfigureAwait(false);
		return task;
	}

	/// <summary>
	/// Get the next delay in milliseconds
	/// </summary>
	public double GetNextDelay()
	{
		double millisecondDelay = this.Startmilliseconds * Math.Pow(this.Multiplier, count);
		if (millisecondDelay <= this.MaxMilliseconds) this.count++;     // advance to next
		return millisecondDelay;
	}

	public int Count()
	{
		return count;
	}

	public void Reset()
	{
		this.count = 0;
	}
}
