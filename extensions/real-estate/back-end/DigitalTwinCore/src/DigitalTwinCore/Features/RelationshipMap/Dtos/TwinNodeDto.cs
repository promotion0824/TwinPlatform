using System;

namespace DigitalTwinCore.Features.RelationshipMap.Dtos;

public struct TwinNodeDto : IEquatable<TwinNodeDto>
{
    /// <summary>
    /// Id of the twin
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Name of the twin
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Model Id like dtmi:...
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// Count of twin incoming relationships
    /// </summary>
    public long? EdgeInCount { get; set; }

    /// <summary>
    /// Count of twin outgoing relationships
    /// </summary>
    public long? EdgeOutCount { get; set; }

    /// <summary>
    /// Count of twin relationships
    /// </summary>
    public long? EdgeCount { get; set; }

    /// <summary>
    /// Equality
    /// </summary>
    public bool Equals(TwinNodeDto other) => (this.Id, this.Name, this.ModelId)
        .Equals((other.Id, other.Name, other.ModelId));
}