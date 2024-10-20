using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Willow.Rules.Model
{
	/// <summary>
	/// Extension methods for extending <see cref="ILogger"/> with rules engine specific features
	/// </summary>
	public static class LoggingExtensions
	{
		/// <summary>
		/// Logs actors state to logger for monitoring size
		/// </summary>
		/// <remarks>
		/// Only use this method to get stats on a local dev machine. Too expensine to read this data during production due to concurrent dictionary
		/// </remarks>
		public static void LogBufferState(this ILogger logger, IEnumerable<ActorState> actors, IEnumerable<TimeSeries> buffers)
		{
			if (buffers.Any(v => v.Points.Any()))
			{
				logger.LogInformation($"TimeSeriesReport -> Total Buffers: {buffers.Count()}, Total Values: {buffers.Sum(v => v.Points.Count())}, Max buffer: {buffers.Max(v => v.Count)}");
			}

			if (actors.Any(v => v.TimedValues.Count > 0 && v.OutputValues.Count > 0))
			{
				logger.LogInformation($"ActorReport -> Total Actors: {actors.Count()}, Total Buffers: {actors.Sum(v => v.TimedValues.Count)}, Max buffer: {actors.SelectMany(v => v.TimedValues.Values).Max(v => v.Count)}, Total Values: {actors.SelectMany(v => v.TimedValues).Sum(v => v.Value.Points.Count())}, Total Outputs: {actors.Sum(v => v.OutputValues.Count)} Max output: {actors.Max(v => v.OutputValues.Count)}");
			}
		}
	}
}
