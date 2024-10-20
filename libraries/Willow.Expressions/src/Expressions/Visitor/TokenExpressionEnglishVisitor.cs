using System.Globalization;
using System.Linq;

namespace Willow.Expressions.Visitor;

/// <summary>
/// A visitor to convert a TokenExpression to an English language form
/// </summary>
public class TokenExpressionEnglishVisitor : ITokenExpressionVisitor<string>
{
    private readonly bool metric;
    //private readonly int decimalPlaces;

    /// <summary>
    /// Creates a new instance of the <see cref="TokenExpressionEnglishVisitor"/> class
    /// </summary>
    public TokenExpressionEnglishVisitor(bool metric)
    {
        this.metric = metric;
        //this.decimalPlaces = 2;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string Visit(TokenExpression source)
    {
        // TBD: Should this be on every single Visit call also?? YES, otherwise it doesn't happen
        if (!string.IsNullOrEmpty(source.Text)) return source.Text;
        return source.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionMultiply input)
    {
        return $"({string.Join(" * ", input.Children.Select(c => c.Accept(this)))})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionPropertyAccess input)
    {
        return $"{input.Child.Accept(this)}.{input.PropertyName}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionVariableAccess input)
    {
        return $"{input.VariableName}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionFunctionCall input)
    {
        return $"{input.FunctionName}({string.Join(",", input.Children.Select(c => c.Accept(this)))})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionNotEquals input)
    {
        if (!string.IsNullOrEmpty(input.Text)) return input.Text;
        return $"({input.Left.Accept(this)} != {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionOr input)
    {
        return $"({string.Join(" or ", input.Children.Select(c => c.Accept(this)))})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionTernary input)
    {
        if (input.Falsehood != null)
            return $"(if {input.Conditional.Accept(this)} then {input.Truth.Accept(this)} else {input.Falsehood.Accept(this)})";
        else
            return $"(if {input.Conditional.Accept(this)} then {input.Truth.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionTuple input)
    {
        return $"{{{string.Join(", ", input.Children.Select(c => c.Accept(this)))}}}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionPower input)
    {
        return $"({input.Left.Accept(this)}^{input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionSubtract input)
    {
        return $"({input.Left.Accept(this)} - {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionIdentity input)
    {
        return $"({input.Child.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionFailed input)
    {
        return $"FAILED({string.Join(", ", input.Children.Select(c => c.Accept(this)))})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionIntersection input)
    {
        return $"({string.Join(" intersect ", input.Children.Select(c => c.Accept(this)))})";
    }

    /// <summary>
    /// Visit
    /// </summary>

    public virtual string DoVisit(TokenExpressionSetUnion input)
    {
        return $"({string.Join(" union ", input.Children.Select(c => c.Accept(this)))})";
    }

    private static string Ordinal(int value)
    {
        switch (value)
        {
            case 1: return "first";
            case 2: return "second";
            case 3: return "third";
            case 4: return "fourth";
            case 5: return "fifth";
            case 6: return "sixth";
            default:
                if (value % 10 == 1) return $"{value}st";
                if (value % 10 == 2) return $"{value}nd";
                if (value % 10 == 3) return $"{value}rd";
                return $"{value}th";
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenDouble input)
    {
        return input.DoDescribe(true);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        return $"LocalTime({input.Child.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionConstantColor input)
    {
        if (!string.IsNullOrEmpty(input.Text)) { return input.Text; }

        var color = TokenExpressionConstantColor.GetClosestNamedColor(input.R, input.G, input.B);

        if (color.Equals(input)) { return color.Text; }
        else { return $"color ({input.R},{input.G},{input.B}) near {color.Text}"; }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionArray input)
    {
        return $"[{string.Join(",", input.Children.Select(c => c.Accept(this)))}]";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionConstant input)
    {
        return $"{input.Value.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionConstantBool input)
    {
        // Should this be in quotes?
        return $"{input.Value.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionAdd input)
    {
        return $"({string.Join(" + ", input.Children.Select(c => c.Accept(this)))})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionMatches input)
    {
        return $"({input.Left.Accept(this)} matches {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionAnd input)
    {
        return $"({string.Join(" and ", input.Children.Select(c => c.Accept(this)))})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionConstantNull input)
    {
        return $"null";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionConstantDateTime input)
    {
        return $"'{input.Value.ToString(CultureInfo.InvariantCulture)}'";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionConstantString input)
    {
        return $"'{input.Value.ToString(CultureInfo.InvariantCulture)}'";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionDivide input)
    {
        return $"({input.Left.Accept(this)} / {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionIs input)
    {
        return $"({input.Left.Accept(this)} is {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionEquals input)
    {
        if (!string.IsNullOrEmpty(input.Text)) return input.Text;
        return $"({input.Left.Accept(this)} = {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionGreater input)
    {
        return $"({input.Left.Accept(this)} > {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionGreaterOrEqual input)
    {
        return $"({input.Left.Accept(this)} >= {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionNot input)
    {
        return $"not {input.Child.Accept(this)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionUnaryMinus input)
    {
        return $"-{input.Child.Accept(this)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionLess input)
    {
        return $"({input.Left.Accept(this)} < {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionLessOrEqual input)
    {
        return $"({input.Left.Accept(this)} <= {input.Right.Accept(this)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual string DoVisit(TokenExpressionSum input)
    {
        return $"sum {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionCount input)
    {
        return $"count {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionAverage input)
    {
        return $"average of {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionAny input)
    {
        return $"any {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionAll input)
    {
        return $"all {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionFirst input)
    {
        return $"first of {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionEach input)
    {
        return $"for each {input.VariableName.Accept(this)} in {input.EnumerableArgument.Accept(this)} return {input.Body.Accept(this)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionMin input)
    {
        return $"min {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionMax input)
    {
        return $"max {TranslateLinqExpression(input)}";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionParameter input)
    {
        return input.Name;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionWrapped input)
    {
        return input.BareObject?.ToString() ?? "";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public string DoVisit(TokenExpressionTemporal input)
    {
        return $"{input.FunctionName} for " + TranslateTemporalExpression(input);
    }

    private string TranslateTemporalExpression(TokenExpressionTemporal input)
    {
        return $"{input.Child.Accept(this)}{(input.TimePeriod is not null ? $" over {input.TimePeriod}" : "")}";
    }

    private string TranslateLinqExpression(TokenExpressionLinq input)
    {
        return $"{input.Child.Accept(this)}";
    }

    public string DoVisit(TokenExpressionTimer input)
    {
        return (input.UnitOfMeasure != null) ? $"TIMER({input.Child.Accept(this)}, {input.UnitOfMeasure})" : $"TIMER({input.Child.Accept(this)})";
    }
}
