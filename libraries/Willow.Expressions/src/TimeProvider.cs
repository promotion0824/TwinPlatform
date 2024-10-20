using System;
using System.Threading;

namespace Willow.Units
{
    /// <summary>
    /// TimeProvider allows for replacement of DateTime.UtcNow for testing and other situations
    /// </summary>
    /// <remarks>
    /// The <see cref="DefaultTimeProvider"/> calls through to DateTimeOffset.UtcNow.
    /// A <see cref="ManualTimeProvider"/> makes testing easy.
    /// A time provider must NEVER go backwards in time.
    /// </remarks>
    public abstract class TimeProvider
    {
        [ThreadStatic]
        private static TimeProvider? current;     // avoid constructor cycle, use null (on each thread) and >> below

        /// <summary>
        /// Set the current time provider
        /// </summary>
        public static TimeProvider Current
        {
            get
            {
                return current ?? DefaultTimeProvider.Instance;
            }

            set
            {
                //Trace.WriteLine("New time provider : " + value.GetType().Name);
                lastTimeStamp = 0;      // reset so that UtcNowUniqueTicks isn't forced past the new start time
                current = value ?? throw new ArgumentNullException("value");
            }
        }

        /// <summary>
        /// Earliest feasible date (1970,1,1)
        /// </summary>
        public static readonly DateTimeOffset EarliestFeasible = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0));

        /// <summary>
        /// UtcNow
        /// </summary>
        public System.DateTime UtcNow => this.Now.UtcDateTime;

        private static long lastTimeStamp = 0;

        /// <summary>
        /// Get the current Utc tick value but increment it to make it unique for each call to this function
        /// so it can serve as a time stamp
        /// </summary>
        public long UtcNowUniqueTicks
        {
            get
            {
                int debugCheck = 0;
                long orig, newval;
                do
                {
                    if (debugCheck++ > 1000000) throw new Exception("Too many iterations - bug!");
                    orig = lastTimeStamp;
                    newval = this.Now.UtcTicks;
                    if (orig + 1 > newval) newval = orig + 1;
                }
                while (Interlocked.CompareExchange(ref lastTimeStamp, newval, orig) != orig);
                return newval;
            }
        }

        /// <summary>
        /// DateTimeOffset now
        /// </summary>
        public abstract DateTimeOffset Now { get; set; }

        private DateTimeOffset LocalTodayStart => this.Now.Date;

        /// <summary>
        /// Puts the TimeProvider back to the default
        /// </summary>
        public static void ResetToDefault()
        {
            current = DefaultTimeProvider.Instance;
        }

        /// <summary>
        /// Returns a string representation of the current time provider's state
        /// </summary>
        public override string ToString()
        {
            return this.Now.ToString();
        }

        /// <summary>
        /// Start using a new time provider in a using block
        /// </summary>
        /// <param name="newTimeProvider">The new time provider to use</param>
        /// <returns>An IDisposable that will revert to the old time provider when disposed</returns>
        public static IDisposable StartUsing(TimeProvider newTimeProvider)
        {
            return new TimeProviderDisposable(newTimeProvider);
        }

        private class TimeProviderDisposable : IDisposable
        {
            private readonly TimeProvider pushed;

            public TimeProviderDisposable(TimeProvider newTimeProvider)
            {
                this.pushed = TimeProvider.Current;
                TimeProvider.Current = newTimeProvider;
            }

            public void Dispose()
            {
                TimeProvider.Current = pushed;
            }
        }
    }

    /// <summary>
    /// The default time provider uses DateTime.UtcNow
    /// </summary>
    public class DefaultTimeProvider : TimeProvider
    {
        /// <summary>
        /// A singleton instance of the default time provider
        /// </summary>
        public static readonly TimeProvider Instance = new DefaultTimeProvider();

        /// <summary>
        /// Now
        /// </summary>
        public override DateTimeOffset Now
        {
            get { return DateTimeOffset.Now; }
            set { throw new Exception("Cannot set the time for the default time provider"); }
        }
    }

    /// <summary>
    /// The ManualTimeProvider is good for testing - you can manually advance time as you wish to simulate real time
    /// </summary>
    public class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset now;

        /// <summary>
        /// Get the Now time
        /// </summary>
        public override DateTimeOffset Now
        {
            get { return now; }
            set { this.now = value; }
        }

        /// <summary>
        /// Create a new ManualTimeProvider for testing (starting at curent UtcNow time)
        /// </summary>
        public ManualTimeProvider() : this(DateTimeOffset.UtcNow) { }

        /// <summary>
        /// Create a new ManualTimeProvider for testing starting at a specific time
        /// </summary>
        public ManualTimeProvider(DateTimeOffset initialTime)
        {
            this.now = initialTime;
        }

        /// <summary>
        /// Create a new ManualTimeProvider for testing starting at a specific year month day at 12:30:36
        /// </summary>
        public ManualTimeProvider(int year, int month, int day)
            : this(new DateTimeOffset(year, month, day, 12, 30, 36, TimeSpan.FromHours(-8)))
        {
        }

        /// <summary>
        /// Do not use this, it's obsolete
        /// </summary>
        [Obsolete("Use Add method instead")]
        public void AddMinutes(int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
        {
            Add(days, hours, minutes, seconds);
        }

        /// <summary>
        /// Add a given interval to the current time
        /// </summary>
        public void Add(int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
        {
            this.now = this.now.AddDays(days).AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
        }
    }
}
