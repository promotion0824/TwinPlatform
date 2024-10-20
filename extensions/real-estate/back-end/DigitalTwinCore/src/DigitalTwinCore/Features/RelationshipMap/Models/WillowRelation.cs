using System;
using System.Collections.Concurrent;
using System.Linq;
using Abodit.Graph;

namespace DigitalTwinCore.Features.RelationshipMap.Models;

public sealed class WillowRelation: IEquatable<WillowRelation>, IRelation
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Substance { get; set; }

    public bool IsReflexive => false;

    private static readonly ConcurrentDictionary<(string id, string name, string substance), WillowRelation> IdentityMap = new();

    private WillowRelation() { }

    public static WillowRelation Get(string id, string name, string substance)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        var result = IdentityMap.GetOrAdd((id, name, substance), (props) =>
            new WillowRelation { Id = id, Name = props.name, Substance = props.substance });

        return result;
    }

    public bool Equals(WillowRelation other) => (Name, Substance) == (other?.Name, other?.Substance);

    internal static WillowRelation[] GetAll(string name)
    {
        return IdentityMap.Values.Where(v => v.Name.Equals(name)).ToArray();
    }
}