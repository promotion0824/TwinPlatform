using System;
using System.Collections.Generic;
namespace Willow.Rules.Model;

/// <summary>
/// Extension methods for working with the linked list of time series values in <see cref="TimeSeries" />
/// </summary>
public static class LinkedListExtensions
{
	/// <summary>
	/// Checks whether the provided point is of similar attributes as this point indicating it is a duplicate
	/// </summary>
	public static bool IsTheSame(this TimedValue value, TimedValue newValue)
	{
		if (value.Timestamp == newValue.Timestamp &&
			value.BoolValue == newValue.BoolValue &&
			value.ValueDouble == newValue.ValueDouble)
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Re-sorts the list by the points' time stamp
	/// </summary>
	/// <remarks>
	/// The sort can be used for safety puprposes at the start of a run to ensure 
	/// that values are ordered correctly. This is an inplace sort using an
	/// insertion sort which performs very well when the list is already in order.
	/// </remarks>
	public static void Sort(this LinkedList<TimedValue> list)
	{
		var node = list.First;
		while (node != null)
		{
			var next = node.Next;
			while (next != null)
			{
				if (node.Value.Timestamp > next.Value.Timestamp)
				{
					// Move next before node
					list.Remove(next);
					list.AddBefore(node, next);
					// And make it the current node
					node = next;
				}
				// follow along the chain
				next = next.Next;
			}
			// The first element is now the earliest, can advance to the next one
			node = node.Next;
		}
	}

	/// <summary>
	/// Populate a time series points list with values using a trajectory compressor
	/// </summary>
	/// <param name="points"></param>
	public static void Populate(this LinkedList<TimedValue> points,
		TrajectoryCompressor trajectoryCompressor,
		IEnumerable<TimedValue> values)
	{
		var trajectoryCompressorState = new TrajectoryCompressorState();

		foreach (var v in values)
		{
			trajectoryCompressor.Add(trajectoryCompressorState, v.Timestamp, v.NumericValue,
			(t, x) => { points.AddLast(new TimedValue(t, x)); },
			(a, b, c) => { points.RemoveLast(); points.AddLast(new TimedValue(b, c)); });
		}
	}
}
