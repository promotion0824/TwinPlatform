
namespace Authorization.Common.Models;

/// <summary>
/// Location Twin Slim Model
/// </summary>
public record LocationTwinSlim
{
    /// <summary>
    /// Twin Id.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Twin Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Twin Model Id.
    /// </summary>
    public string ModelId { get; set; } = null!;

    /// <summary>
    /// Tree of Children twins
    /// </summary>
    public List<LocationTwinSlim> Children { get; set; } = null!;
}
