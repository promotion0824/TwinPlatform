using System.Linq;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace Willow.Expressions.Visitor;

/// <summary>
/// Calculates the units for an expression following some unit algebra, e.g. kwH + kwH = kwH
/// </summary>
public class UnitsVisitor : ITokenExpressionVisitor<string>
{
    public UnitsVisitor() : base()
    {
    }

    public string Visit(TokenExpression source)
    {
        return source.Accept(this);
    }

    public string DoVisit(TokenExpressionCount input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionAverage input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionMin input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionMax input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionFirst input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionAny input)
    {
        return "bool";
    }

    public string DoVisit(TokenExpressionIdentity input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionFailed input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionAll input)
    {
        return "bool";
    }

    public string DoVisit(TokenExpressionPropertyAccess input)
    {
        // The unit of a property is totally unrelated to the unit of the child being accessed
        // When accessing a twin property we really need to find the units for the property
        // if (input.Child is TokenExpressionTwin twin)
        // {
        //     // TODO: Access the properties of the underlying object, see BindToTwinsVisitor
        // }
        return input.Unit;
    }

    public string DoVisit(TokenExpressionVariableAccess input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionFunctionCall input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionConstant input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionConstantNull input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionConstantDateTime input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionConstantString input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionArray input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionConstantBool input)
    {
        return "bool";
    }

    public string DoVisit(TokenExpressionConstantColor input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenDouble input)
    {
        return string.IsNullOrEmpty(input.Unit) ? "" : input.Unit;
    }

    public string DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionAdd input)
    {
        if (!string.IsNullOrEmpty(input.Unit)) return input.Unit;  //e.g. (degC / 5/9) + 32F is degF
                                                                   // Allow scalar + units, e.g. 32 + ...
        var units = input.Children.Select(c => c.Accept(this)).Where(s => !string.IsNullOrEmpty(s)).ToList();
        if (!units.Any()) return "";
        if (units.Distinct().Count() < 2) return units.First();

        // TODO: Handle conversions we can do, e.g. kwH, wH
        return "error";
    }

    public string DoVisit(TokenExpressionMatches input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionDivide input)
    {
        if (!string.IsNullOrEmpty(input.Unit)) return input.Unit;  //e.g. (degC / 5/9) + 32F is degF
        var top = input.Left.Accept(this);
        var bottom = input.Right.Accept(this);
        if (top == bottom) return "";
        if (top == "") return "";
        if (bottom == "") return top;
        return $"{top}/{bottom}";   // e.g. m/s
    }

    public string DoVisit(TokenExpressionUnaryMinus input)
    {
        if (!string.IsNullOrEmpty(input.Unit)) return input.Unit;
        return input.Child.Accept(this);
    }

    public string DoVisit(TokenExpressionMultiply input)
    {
        if (!string.IsNullOrEmpty(input.Unit)) return input.Unit;  //e.g. (degF - 32) * 5 / 9 is degC
        var units = input.Children.Select(c => c.Accept(this)).Where(x => x != "").ToList();
        if (units.Count == 0 && input.Children.Length > 0) return "";
        // TODO: m^2 etc.
        return string.Join(".", units);
    }

    public string DoVisit(TokenExpressionPower input)
    {
        if (!string.IsNullOrEmpty(input.Unit)) return input.Unit;
        string left = input.Left.Accept(this);
        if (string.IsNullOrEmpty(left)) return left;
        else return $"{left}^{input.Right.Serialize()}"; // assumes power is a number
    }

    public string DoVisit(TokenExpressionSubtract input)
    {
        if (!string.IsNullOrEmpty(input.Unit)) return input.Unit;
        var left = input.Left.Accept(this);
        var right = input.Right.Accept(this);
        // Special case, for example when we subtract degC and degC the result is in degC but
        // the range isn't the normal 0-100C type of range, it's maybe -10 to +10
        // and we want that on its own area of the time series chart otherwise it gets lost
        // Can't handle this for now as we don't have a proper units library nor the concept of a delta
        if (left == right) return left;
        if (left == "") return right;
        if (right == "") return left;
        // TODO: Handle kwH, wH, ...
        return $"unknown";
    }

    public string DoVisit(TokenExpressionNot input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionAnd input)
    {
        return "bool";
    }

    public string DoVisit(TokenExpressionOr input)
    {
        return "bool";
    }

    public string DoVisit(TokenExpressionTernary input)
    {
        var left = input.Truth.Accept(this);
        var right = input.Falsehood.Accept(this);
        if (left == right) return left;
        // TODO: Handle kwH, wH, ...
        return $"unknown";
    }

    public string DoVisit(TokenExpressionEach input)
    {
        // body determines result type
        return input.Body.Accept(this);
    }

    public string DoVisit(TokenExpressionIntersection input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionSetUnion input)
    {
        return input.Unit;
    }

    private string UnitsMustMatch(TokenExpressionBinary input)
    {
        if (string.IsNullOrEmpty(input.Left.Unit)) return "bool";
        if (string.IsNullOrEmpty(input.Right.Unit)) return "bool";
        if (input.Left.Unit != input.Right.Unit) return "error";
        return "bool";
    }

    public string DoVisit(TokenExpressionIs input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionEquals input)
    {
        return UnitsMustMatch(input);
    }

    public string DoVisit(TokenExpressionGreater input)
    {
        return UnitsMustMatch(input);
    }

    public string DoVisit(TokenExpressionGreaterOrEqual input)
    {
        return UnitsMustMatch(input);
    }

    public string DoVisit(TokenExpressionLess input)
    {
        return UnitsMustMatch(input);
    }

    public string DoVisit(TokenExpressionLessOrEqual input)
    {
        return UnitsMustMatch(input);
    }

    public string DoVisit(TokenExpressionNotEquals input)
    {
        return UnitsMustMatch(input);
    }

    public string DoVisit(TokenExpressionTuple input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionSum input)
    {
        return input.Child.Accept(this);
    }

    public string DoVisit(TokenExpressionParameter input)
    {
        return input.Unit;
    }

    public string DoVisit(TokenExpressionWrapped input)
    {
        // If input is a TokenExpressionTwin this will return the unit from the twin itself
        return input.Unit;
    }

    public string DoVisit(TokenExpressionTemporal input)
    {
        if (input.FunctionName == "SLOPE")
        {
            if (input.Unit == "kWh") return "kW";
            else if (string.IsNullOrEmpty(input.Unit)) return "";
            else return input.Unit + "/day";
        }

        return input.Unit;
    }

    public string DoVisit(TokenExpressionTimer input)
    {
        return input.Unit;
    }
}
