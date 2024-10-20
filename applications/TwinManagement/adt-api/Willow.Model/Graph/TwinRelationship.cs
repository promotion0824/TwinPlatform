using Abodit.Graph;
using System.Text.Json.Serialization;

namespace Willow.Model.Graph;

public struct TwinRelationship : IEquatable<TwinRelationship>, IRelation
{
    public string Id => $"{StartId}-{Name}-{EndId}";

    public int StartId { get; set; }

    public int EndId { get; set; }

    public string Name { get; set; }

    public string StartTwinId { get; set; }

    public string EndTwinId { get; set; }

    [JsonIgnore]
    public bool IsReflexive => false;

    public bool Equals(TwinRelationship other) => (StartId, EndId, Name).Equals((other.StartId, other.EndId, other.Name));

    public override bool Equals(object? obj) => obj is TwinRelationship other && Equals(other);

    public override int GetHashCode()
    {
        return (Name + StartId + EndId).GetHashCode();
    }

    public static bool operator ==(TwinRelationship left, TwinRelationship right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TwinRelationship left, TwinRelationship right)
    {
        return !(left == right);
    }
}
