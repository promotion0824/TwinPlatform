using System.Threading.Tasks;
using System;
using System.Threading;

namespace Willow.Extensions.Logging;

/// <summary>
/// Exponential backoff
/// </summary>
/// <remarks>
/// Create an instance of this class. Each time you succeed call .Reset() on it.
/// Each time you fail call either GetNextDelay() to find out how long to wait
/// before trying again or Delay() to actually wait that long.
/// </remarks>
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

    /// <summary>
    /// Delay for an exponentially increasing period of time
    /// </summary>
    /// <remarks>
    /// Use this inside a Polly retry block for all calls to external systems
    /// especially ones that rate limit us returning 429 errors when we call too often.
    /// </remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Reset the exponential backoff
    /// </summary>
    /// <remarks>
    /// Call this every time you are successful
    /// </remarks>
    public void Reset()
    {
        this.count = 0;
    }
}
