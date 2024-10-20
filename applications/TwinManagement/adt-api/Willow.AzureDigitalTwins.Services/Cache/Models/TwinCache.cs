using Azure.DigitalTwins.Core;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Extensions;

namespace Willow.AzureDigitalTwins.Services.Cache.Models;

public interface ITwinCache
{
    public ConcurrentDictionary<string, BasicDigitalTwin> Twins { get; }
    public ConcurrentDictionary<string, BasicRelationship> Relationships { get; }
    public ConcurrentDictionary<string, HashSet<string>> TwinsByModel { get; }
    public ConcurrentDictionary<string, List<string>> TwinRelationships { get; }
    public ConcurrentDictionary<string, List<string>> TwinIncomingRelationships { get; }
    public ConcurrentSet<string> LoadedTwinsRelationships { get; }
    public ConcurrentSet<string> LoadedTwinsIncomingRelationships { get; }

    bool TryCreateOrReplaceTwin(BasicDigitalTwin twin);
    bool TryCreateOrReplaceRelationship(BasicRelationship relationship);
    bool TryRemoveTwin(string id);
    bool TryRemoveRelationship(string id);
}

public class TwinCache : ITwinCache
{
    public ConcurrentDictionary<string, BasicDigitalTwin> Twins { get; init; }
    public ConcurrentDictionary<string, BasicRelationship> Relationships { get; init; }
    public ConcurrentDictionary<string, List<string>> TwinRelationships { get; init; }
    public ConcurrentDictionary<string, List<string>> TwinIncomingRelationships { get; init; }
    public ConcurrentDictionary<string, HashSet<string>> TwinsByModel { get; init; }
    public ConcurrentSet<string> LoadedTwinsRelationships { get; init; }
    public ConcurrentSet<string> LoadedTwinsIncomingRelationships { get; init; }

    public TwinCache(ConcurrentDictionary<string, BasicDigitalTwin> twins,
                    ConcurrentDictionary<string, BasicRelationship> relationships,
                    ConcurrentDictionary<string, List<string>> twinRelationships,
                    ConcurrentDictionary<string, HashSet<string>> twinsByModel,
                    ConcurrentDictionary<string, List<string>> twinIncomingRelationships)
    {
        Twins = twins;
        Relationships = relationships;
        TwinRelationships = twinRelationships;
        TwinsByModel = twinsByModel;
        TwinIncomingRelationships = twinIncomingRelationships;
        LoadedTwinsRelationships = new ConcurrentSet<string>();
        LoadedTwinsIncomingRelationships = new ConcurrentSet<string>();
    }

    // Note that although the ConcurrentDictionary operations are individually thread-safe,
    //   when we have multiple dictionary operations in a single fn, we want all the changes
    //   to be atomic with respect to other functions in this file.
    //   We'd also like changes to appear atomic from the view of the outside - this is done by
    //   locking on Twin Object elsewhere when needed (such as GetTwinRelationshipsAsync)
    // Note that we never lock around any I/O or long-running operations - only multiple list/dictionary operations

    public bool TryCreateOrReplaceTwin(BasicDigitalTwin twin)
    {
        lock (Twins)
        {
            Twins[twin.Id] = twin;

            TwinsByModel.AddOrUpdate(
                twin.Metadata.ModelId,
                id => new HashSet<string> { twin.Id },
                (id, list) =>
                {
                    list.Add(twin.Id);
                    return list;
                });
        }

        return true;
    }

    public bool TryCreateOrReplaceRelationship(BasicRelationship relationship)
    {
        lock (Relationships)
        {
            Relationships[relationship.Id] = relationship;

            TwinRelationships.AddOrUpdate(
                relationship.SourceId,
                _ => new List<string> { relationship.Id },
                (_, list) =>
                {
                    if (!list.Contains(relationship.Id))
                        list.Add(relationship.Id);
                    return list;
                });

            TwinIncomingRelationships.AddOrUpdate(
                relationship.TargetId,
                _ => new List<string> { relationship.Id },
                (_, list) =>
                {
                    if (!list.Contains(relationship.Id))
                        list.Add(relationship.Id);
                    return list;
                });
        }

        return true;
    }

    public bool TryRemoveTwin(string id)
    {
        lock (Twins)
        {
            var removed = Twins.TryRemove(id, out BasicDigitalTwin existing);

            if (removed)
            {
                try
                {
                    // Remove from TwinsByModel Cache
                    if (TwinsByModel[existing.Metadata.ModelId].Remove(existing.Id) && TwinsByModel[existing.Metadata.ModelId].Count == 0)
                    {
                        TwinsByModel.TryRemove(existing.Metadata.ModelId, out _);
                    }

                    // Remove from Loaded Twin Relationships (outgoing and incoming)
                    if (LoadedTwinsRelationships.Contains(id))
                    {
                        LoadedTwinsRelationships.Remove(id);
                    }
                    if (LoadedTwinsIncomingRelationships.Contains(id))
                    {
                        LoadedTwinsIncomingRelationships.Remove(id);
                    }

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Twin cache TwinsByModel not found in TryRemoveTwin", ex);
                }
            }
            return removed;
        }
    }

    public bool TryRemoveRelationship(string id)
    {
        lock (Relationships)
        {
            var removed = Relationships.TryRemove(id, out BasicRelationship existing);

            if (existing != null)
            {
                try
                {
                    TwinRelationships[existing.SourceId].Remove(existing.Id);

                    // If no cached outgoing relationship, remove twin entry
                    if (TwinRelationships[existing.SourceId].Count == 0)
                    {
                        TwinRelationships.TryRemove(existing.SourceId, out _);
                    }

                    TwinIncomingRelationships[existing.TargetId].Remove(existing.Id);

                    // If no cached outgoing relationship, remove twin entry
                    if (TwinIncomingRelationships[existing.TargetId].Count == 0)
                    {
                        TwinIncomingRelationships.TryRemove(existing.TargetId, out _);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Twin cache TwinsRel not found in TryRemoveRelationship", ex);
                }
            }
            return removed;
        }

    }

}
