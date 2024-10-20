namespace Willow.Expressions;

/// <summary>
/// Represents a unit and it's value
/// </summary>
public struct UnitValue
{
    public UnitValue(Unit unit, double value)
    {
        Unit = unit;
        Value = value;
    }

    /// <summary>
    /// The unit
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// The value
    /// </summary>
    public double Value { get; }
}
