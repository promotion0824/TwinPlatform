using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Willow.Expressions.Visitor;

/// <summary>
/// A visitor to convert a TokenExpression to a serialized form
/// </summary>
public class TokenExpressionSerializer : ITokenExpressionVisitor<string>
{
    /// <summary>
    /// Visit
    /// </summary>
    public virtual string Visit(TokenExpression source)
    {
        return source.Accept(this);
    }

    /// <summary>
    /// If the precedence of the child is lower than the parent, wrap it in parentheses
    /// </summary>
    /// <example>
    /// e.g. 1 + 2 ... * 3 should be serialized as (1 + 2) * 3
    /// </example>
    public string WrapIfNeeded(TokenExpression parent, TokenExpression child)
    {
        if (child is TokenExpressionFunctionCall) { return child.Accept(this); } // e.g. FN() is already wrapped
        if (child is TokenExpressionTemporal) { return child.Accept(this); } // e.g. DELTA() is already wrapped
        if (child.Priority > parent.Priority) { return child.Accept(this); }// e.g. 3 + (4 * 5)
        if (child.Priority == parent.Priority && parent.Commutative) { return child.Accept(this); }// e.g. 3 + (4 + 5)

        // Otherwise wrap it to be sure
        return $"({child.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionMultiply input)
    {
        return $"{string.Join(" * ", input.Children.Select(c => WrapIfNeeded(input, c)))}";
    }

    private const string VariableNameSpecialCharacters = "' ;:+-*/()";

    /// <summary>
    /// Names with special characters need to be in square parentheses
    /// </summary>
    private static bool IsSimpleVariableName(string name)
    {
        if (name.Length < 1) return false;
        if (name.Intersect(VariableNameSpecialCharacters).Any()) return false;
        if (char.IsLetter(name[0])) return true;
        if (name[0] == '_') return true;
        if (name[0] == '$') return true;
        return false;
    }

    /// <summary>
    /// Names with special characters need to be in square parentheses
    /// </summary>
    private static string SquareVariableName(string name)
    {
        return IsSimpleVariableName(name) ? name : $"[{name}]";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionPropertyAccess input)
    {
        return $"{WrapIfNeeded(input, input.Child)}.{SquareVariableName(input.PropertyName)}";
    }

    /// <summary>
    /// Serialize a <see cref="TokenExpressionVariableAccess"/> adding [ ] to any invalid stand-alone variable names
    /// </summary>
    public virtual string DoVisit(TokenExpressionVariableAccess input)
    {
        return SquareVariableName(input.VariableName);
    }

    public virtual string DoVisit(TokenExpressionFunctionCall input)
    {
        // No need for parentheses around each child
        return $"{input.FunctionName}({string.Join(",", input.Children.Select(c => c.Accept(this)))})";
    }

    public virtual string DoVisit(TokenExpressionNotEquals input)
    {
        return $"{WrapIfNeeded(input, input.Left)} != {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionOr input)
    {
        return $"{string.Join(" | ", input.Children.Select(c => WrapIfNeeded(input, c)))}";
    }

    public virtual string DoVisit(TokenExpressionTernary input)
    {
        return $"IF({input.Conditional.Accept(this)}, {input.Truth.Accept(this)}, {input.Falsehood.Accept(this)})";
    }

    public virtual string DoVisit(TokenExpressionTuple input)
    {
        return $"Tuple({string.Join(",", input.Children.Select(c => c.Accept(this)))})";
    }

    public virtual string DoVisit(TokenExpressionPower input)
    {
        return $"{WrapIfNeeded(input, input.Left)}^{WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionSubtract input)
    {
        return $"{WrapIfNeeded(input, input.Left)} - {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionIdentity input)
    {
        return $"{input.Child.Accept(this)}";
    }

    public virtual string DoVisit(TokenExpressionFailed input)
    {
        return $"FAILED({string.Join(",", input.Children.Select(c => c.Accept(this)))})";
    }

    public virtual string DoVisit(TokenExpressionIntersection input)
    {
        return $"INTERSECT({string.Join(",", input.Children.Select(c => c.Accept(this)))})";
    }

    public virtual string DoVisit(TokenExpressionSetUnion input)
    {
        return $"{string.Join("∪", input.Children.Select(c => WrapIfNeeded(input, c)))}";
    }

    public virtual string DoVisit(TokenDouble input)
    {
        return input.Unit == TokenExpression.NOUNIT ?
            Convert.ToDecimal(input.Value).ToString(CultureInfo.InvariantCulture) :
            Convert.ToDecimal(input.Value).ToString(CultureInfo.InvariantCulture) + SerializeUnit(input.Unit);
    }

    public virtual string DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        return $"LocalTime({input.Child.Accept(this)})";
    }

    public virtual string DoVisit(TokenExpressionArray input)
    {
        return $"{{{string.Join(",", input.Children.Select(c => c.Accept(this)))}}}";
    }

    public virtual string DoVisit(TokenExpressionConstant input)
    {
        return $"{input.Value.ToString(CultureInfo.InvariantCulture)}";
    }

    public virtual string DoVisit(TokenExpressionConstantBool input)
    {
        return $"{input.Value.ToString(CultureInfo.InvariantCulture)}";
    }

    public virtual string DoVisit(TokenExpressionConstantColor input)
    {
        // Should this be in quotes?
        return $"RGB({input.R},{input.G},{input.B})";
    }

    public virtual string DoVisit(TokenExpressionAdd input)
    {
        return $"{string.Join(" + ", input.Children.Select(c => WrapIfNeeded(input, c)))}";
    }

    public virtual string DoVisit(TokenExpressionMatches input)
    {
        return $"{WrapIfNeeded(input, input.Left)} ∈ {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionAnd input)
    {
        return $"{string.Join(" & ", input.Children.Select(c => WrapIfNeeded(input, c)))}";
    }

    public virtual string DoVisit(TokenExpressionConstantNull input)
    {
        return $"null";
    }

    public virtual string DoVisit(TokenExpressionConstantDateTime input)
    {
        return $"DateTime(\"{input.ValueDateTime.ToString("s", CultureInfo.InvariantCulture)}\")";
    }

    public virtual string DoVisit(TokenExpressionConstantString input)
    {
        return $"\"{input.Value.ToString(CultureInfo.InvariantCulture)}\"";
    }

    public virtual string DoVisit(TokenExpressionDivide input)
    {
        return $"{WrapIfNeeded(input, input.Left)} / {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionIs input)
    {
        return $"{WrapIfNeeded(input, input.Left)} is {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionEquals input)
    {
        return $"{WrapIfNeeded(input, input.Left)} == {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionGreater input)
    {
        return $"{WrapIfNeeded(input, input.Left)} > {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionGreaterOrEqual input)
    {
        return $"{WrapIfNeeded(input, input.Left)} >= {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionNot input)
    {
        return $"!{WrapIfNeeded(input, input.Child)}";
    }

    public virtual string DoVisit(TokenExpressionUnaryMinus input)
    {
        return $"-{WrapIfNeeded(input, input.Child)}";
    }

    public virtual string DoVisit(TokenExpressionLess input)
    {
        return $"{WrapIfNeeded(input, input.Left)} < {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionLessOrEqual input)
    {
        return $"{WrapIfNeeded(input, input.Left)} <= {WrapIfNeeded(input, input.Right)}";
    }

    public virtual string DoVisit(TokenExpressionSum input)
    {
        return $"SUM({SerializeLinqExpression(input)})";
    }

    public string DoVisit(TokenExpressionCount input)
    {
        return $"COUNT({SerializeLinqExpression(input)})";
    }

    public string DoVisit(TokenExpressionAverage input)
    {
        return $"AVERAGE({SerializeLinqExpression(input)})";
    }

    public string DoVisit(TokenExpressionAny input)
    {
        return $"ANY({SerializeLinqExpression(input)})";
    }

    public string DoVisit(TokenExpressionAll input)
    {
        return $"ALL({SerializeLinqExpression(input)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionFirst input)
    {
        return $"FIRST({SerializeLinqExpression(input)})";
    }

    public string DoVisit(TokenExpressionEach input)
    {
        string enumerable = input.EnumerableArgument.Accept(this);
        string variable = input.VariableName.Accept(this);
        string body = input.Body.Accept(this);
        return $"EACH({enumerable}, {variable}, {body})";
    }

    public string DoVisit(TokenExpressionMin input)
    {
        return $"MIN({SerializeLinqExpression(input)})";
    }

    public string DoVisit(TokenExpressionMax input)
    {
        return $"MAX({SerializeLinqExpression(input)})";
    }

    public string DoVisit(TokenExpressionParameter input)
    {
        return $"parameter(\"{input.Type.Name}\", \"{input.Name}\")";
    }

    public string DoVisit(TokenExpressionWrapped input)
    {
        return "[" + (input.ToString() ?? "") + "]";
    }

    public string DoVisit(TokenExpressionTemporal input)
    {
        var tokens = new List<string>();

        tokens.Add(input.Child.Accept(this));

        if (input.TimePeriod is not null)
        {
            if (input.TimePeriod is TokenDouble)
            {
                //double will have unit attached
                tokens.Add($"{input.TimePeriod.Accept(this)}");
            }
            else
            {
                //wrap with parenthesis for non const values
                tokens.Add($"({input.TimePeriod.Accept(this)}){SerializeUnit(input.TimePeriod.Unit)}");
            }
        }

        if (input.TimeFrom is not null)
        {
            if (input.TimeFrom is TokenDouble)
            {
                tokens.Add($"{input.TimeFrom.Accept(this)}");
            }
            else
            {
                tokens.Add($"({input.TimeFrom.Accept(this)}){SerializeUnit(input.TimeFrom.Unit)}");
            }
        }

        return $"{input.FunctionName}({string.Join(", ", tokens)})";
    }

    private string SerializeLinqExpression(TokenExpressionLinq input)
    {
        return $"{input.Child.Accept(this)}";
    }

    public string DoVisit(TokenExpressionTimer input)
    {
        return (input.UnitOfMeasure != null) ? $"TIMER({input.Child.Accept(this)}, {input.UnitOfMeasure})" : $"TIMER({input.Child.Accept(this)})";
    }

    private static string? SerializeUnit(string unit)
    {
        if (!string.IsNullOrWhiteSpace(unit))
        {
            return $"[{unit}]";
        }

        return unit;
    }
}
