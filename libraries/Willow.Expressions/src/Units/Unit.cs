using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Willow.Expressions;

/// <summary>
/// Unit of measure
/// </summary>
public partial class Unit : IEquatable<Unit>
{
    /// <summary>
    /// Canonical name of unit. Names are case sensitive.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Alias names for the unit
    /// </summary>
    public string[] Aliases { get; init; }

    /// <summary>
    /// Unit outputType
    /// </summary>
    public UnitOutputType OutputType
    {
        get
        {
            if (this.Equals(boolean))
            {
                return UnitOutputType.Binary;
            }

            return UnitOutputType.Analog;
        }
    }

    private Func<double, double> BaseConversion { get; init; }

    private Func<string, double, bool> OutOfRange { get; init; }

    private Unit(string name, params string[] aliases)
    {
        this.Name = name;
        this.Aliases = aliases.ToArray() ?? new string[0];
        this.BaseConversion = (v) => v;
        this.OutOfRange = (m, v) => false;
    }

    /// <summary>
    /// Checks whether this unit's name or any alias is equal to the provided name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool HasNameOrAlias(string name)
    {
        return string.Equals(name, this.Name, StringComparison.OrdinalIgnoreCase)
            || Aliases.Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether a unit's value is not within its upper or lower limits
    /// </summary>
    public static (bool ok, bool result) IsOutOfRange(string unit, string modelId, double pointValue)
    {
        //Cleanup unit to standardize - different variation possible that might include spaces, e.g. kJ / kg
        if (!string.IsNullOrWhiteSpace(unit) && TryGetUnit(unit.Replace(" ", ""), out var unitValue))
        {
            return (true, unitValue!.OutOfRange(modelId, pointValue));
        }

        return (false, false);
    }

    /// <summary>
    /// Checks whether a unit has a range limit configured
    /// </summary>
    public static bool HasRange(string unit, string modelId)
    {
        return IsOutOfRange(unit, modelId, 0).ok;
    }

    /// <summary>
    /// Convert a value from one unit to another if possible
    /// </summary>
    public static bool TryConvert(Unit fromUnit, Unit toUnit, out Func<TokenExpression, TokenExpression>? result)
    {
        if (fromUnit.Name == toUnit.Name)
        {
            result = x => x;
            return true;
        }
        if (fromUnit.Equals(degC) && toUnit.Equals(degF))
        {
            result = x =>
                new TokenExpressionAdd(TokenExpressionConstant.Create(32.0),
                    new TokenExpressionMultiply(x, TokenExpressionConstant.Create(9.0 / 5.0)))
                { Unit = "degF" };
            return true;
        }

        if (fromUnit.Equals(degF) && toUnit.Equals(degC))
        {
            result = x =>
                new TokenExpressionMultiply(
                    new TokenExpressionAdd(TokenExpressionConstant.Create(-32.0), x),
                    TokenExpressionConstant.Create(5.0 / 9.0))
                { Unit = "degC" };
            return true;
        }

        if (fromUnit.Equals(percentage) && toUnit.Equals(percentage100))
        {
            result = x =>
                new TokenExpressionMultiply(x, TokenExpressionConstant.Create(100.0));
            return true;
        }

        if (fromUnit.Equals(percentage100) && toUnit.Equals(percentage))
        {
            result = x =>
                new TokenExpressionMultiply(x, TokenExpressionConstant.Create(0.01));
            return true;
        }

        // 2.1188799727597 cfm == 1 litre/second
        if (fromUnit.Equals(cfm) && toUnit.Equals(lps))
        {
            result = x =>
                new TokenExpressionDivide(x, TokenExpressionConstant.Create(2.1188799727597));
            return true;
        }

        if (fromUnit.Equals(lps) && toUnit.Equals(cfm))
        {
            result = x =>
                new TokenExpressionMultiply(x, TokenExpressionConstant.Create(2.1188799727597));
            return true;
        }

        result = default;
        return false;
    }

    private static ConcurrentDictionary<string, Unit> allUnits = new();

    /// <summary>
    /// Gets the canonical, identity-mapped unit from a string
    /// </summary>
    public static Unit Get(string name)
    {
        if (string.IsNullOrEmpty(name)) return Unit.scalar;
        if (TryGetUnit(name, out Unit? u)) return u!;

        var unit = predefinedUnits.FirstOrDefault(v => v.HasNameOrAlias(name)) ?? new Unit(name);

        allUnits.TryAdd(name, unit); // identity map

        return unit;
    }

    /// <summary>
    /// Do these two units have the same dimension, e.g. temperature, length, ...
    /// </summary>
    internal static bool HasSameDimension(Unit unit1, Unit unit2)
    {
        if (unit1.Equals(unit2)) return true;
        if (unit1.Equals(degC) && unit2.Equals(degF)) return true;
        if (unit1.Equals(degF) && unit2.Equals(degC)) return true;
        if (unit1.Equals(percentage) && unit2.Equals(percentage100)) return true;
        if (unit1.Equals(percentage100) && unit2.Equals(percentage)) return true;
        return false;
    }

    public static double ConvertToBaseValue(Unit unit, double value)
    {
        return unit.BaseConversion(value);
    }

    /// <summary>
    /// Equal name (since identity mapped)
    /// </summary>
    public bool Equals(Unit? other)
    {
        return other is Unit u && u.Name == this.Name;
    }

    /// <summary>
    /// Attempts to get a predefined unit of this name
    /// </summary>
    public static bool TryGetUnit(string unit, out Unit? unitOfMeasure)
    {
        unitOfMeasure = null;

        if (string.IsNullOrEmpty(unit))
        {
            return false;
        }

        return allUnits.TryGetValue(unit, out unitOfMeasure);
    }
}
