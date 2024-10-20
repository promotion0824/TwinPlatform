using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abodit.Mutable;
using DigitalTwinCore.Features.RelationshipMap.Models;
using Microsoft.Extensions.Logging;

namespace DigitalTwinCore.Features.RelationshipMap.Services;

public interface ITwinSystemService
{
    Task<Graph<BasicDigitalTwin, WillowRelation>> GetTwinSystemGraph(string[] twinIds, Guid siteId);
}

public class TwinSystemService : ITwinSystemService
{
    private readonly ITwinService _twinService;
    private readonly ILogger<TwinSystemService> _logger;

    public TwinSystemService(ITwinService twinService, ILogger<TwinSystemService> logger)
    {
        _twinService = twinService;
        _logger = logger;
    }

    public async Task<Graph<BasicDigitalTwin, WillowRelation>> GetTwinSystemGraph(string[] twinIds, Guid siteId)
    {
        var results = new Graph<BasicDigitalTwin, WillowRelation>();
        var memoized = new ConcurrentDictionary<string, BasicDigitalTwin>();
        BasicDigitalTwin IdentityMap(BasicDigitalTwin twin) => memoized.GetOrAdd(twin.Id, twin);

        if (twinIds.Contains("all"))
        {
            var topLevelTwins = await _twinService.GetTopLevelEntities(siteId);
            twinIds = twinIds.Where(x => x != "all").Concat(topLevelTwins.Select(x => x.Id)).ToArray();
        }

        var seen = new List<string>();
        var queue = new Queue<(string twinId, int distance, string following)>();
        foreach (var twinId in twinIds)
        {
            queue.Enqueue((twinId, 0, string.Empty));
        }

        async Task FindRelationships(BasicDigitalTwin twin,
            int distance,
            Direction relationshipDirection,
            string[] includeRelationships,
            string getRelationship,
            string enqueueRelationship,
            Direction statementDirection)
        {
            var relatedForwardTwins = await _twinService.GetRelatedTwins(siteId, twin.Id, relationshipDirection);
            foreach (var edge in relatedForwardTwins.Where(x => includeRelationships.Contains(x.RelationshipType)))
            {
                var pred = WillowRelation.Get(edge.Id, getRelationship ?? edge.RelationshipType, edge.Substance);
                if (statementDirection == Direction.Forward)
                {
                    results.AddStatement(twin, pred, IdentityMap(edge.Destination));
                }
                else
                {
                    results.AddStatement(IdentityMap(edge.Destination), pred, twin);

                }

                queue.Enqueue((edge.Destination.Id, distance + 1, enqueueRelationship));
            }
        }

        var limit = 10000; // traversal limit

        async Task ProcessQueue(string id, int distance, string following)
        {
            if (seen.Contains(id))
            {
                return;
            }

            seen.Add(id);

            var twin = IdentityMap(await _twinService.GetTwin(siteId, id));

            switch (following)
            {
                case "feeds":
                    {
                        await FindRelationships(twin,
                            distance,
                            Direction.Forward,
                            new[] { "isFedBy" },
                            "feeds",
                            "feeds",
                            Direction.Backward);
                        break;
                    }
                case "isFedBy":
                    {
                        await FindRelationships(twin,
                            distance,
                            Direction.Backward,
                            new[] { "isFedBy" },
                            "feeds",
                            "isFedBy",
                            Direction.Forward);
                        break;
                    }
                case "servedBy":
                    {
                        await FindRelationships(twin,
                            distance,
                            Direction.Backward,
                            new[] { "servedBy" },
                            "serves",
                            "servedBy",
                            Direction.Forward);
                        break;
                    }
                case "physicalGreater":
                    {
                        await FindRelationships(twin,
                            distance,
                            Direction.Forward,
                            new[] { "locatedIn", "isPartOf", "includedIn" },
                            null,
                            "physicalGreater",
                            Direction.Forward);
                        break;
                    }
                default:
                    var relatedForwardTwins = (await _twinService.GetRelatedTwins(siteId, twin.Id, Direction.Forward)).ToList();
                    foreach (var (start, predicate, end) in
                             ProcessForwardTwins(relatedForwardTwins, distance, twin, following, queue, IdentityMap))
                    {
                        results.AddStatement(start, predicate, end);
                    }

                    var relatedBackTwins = (await _twinService.GetRelatedTwins(siteId, twin.Id, Direction.Backward)).ToList();
                    foreach (var (start, predicate, end) in
                             ProcessBackwardTwins(relatedBackTwins, distance, twin, queue, IdentityMap))
                    {
                        results.AddStatement(start, predicate, end);
                    }

                    break;
            }
        }
        
        while (queue.Count > 0)
        {
            if (limit-- < 0)
            {
                _logger.LogWarning("Graph traversal hit traversals limit");
                break;
            }
            
            var (twinId, distance, following) = queue.Dequeue();
            await ProcessQueue(twinId, distance, following);
        }

        return results;
    }

