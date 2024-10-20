using Willow.Rules.Model;
using System.Collections.Concurrent;

namespace Willow.Rules.Services;

/// <summary>
/// Keeps counts of relationships that are the same
/// </summary>
internal class MetaGraphRelationshipIdentityMapper
{
	/// <summary>
	/// An identity map for all meta graph relationships
	/// </summary>
	/// <remarks>
	/// This is fairly small but may still be better if it wasn't required to be in memory (TODO)
	/// </remarks>
	private ConcurrentDictionary<(int start, int end, string relation, string substance), MetaGraphRelation> allRelations = new();

	/// <summary>
	/// Get a unique relation per link so we can track a count for it
	/// </summary>
	public (MetaGraphRelation relation, bool justAdded) Get(MetaGraphNode start, MetaGraphNode end, string relation, string substance)
	{
		// string key = $"{start.Id} {relation} {end.Id} {substance}";

		if (allRelations.TryGetValue((start.Id, end.Id, relation, substance), out var existing))
		{
			existing.Count++;
			return (existing, false);
		}
		else
		{
			var result = new MetaGraphRelation(start.Id, end.Id, relation, substance);
			allRelations.AddOrUpdate((start.Id, end.Id, relation, substance), result, (k, r) => result);
			return (result, true);
		}
	}
}