using Willow.Expressions;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Dto for <see cref="Unit" />
/// </summary>
public class UnitDto
{
    /// <summary>
    /// Creates a <see cref="UnitDto" /> from an <see cref="Unit" />
    /// </summary>
    public UnitDto(Unit unit)
    {
        Name = unit.Name;
        Aliases = unit.Aliases;
    }

    /// <summary>
    /// Name of the unit
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Aliases for the unit
    /// </summary>
    public string[] Aliases { get; init; }
}
