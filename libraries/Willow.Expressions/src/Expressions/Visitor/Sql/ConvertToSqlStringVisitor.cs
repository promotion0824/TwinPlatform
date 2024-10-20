using System;
using System.Globalization;
using System.Linq;

namespace Willow.Expressions.Visitor.Sql;

/// <summary>
/// ConvertToSqlStringVisitor is similar to ConvertToExpression visitor
/// but date expressions convert to DatePart where necessary and the filter
/// is rendered as a string
/// </summary>
public class ConvertToSqlStringVisitor<TSource> : ConvertToSqlStringVisitorBase, ITokenExpressionVisitor<SqlQueryExpression>
{
    /// <summary>
    /// Visit
    /// </summary>
    public virtual SqlQueryExpression Visit(TokenExpression source)
    {
        return source.Simplify().Accept(this);
    }

    private SqlQueryExpression VisitBinary(TokenExpressionBinary input, string op)
    {
        var left = input.Left.Accept(this);
        var right = input.Right.Accept(this);
        return new SqlQueryExpression($"({left.QueryString} {op} {right.QueryString})", left.Parameters, right.Parameters);
    }

    private SqlQueryExpression VisitNary(TokenExpressionNary input, string op)
    {
        var children = input.Children.Select(c => c.Accept(this)).ToList();
        return new SqlQueryExpression($"({string.Join(" " + op + " ", children.Select(c => c.QueryString))})", children.SelectMany(c => c.Parameters).ToArray());
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionIdentity input)
    {
        return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionPropertyAccess input)
    {
        return input.Child.Accept(this).Append("." + input.PropertyName);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionVariableAccess input)
    {
        if (input.Type == typeof(bool))
        {
            // TODO: How to handle this better?

            // MSSQL doesn't have a bool field type, have to assume it's a bit or an int
            // and 1 = true
            return "[" + input.VariableName + "] = 1";
        }
        return "[" + input.VariableName + "]";
    }

