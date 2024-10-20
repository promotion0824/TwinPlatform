namespace Willow.Model.Graph;

public struct TwinNode : IEquatable<TwinNode>
{
    /// <summary>
    /// A local id just for this sub graph
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Id of the twin
    /// </summary>
    public string TwinId { get; set; }

    /// <summary>
    /// Name of the twin
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the twin
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Model Id like dtmi:...
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// Phenomenon
    /// </summary>
    public string Phenomenon { get; set; }

    /// <summary>
    /// Unit of measure
    /// </summary>
    public string Unit { get; set; }

    /// <summary>
    /// Position (supply, return, ...)
    /// </summary>
    public string Position { get; set; }

    /// <summary>
    /// When the graph is too dense nodes with the same group key can be collapsed to a node
    /// </summary>
    public string GroupKey { get; set; }

    /// <summary>
    /// When a group is too large we have a second group key
    /// </summary>
    public string GroupKey2 { get; set; }

    /// <summary>
    /// Used client-side during expand/collapse node groupings
    /// </summary>
    public string CollapseKey { get; set; }

    /// <summary>
    /// Equality
    /// </summary>
    public bool Equals(TwinNode other) => (Id, Name, ModelId, Phenomenon, Unit, Position)
        .Equals((other.Id, other.Name, other.ModelId, other.Phenomenon, other.Unit, other.Position));

    public override bool Equals(object? obj) => obj is TwinNode other && Equals(other);

    public override int GetHashCode()
    {
        return (Id + Name + ModelId + Phenomenon + Unit + Position).GetHashCode();
    }

    public static bool operator ==(TwinNode left, TwinNode right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TwinNode left, TwinNode right)
    {
        return !(left == right);
    }
}
