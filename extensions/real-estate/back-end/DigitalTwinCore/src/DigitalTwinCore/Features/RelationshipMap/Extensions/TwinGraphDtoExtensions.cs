using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Abodit.Mutable;
using DigitalTwinCore.Features.RelationshipMap.Dtos;
using DigitalTwinCore.Features.RelationshipMap.Models;
using DigitalTwinCore.Features.TwinsSearch.Models;

namespace DigitalTwinCore.Features.RelationshipMap.Extensions
{
    public static class TwinGraphDtoExtensions
    {
        public static TwinGraphDto MapToGraphDto(this Graph<BasicDigitalTwin, WillowRelation> graph)
        {
            var nodes = new List<TwinNodeDto>();

            var primaryGroups = graph.Nodes.GroupBy(n => SuggestGroupKey(n, graph));
            foreach (var primaryGroup in primaryGroups)
            {
                var count = primaryGroup.Count();
                if (count < 10)
                {
                    nodes.AddRange(primaryGroup.Select(twin => DtoFactory(twin)));
                }
                else
                {
                    var divisor = (int)Math.Round(Math.Sqrt(count));
                    var alphaGroups = primaryGroup
                        .OrderBy(x => (x.Name?.ToUpperInvariant() ?? ""))
                        .Select((x, i) => (twin: x, key: i * divisor / count));

                    foreach (var alphaGroup in alphaGroups.GroupBy(g => g.key))
                    {
                        nodes.AddRange(alphaGroup.Select(ag => ag.twin).Select(twin => DtoFactory(twin)));
                    }
                }
            }

            var relationships = graph.Edges.Select(e => CreateRelationshipDto(e.Predicate.Id, e.Start.Id, e.End.Id,
                e.Predicate.Name, e.Predicate.Substance));

            return new TwinGraphDto
            {
                Nodes = nodes.ToArray(),
                Edges = relationships.ToArray()
            };
        }

        private static TwinNodeDto DtoFactory(BasicDigitalTwin relatedTwin) =>
            new()
            {
                Id = relatedTwin.Id,
                Name = string.IsNullOrWhiteSpace(relatedTwin.Name) ? relatedTwin.Id : relatedTwin.Name,
                ModelId = relatedTwin.Metadata.ModelId
            };

        private static (string gpName, string extra) SuggestGroupKey(BasicDigitalTwin twin, Graph<BasicDigitalTwin, WillowRelation> graph)
        {
            // Group must match all the ins and outs, e.g. one entity
            // with multiple capabilities
            string InsOuts(BasicDigitalTwin n) =>
                string.Join("_",
                    graph.Follow(n).Select(e => e.End)
                    .Concat(graph.Back(n).Select(e => e.Start))
                    .Select(x => x.Name)).GetHashCode().ToString();

            if (string.IsNullOrEmpty(twin.Name)) return ("", "");

            if (twin.Name.Contains("Zone Air", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("ZoneAir"))
            {
                return ("zone air related", "");
            }

            if (twin.Name.Contains("Supply", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Supply"))
            {
                return ("supply related", "");
            }

            if (twin.Name.Contains("Outside", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Outside"))
            {
                return ("outside related", "");
            }

            if (twin.Name.Contains("Cooling", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Cooling"))
            {
                return ("cooling related", "");
            }

            if (twin.Name.Contains("Return", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Return"))
            {
                return ("return related", "");
            }

            if (twin.Name.Contains("Exhaust", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Exhaust"))
            {
                return ("exhaust related", "");
            }

            if (twin.Name.Contains("Discharge", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Discharge"))
            {
                // TODO: Ask Rick if supply and discharge can be combined
                return ("discharge related", "");
            }

            if (twin.Name.Contains("Temperature", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Temperature"))
            {
                return ("temperature related", "");
            }

            if (twin.Name.Contains("Pressure", StringComparison.OrdinalIgnoreCase) || twin.Metadata.ModelId.Contains("Pressure"))
            {
                return ("pressure related", "");
            }

            if (twin.Unit?.Equals("Terminal Units") ?? false)  // HBC and CBC counts
            {
                return ("counters", "");
            }

            // TODO: Use model inheritance not ID names

            if (twin.Metadata.ModelId.Contains("Actuator"))
            {
                return ("actuators", "");
            }

            if (twin.Metadata.ModelId.Contains("Setpoint"))
            {
                return ("setpoints", "");
            }

            // otherwise potentially group by the model id
            return (twin.Metadata.ModelId, InsOuts(twin));
        }

        private static TwinRelationshipDto CreateRelationshipDto(
            string id, string sourceId, string targetId, string name, string substance)
        {
            return new TwinRelationshipDto
            {
                Id = id, 
                SourceId = sourceId,
                TargetId = targetId,
                Name = name,
                Substance = substance
            };
        }
    }

    public static class TwinNodeSimpleDtoExtensions
    {
        public static TwinNodeDto MapToTwinNodeSimpleDto(this RelationshipMapRelationship relationshipMapRelationship, int relCount, int inRelCount, int outRelCount)
        {
            return new TwinNodeDto
            {
                Id = relationshipMapRelationship.Id,
                Name = relationshipMapRelationship.Name,
                ModelId = relationshipMapRelationship.ModelId,
                EdgeInCount = inRelCount,
                EdgeOutCount = outRelCount,
                EdgeCount = relCount
            };
        }
        public static TwinNodeDto MapToTwinNodeSimpleDto(this RelationshipMapRelationship relationshipMapRelationship)
        {
            return new TwinNodeDto
            {
                Id = relationshipMapRelationship.OpponentId,
                Name = relationshipMapRelationship.OpponentName,
                ModelId = relationshipMapRelationship.OpponentModelId,
                EdgeInCount = relationshipMapRelationship.In,
                EdgeOutCount = relationshipMapRelationship.Out,
                EdgeCount = relationshipMapRelationship.OpponentRelationshipCount
            };
        }
    }

    public static class TwinRelationshipSimpleDtoExtensions
    {
        public static TwinRelationshipDto MapToTwinRelationshipSimpleDto(this RelationshipMapRelationship relationshipMapRelationship)
        {
            return new TwinRelationshipDto
            {
                SourceId = relationshipMapRelationship.SourceId,
                TargetId = relationshipMapRelationship.TargetId,
                Id = relationshipMapRelationship.RelId,
                Name = relationshipMapRelationship.RelName,
                Substance = relationshipMapRelationship.Substance
            };
        }
    }
}
