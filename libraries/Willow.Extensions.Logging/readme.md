Willow.Extensions.Logging
====

Use these extension methods on an ILogger to create a new logger that throttles output or to time a block of code.

For example

````
using (var timedLogger = logger.TimeOperation("Flush metadata to database"))
{

}
````

There is also an overload that takes a TimeSpan and anything above that limit it logged as an error.

The throttled logger works like this:

````
var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
````

Exponential Backoff
----
Use the Exponential backoff class to either delay a retry to a failed external call, or to provide an increasing TimeSpan that can be applied to DatetimeOffset.Now to calculate
when the next retry should be attempted.