    /// <summary>
    /// Fix table names that may have a schema name in them, e.g. dbo.table1 becomes [dbo].[table1]
    /// </summary>
    private static string BracketedNameFromDotted(string dottedName)
    {
        return "[" + string.Join("].[", dottedName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)) + "]";
    }

    private string CreateFunctionParameters(Type[] sqlFunctionTypes, TokenExpression[] parameters)
    {
        return string.Join(",", parameters.Zip(sqlFunctionTypes, (e, t) => CreateFunctionParameter(t, e)));
    }

    private SqlQueryExpression CreateFunctionParameter(Type sqlFunctionType, TokenExpression tokenExpression)
    {
        if (sqlFunctionType == typeof(DatePart) && tokenExpression is TokenExpressionConstantString)
        {
            string datepart = ((TokenExpressionConstantString)tokenExpression).Value.ToString(CultureInfo.InvariantCulture);
            return datepart;
        }
        else if (sqlFunctionType == typeof(DatePart) && tokenExpression is TokenExpressionVariableAccess)
        {
            // Handle unquoted dateparts even though that might be a missing variable warning
            string datepart = ((TokenExpressionVariableAccess)tokenExpression).VariableName;
            return datepart;
        }
        else
        {
            return tokenExpression.Accept(this);
        }
    }

    /// <summary>
    /// Sql Function Names that can be translated to SQL Function calls
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionFunctionCall input)
    {
        var fn = input.FunctionName.ToUpper();

        // TODO: Move all these special cases into the sqlFunction object
        if (fn == "STARTSWITH" && input.Children.Length == 2 && input.Children[1] is TokenExpressionConstantString)
        {
            var p1string = ((TokenExpressionConstantString)input.Children[1]).ValueString;
            var p1 = new TokenExpressionConstantString(p1string + "%");
            return input.Children[0].Accept(this) + " LIKE" + p1.Accept(this);
        }
        else if (fn == "ENDSWITH" && input.Children.Length == 2 && input.Children[1] is TokenExpressionConstantString)
        {
            var p1string = ((TokenExpressionConstantString)input.Children[1]).ValueString;
            var p1 = new TokenExpressionConstantString("%" + p1string);
            return input.Children[0].Accept(this) + " LIKE" + p1.Accept(this);
        }
        else if (fn == "CONTAINS" && input.Children.Length == 2 && input.Children[1] is TokenExpressionConstantString)
        {
            var p1string = ((TokenExpressionConstantString)input.Children[1]).ValueString;
            var p1 = new TokenExpressionConstantString("%" + p1string + "%");
            return input.Children[0].Accept(this) + " LIKE" + p1.Accept(this);
        }
        else if (fn == "LIKE" && input.Children.Length == 2)
        {
            // Acts as an operator not a function
            return input.Children[0].Accept(this) + " LIKE" + input.Children[1].Accept(this);
        }
        else if (fn == "NOT" && input.Children.Length == 1)
        {
            // Doesn't need parentheses
            return "NOT" + input.Children[0].Accept(this);
        }

        foreach (var sqlFunction in RegisteredFunctions)
        {
            // Same name
            if (!sqlFunction.Name.Equals(fn, StringComparison.InvariantCultureIgnoreCase)) continue;
            // Same length of arguments
            if (input.Children.Length != sqlFunction.ArgumentTypes.Length) continue;
            // TODO: Check parameters are correct type

            // Default behavior can handle it including special parameter types
            // like datepart which doesn't have quotes on it (even though input needs them)
            return $"{fn}(" + CreateFunctionParameters(sqlFunction.ArgumentTypes, input.Children) + ")";
        }
        return "Undefined";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionConstantNull input)
    {
        return "Null";
    }

    private int parameterSequence = 1;

    /// <summary>
    /// Visit
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionConstantDateTime input)
    {
        // Constants become parameters
        string paramName = "@p" + this.parameterSequence++;
        var parameter = new SqlParameter(paramName, typeof(DateTime), input.ValueDateTime);
        return new SqlQueryExpression(paramName, new[] { parameter });
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionConstantString input)
    {
        // Constants become parameters
        string paramName = "@p" + this.parameterSequence++;
        var parameter = new SqlParameter(paramName, typeof(string), input.ValueString);
        return new SqlQueryExpression(paramName, new[] { parameter });
    }

    /// <summary>
    /// Visit array becomes just a list with no parentheses
    /// </summary>
    public virtual SqlQueryExpression DoVisit(TokenExpressionArray input)
    {
        return VisitNary(input, ",");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionConstant input)
    {
        // Never called since the specific ones are implemented
        // Could make the others call through here to may overrides easier
        throw new NotImplementedException("Specific types of constant have their own calls, this should never happen");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionConstantBool input)
    {
        // Sql does not have boolean literal values
        // See http://stackoverflow.com/questions/7170688/sql-server-boolean-literal
        if (input.ValueBool)
        { return "1=1"; }
        else
        { return "1=0"; }
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionConstantColor input)
    {
        // Only makes sense when matching against a color field, not as a solitary constant
        return "1=0";
    }

    public virtual SqlQueryExpression DoVisit(TokenDouble input)
    {
        // Constants become parameters
        string paramName = "@p" + this.parameterSequence++;
        var parameter = new SqlParameter(paramName, typeof(double), input.ValueDouble);
        return new SqlQueryExpression(paramName, new[] { parameter });
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionIntersection input)
    {
        throw new NotImplementedException("An intersection of ranges cannot be visited");
        //            return VisitNary(input, " INTERSECT ");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionSetUnion input)
    {
        throw new NotImplementedException("A union of ranges cannot be visited");
        //           return VisitNary(input, " UNION ");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        var child = input.Child.Accept(this);
        // d => TimeZoneInfo.ConvertTimeFromUtc(d, TimeZoneInfo.Local);
        // NOTE: This does NOT handle daylight savings times
        // It's useful for activities that happen during the day to get them in the
        // right day but don't rely on it for more than that
        var offset = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;
        return $"DateAdd(minute, {offset}, {child})";
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionAdd input)
    {
        return VisitNary(input, "+");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionMatches input)
    {
        var lhs = input.Left.Accept(this);
        throw new NotImplementedException("Have only implemented matches for temporal set and range at the moment");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionDivide input)
    {
        return VisitBinary(input, "/");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionUnaryMinus input)
    {
        return $"-" + input.Child.Accept(this);
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionMultiply input)
    {
        return VisitNary(input, "*");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionPower input)
    {
        if (input.Right is TokenDouble constRight)
            return $"POWER({input.Left.Accept(this)},{constRight.ValueDouble})";
        else
            return $"POWER({input.Left.Accept(this)},{input.Right.Accept(this)})";
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionSubtract input)
    {
        return VisitBinary(input, "-");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionNot input)
    {
        return $"(NOT " + input.Child.Accept(this) + ")";
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionAnd input)
    {
        return VisitNary(input, "AND");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionOr input)
    {
        return VisitNary(input, "OR");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionTernary input)
    {
        // Don't translate constants in CASE statements to parameters
        SqlQueryExpression truth = (input.Truth is TokenDouble)
            ? new SqlQueryExpression(input.Truth.Serialize(), new SqlParameter[0])
            : input.Truth.Accept(this);

        SqlQueryExpression falsehood = (input.Falsehood is TokenDouble)
            ? new SqlQueryExpression(input.Falsehood.Serialize(), new SqlParameter[0])
            : input.Falsehood.Accept(this);

        if (input.Falsehood != null && input.Falsehood != TokenExpression.Null)
            return $"CASE WHEN " + input.Conditional.Accept(this) + " THEN " + truth + " ELSE " + falsehood + " END";
        else
            return $"CASE WHEN " + input.Conditional.Accept(this) + " THEN " + truth + " END";
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionTuple input)
    {
        return VisitNary(input, ", ");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionIs input)
    {
        if (input.Right == TokenExpression.Null)
        {
            return input.Left.Accept(this) + " IS NULL";
        }
        return VisitBinary(input, "=");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionEquals input)
    {
        if (input.Right == TokenExpression.Null)
        {
            return input.Left.Accept(this) + " IS NULL";
        }
        return VisitBinary(input, "=");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionGreater input)
    {
        return VisitBinary(input, ">");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionGreaterOrEqual input)
    {
        return VisitBinary(input, ">=");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionLess input)
    {
        return VisitBinary(input, "<");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionLessOrEqual input)
    {
        return VisitBinary(input, "<=");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionNotEquals input)
    {
        if (input.Right == TokenExpression.Null)
        {
            return input.Left.Accept(this) + " IS NOT NULL";
        }
        return VisitBinary(input, "!=");
    }

    public virtual SqlQueryExpression DoVisit(TokenExpressionSum input)
    {
        if (input.Child is TokenExpressionArray array) return "SUM (" + VisitNary(array, ", ") + ")";
        return "SUM(" + input.Child.Accept(this) + ")";
    }

    public SqlQueryExpression DoVisit(TokenExpressionCount input)
    {
        if (input.Child is TokenExpressionArray array) return "COUNT (" + VisitNary(array, ", ") + ")";
        return "COUNT(" + input.Child.Accept(this) + ")";
    }

    public SqlQueryExpression DoVisit(TokenExpressionAverage input)
    {
        if (input.Child is TokenExpressionArray array) return "AVERAGE (" + VisitNary(array, ", ") + ")";
        return "AVERAGE(" + input.Child.Accept(this) + ")";
    }

    public SqlQueryExpression DoVisit(TokenExpressionAny input)
    {
        if (input.Child is TokenExpressionArray array) return "ANY (" + VisitNary(array, ", ") + ")";
        return "ANY(" + input.Child.Accept(this) + ")";
    }

    public SqlQueryExpression DoVisit(TokenExpressionAll input)
    {
        if (input.Child is TokenExpressionArray array) return "ALL (" + VisitNary(array, ", ") + ")";
        return "ALL(" + input.Child.Accept(this) + ")";
    }

    public SqlQueryExpression DoVisit(TokenExpressionEach input)
    {
        // It will be something like this .. but this is NOT going to work
        return $"SELECT {input.Body.Accept(this)} FROM {input.EnumerableArgument.Accept(this)}";
    }

    /// <inheritdoc />
    public SqlQueryExpression DoVisit(TokenExpressionMin input)
    {
        if (input.Child is TokenExpressionArray array) return "MIN (" + VisitNary(array, ", ") + ")";
        return "MIN(" + input.Child.Accept(this) + ")";
    }

    /// <inheritdoc />
    public SqlQueryExpression DoVisit(TokenExpressionFirst input)
    {
        if (input.Child is TokenExpressionArray array) return "FIRST (" + VisitNary(array, ", ") + ")";
        return "FIRST(" + input.Child.Accept(this) + ")";
    }

    public SqlQueryExpression DoVisit(TokenExpressionMax input)
    {
        if (input.Child is TokenExpressionArray array) return "MAX (" + VisitNary(array, ", ") + ")";
        return "MAX(" + input.Child.Accept(this) + ")";
    }

    public SqlQueryExpression DoVisit(TokenExpressionParameter input)
    {
        // TODO: Eh? What would object value be for an unbound parameter like this?
        return new SqlQueryExpression(input.Name, new SqlParameter[] { new SqlParameter(input.Name, input.Type, null) });
    }

    public SqlQueryExpression DoVisit(TokenExpressionWrapped input)
    {
        throw new NotImplementedException("Can't convert an object to SQL");
    }

    public SqlQueryExpression DoVisit(TokenExpressionTemporal input)
    {
        return $"{input.FunctionName}({input.Child.Accept(this)},{input.TimePeriod})";
    }

    public SqlQueryExpression DoVisit(TokenExpressionFailed input)
    {
        return $"FAILED({string.Join(",", input.Children.Select(x => x.Accept(this)))})";  // will not work
    }

    public SqlQueryExpression DoVisit(TokenExpressionTimer input)
    {
        return (input.UnitOfMeasure != null) ? $"TIMER({input.Child.Accept(this)}, {input.UnitOfMeasure})" : $"TIMER({input.Child.Accept(this)})";
    }
}
