using Abodit.Graph;
using System.Collections.Concurrent;

namespace Willow.Model.Adt;

public class TwinRelation : IEquatable<TwinRelation>, IRelation
{
    /// <summary>
    /// Name
    /// </summary>
    public string? Name { get; set; }

    public bool IsReflexive => false;

    // Could be called from two aspnetcore threads at the same time
    private static ConcurrentDictionary<string, TwinRelation> identityMap = new();

    // Force use of factory method
    private TwinRelation() { }

    /// <summary>
    /// Gets an identity-mapped <see cref="WillowRelation"/>
    /// </summary>
    public static TwinRelation Get(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        var result = identityMap.GetOrAdd(name, (props) =>
        {
            return new TwinRelation { Name = props };
        });

        return result;
    }

    public bool Equals(TwinRelation? other) => Name == other?.Name;

    public override bool Equals(object? obj) => obj is TwinRelation other && Equals(other);

    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Get all matching relations by name (i.e. any substance)
    /// </summary>
    internal static TwinRelation[] GetAll(string name)
    {
        return identityMap.Values.Where(v => v.Name != null && v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}
