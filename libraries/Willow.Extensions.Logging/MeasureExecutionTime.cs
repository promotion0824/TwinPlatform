using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Extensions.Logging
{
    /// <summary>
    /// Measure execution time of a function and log the result and execution time with any Metrics, including exceptions
    /// </summary>  
    public static class MeasureExecutionTime
    {
        /// <summary>
        /// Pass in a function to execute and a metrics function to execute on completion, and an optional exception log/metrics
        /// </summary>
        public static async Task<TR> ExecuteTimed<TR>(Func<Task<TR>> fnGetResultAsync,
            Action<TR, long> fnTimedResult,
            Action<Exception>? fnOnException = null)
        {
            var timer = Stopwatch.StartNew();
            TR res = default;
            try
            {
                res = await fnGetResultAsync();
            }
            catch(Exception ex)
            {
                timer.Stop();
                fnOnException?.Invoke(ex);
                throw;
            }
            timer.Stop();
            fnTimedResult?.Invoke(res, timer.ElapsedMilliseconds);
            return res;
        }
    }
}