    private static IEnumerable<(BasicDigitalTwin start, WillowRelation predicate, BasicDigitalTwin end)>
        ProcessBackwardTwins(List<Edge> edges,
            int distance,
            BasicDigitalTwin twin,
            Queue<(string twinId, int distance, string following)> queue,
            Func<BasicDigitalTwin, BasicDigitalTwin> identityMap)
    {
        foreach (var edge in edges)
        {
            switch (edge.RelationshipType)
            {
                case "isFedBy":
                    {
                        // flip the edge for the output
                        var pred = WillowRelation.Get(edge.Id, "feeds", edge.Substance);
                        yield return (identityMap(edge.Destination), pred, twin);

                        switch (edge.Destination.Metadata.ModelId)
                        {
                            // Bounce back down through an HVACZone
                            case "dtmi:com:willowinc:HVACZone;1":
                            // These also act like a zone
                            case "dtmi:com:willowinc:OccupancyZone;1":
                            // These also act like a zone
                            case "dtmi:com:willowinc:InferredOccupancySensor;1":
                                queue.Enqueue((edge.Destination.Id, distance + 1, ""));
                                break;
                            default:
                                queue.Enqueue((edge.Destination.Id, distance + 1, "isFedBy"));
                                break;
                        }

                        break;
                    }
                case "servedBy":
                    {
                        // flip the edge for the output
                        var pred = WillowRelation.Get(edge.Id, "serves", edge.Substance);
                        yield return (identityMap(edge.Destination), pred, twin);
                        queue.Enqueue((edge.Destination.Id, distance + 1, "servedBy"));
                        break;
                    }

                case "isCapabilityOf":
                    {
                        // If we go down to a capability, also look up from that capability
                        // because of the double isCapabilityOf issue around HasInferredOccupancy and People Count
                        queue.Enqueue((edge.Destination.Id, distance + 1, "isCapabilityOf"));
                        // Mostly this will get straight back to the same node, but sometimes ...

                        var pred = WillowRelation.Get(edge.Id, edge.RelationshipType, edge.Substance);
                        yield return (identityMap(edge.Destination), pred, twin);
                        break;
                    }
                case "isPartOf":
                    {
                        if (twin.Metadata.ModelId == "dtmi:com:willowinc:OccupancyZone;1")
                        {
                            // check it's an inferred occupancy
                            queue.Enqueue((edge.Destination.Id, distance + 1, ""));
                        }
                        var pred = WillowRelation.Get(edge.Id, edge.RelationshipType, edge.Substance);
                        yield return (identityMap(edge.Destination), pred, twin);
                        break;
                    }
                default:
                    {
                        // Something else, add it to graph but don't follow it
                        var pred = WillowRelation.Get(edge.Id, edge.RelationshipType, edge.Substance);
                        yield return (identityMap(edge.Destination), pred, twin);
                        break;
                    }
            }
        }
    }


    private static IEnumerable<(BasicDigitalTwin start, WillowRelation predicate, BasicDigitalTwin end)>
        ProcessForwardTwins(List<Edge> edges,
            int position,
            BasicDigitalTwin basicDigitalTwin,
            string following,
            Queue<(string twinId, int distance, string following)> queue,
            Func<BasicDigitalTwin, BasicDigitalTwin> identityMap)
    {
        foreach (var edge in edges)
        {
            switch (edge.RelationshipType)
            {
                case "isPartOf":
                case "locatedIn":
                    {
                        switch (edge.Destination.Metadata.ModelId)
                        {
                            // If the parent is an HVACZone we are interested in what feeds that zone
                            // so push in on the queue with empty string to examine all of its links
                            case "dtmi:com:willowinc:HVACZone;1":
                            // These also act like a zone
                            case "dtmi:com:willowinc:OccupancyZone;1":
                            // These also act like a zone
                            case "dtmi:com:willowinc:InferredOccupancySensor;1":
                                queue.Enqueue((edge.Destination.Id, position + 1, ""));
                                break;
                            default:
                                queue.Enqueue((edge.Destination.Id, position + 1, "physicalGreater"));
                                break;
                        }

                        var pred = WillowRelation.Get(edge.Id, edge.RelationshipType, edge.Substance);
                        yield return (basicDigitalTwin, pred, identityMap(edge.Destination));
                        break;
                    }
                case "isFedBy":
                    {
                        queue.Enqueue((edge.Destination.Id, position + 1, "feeds"));
                        // flip the edge for the output
                        var pred = WillowRelation.Get(edge.Id, "feeds", edge.Substance);
                        yield return (identityMap(edge.Destination), pred, basicDigitalTwin);
                        break;
                    }
                case "isCapabilityOf":
                    {
                        queue.Enqueue((edge.Destination.Id, position + 1, "isCapabilityOf"));

                        var pred = WillowRelation.Get(edge.Id, edge.RelationshipType, edge.Substance);
                        yield return (basicDigitalTwin, pred, identityMap(edge.Destination));
                        break;
                    }
                case "hostedBy" when following == "isCapabilityOf":
                    // If we came in on isCapabilityOf, don't leave on hostedBy
                    break;
            }
        }
    }
}