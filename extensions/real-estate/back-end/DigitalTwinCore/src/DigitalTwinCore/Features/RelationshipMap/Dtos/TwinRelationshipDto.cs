using System;
using System.Text.Json.Serialization;
using Abodit.Graph;

namespace DigitalTwinCore.Features.RelationshipMap.Dtos;

public struct TwinRelationshipDto : IEquatable<TwinRelationshipDto>, IRelation
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Substance { get; set; }

    public string SourceId { get; set; }

    public string TargetId { get; set; }

    [JsonIgnore]
    public bool IsReflexive => false;

    public bool Equals(TwinRelationshipDto other) =>
        (this.Id, this.Name, this.Substance).Equals((other.Id, other.Name, other.Substance));
}