using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace Willow.Expressions.Visitor;

/// <summary>
/// An undefined IConvertible result
/// </summary>
public static class UndefinedResult
{
    /// <summary>
    /// An undefined IConvertible value
    /// </summary>
    public static readonly IConvertible Undefined = (IConvertible)"undefined";

    //private UndefinedResult() { }
}

/// <summary>
/// Evaluate an expression against type TSource (e.g. ISong, Env, ...)
/// </summary>
public class ConvertToValueVisitor<TSource> : ITokenExpressionVisitor<IConvertible>
{
    /// <summary>
    /// A TimeZoneOffset for the local / UTC conversions
    /// </summary>
    public TimeSpan TimeZoneOffset { get; set; } = TimeZoneInfo.Local.BaseUtcOffset;

    /// <summary>
    /// Indicator whether Visitor was successful
    /// </summary>
    public bool Success { get; private set; } = true;

    /// <summary>
    /// The error when visiting wasn't Successful
    /// </summary>
    public string Error { get; private set; } = "";

    /// <summary>
    /// A singleton Undefined value
    /// </summary>
    public static readonly IConvertible Undefined = UndefinedResult.Undefined;

    private readonly ConvertToExpressionVisitor<TSource> exprVisitorForTSource =
        new ConvertToExpressionVisitor<TSource>(
            ConvertToExpressionVisitor<TSource>.GetterForObjectUsingReflection);

    private static readonly ConvertToExpressionVisitor<DateTime> exprVisitorForDateTime =
        new ConvertToExpressionVisitor<DateTime>(
            ConvertToExpressionVisitor<DateTime>.GetterForObjectUsingReflection);

    private readonly TSource source;

    /// <summary>
    /// Gets a field from an entity. TSource is typically a DataRow
    /// </summary>
    private readonly Func<TSource, string, object> variableGetter;

    /// <summary>
    /// Gets a temporal object when implicitly required by temporal related expressions
    /// </summary>
    private readonly Func<string, ITemporalObject?> temporalObjectGetter;

    /// <summary>
    /// Machine learning models
    /// </summary>
    private readonly Func<string, IMLRuntime?> mlModelGetter;

    /// <summary>
    /// A Getter for an Object that firsts looks at the source object but falls back to the env values
    /// </summary>
    private Expression GetterForObjectUsingReflection(Expression instance, string variableName)
    {
        var expression = ConvertToExpressionVisitor<TSource>.GetterForObjectUsingReflection(instance, variableName);

        if (expression == ExpressionUndefined.Instance)
        {
            //try from env values
            var variable = variableGetter(this.source, variableName);

            if (variable is not null)
            {
                return Expression.Constant(variable);
            }
        }

        return expression;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ConvertToValueVisitor{TSource}"/> class
    /// </summary>
    public ConvertToValueVisitor(TSource source,
        Func<TSource, string, object> variableGetter,
        Func<string, ITemporalObject?>? temporalObjectGetter = null,
        Func<string, IMLRuntime?>? mlModelGetter = null)
    {
        this.source = source;
        this.variableGetter = variableGetter;
        this.temporalObjectGetter = temporalObjectGetter ?? ((string v) => null);
        this.mlModelGetter = mlModelGetter ?? ((string v) => null);
        exprVisitorForTSource = new ConvertToExpressionVisitor<TSource>(GetterForObjectUsingReflection);
    }

    /// <summary>
    /// A sample Getter for use against Objects (using reflection)
    /// </summary>
    public static object ObjectGetter(TSource source, string variableName)
    {
        // e.g. .On needs to be a access to the On property of a TSource
        var propertyInfo = typeof(TSource).GetInterfaces()
            .Concat(Enumerable.Repeat(typeof(TSource), 1))
            .Select(i => i.GetProperty(variableName, BindingFlags.Instance | BindingFlags.Public))
            .FirstOrDefault(x => x != null);

        if (propertyInfo is null) return Undefined;

        return propertyInfo.GetValue(source, null)!;
    }

    /// <summary>
    /// A sample Getter for use against an environment
    /// </summary>
    public static object ObjectGetterFromEnvironment(Env source, string variableName)
    {
        return source.Get(variableName)!;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible Visit(TokenExpression input)
    {
        if (ReferenceEquals(input, null)) return Undefined;
        var expr = input.Accept(this);
        return expr;
    }

    /// <summary>
    /// Create a combo from a binary expression
    /// </summary>
    protected virtual IConvertible CreateCombo(TokenExpressionBinary input,
        Func<IConvertible, IConvertible, IConvertible> create)
    {
        if (input.Left.Equals(Undefined)) return Undefined;
        if (input.Right.Equals(Undefined)) return Undefined;
        return CreateCombo(input.Left, input.Right, create);
    }

    /// <summary>
    /// Create a combo from am expression
    /// </summary>
    protected virtual IConvertible CreateCombo(TokenExpression leftHalf,
        TokenExpression rightHalf,
        Func<IConvertible, IConvertible, IConvertible> create)
    {
        if (leftHalf is null) throw new ArgumentNullException(nameof(leftHalf));
        if (rightHalf is null) throw new ArgumentNullException(nameof(rightHalf));
        var left = leftHalf.Accept(this);
        var right = rightHalf.Accept(this);
        return create(left, right);
    }

    /// <summary>
    /// Create a combo from an Nary expression
    /// </summary>
    protected virtual IConvertible CreateCombo(TokenExpressionNary input,
        Func<IConvertible, IConvertible, IConvertible> create)
    {
        var children = input.Children.Select(c => (c.Accept(this))).ToList();
        var combined = children.Skip(1).Aggregate(children.First(), create);
        return combined;
    }

    // ReSharper disable once StaticMemberInGenericType
    /// <summary>
    /// The True Expression
    /// </summary>
    protected static readonly Expression True = Expression.Constant(true);

    /// <summary>
    /// The False Expression
    /// </summary>
    protected static readonly Expression False = Expression.Constant(false);

    // ReSharper disable once StaticMemberInGenericType

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        var child = input.Child.Accept(this);
        if (!IsDateTime(child))
        {
            return Undefined;
        }

        // OR ... Assume same side of summer time as now
        // var offset = TimeProvider.Current.Now.Offset;

        return TimeZoneInfo.ConvertTimeFromUtc(child.ToDateTime(CultureInfo.InvariantCulture), TimeZoneInfo.Local);
    }

    private static bool IsNumeric(Type t) => t == typeof(int) || t == typeof(long) || t == typeof(double) || t == typeof(decimal);

    private static bool IsString(Type t) => t == typeof(string);

    private static bool IsBoolean(Type t) => t == typeof(bool);

    private static bool IsDateTime(Type t) => t == typeof(DateTime);

    private static bool IsTimeSpan(Type t) => t == typeof(TimeSpan);

    private static bool BothBoolean(Type a, Type b) => IsBoolean(a) && IsBoolean(b);

    private static bool BothNumeric(Type a, Type b) => IsNumeric(a) && IsNumeric(b);

    private static bool BothString(Type a, Type b) => IsString(a) && IsString(b);

    private static bool CanCompare(Type a, Type b) => BothNumeric(a, b) || BothString(a, b);

    private static bool CanAdd(Type a, Type b) => (IsNumeric(a) && IsNumeric(b)) ||
       ((IsDateTime(a) && IsTimeSpan(b)));

    private static bool CanSubtract(Type a, Type b) => (IsNumeric(a) && IsNumeric(b)) ||
        (IsDateTime(a) && IsDateTime(b)) || (IsDateTime(a) && IsTimeSpan(b));

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionAdd input)
    {
        return CreateCombo(input, (IConvertible x, IConvertible y) =>
            x.ToDouble(CultureInfo.InvariantCulture) + y.ToDouble(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionMultiply input)
    {
        return CreateCombo(input, (IConvertible x, IConvertible y) =>
            x.ToDouble(CultureInfo.InvariantCulture) * y.ToDouble(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts match (variable, temporal expression) into an expression returning a bool
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionMatches input)
    {
        var left = input.Left.Accept(this);
        if (left == Undefined) return Undefined;

        if (input.Right is TokenDouble @double)
        {
            // Just use equality
            return left.Equals(@double.ValueDouble);
        }
        else
        {
            Trace.TraceError($"Match could not be converted to expression: {input.Serialize()}");
            throw new Exception($"Matches can only be used with a TemporalSet, a Range, a Set or a Double, not a {input.Serialize()} in ConvertToValue");
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionAnd input)
    {
        try
        {
            foreach (var child in input.Children)
            {
                var value = child.Accept(this);

                if (value.Equals(Undefined))
                {
                    return Undefined;
                }

                if (!HandleBoolOrNumber(value))
                {
                    return false;
                }
            }

            return true;
        }
        catch (VisitorException ex)
        {
            throw new VisitorException($"Failed to create And expression from '{input.Serialize()}'", ex);
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionOr input)
    {
        try
        {
            foreach (var child in input.Children)
            {
                var value = child.Accept(this);

                if (value.Equals(Undefined))
                {
                    return Undefined;
                }

                if (HandleBoolOrNumber(value))
                {
                    return true;
                }
            }

            return false;
        }
        catch (VisitorException ex)
        {
            throw new VisitorException($"Failed to create Or expression from '{input.Serialize()}'", ex);
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionTernary input)
    {
        try
        {
            var conditional = input.Conditional.Accept(this);

            if (conditional.ToBoolean(CultureInfo.InvariantCulture))
            {
                return input.Truth.Accept(this);
            }
            else
            {
                return input.Falsehood?.Accept(this) ?? Undefined;
            }
        }
        catch (VisitorException ex)
        {
            throw new VisitorException($"Failed to create Ternary expression from '{input.Serialize()}'", ex);
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionTuple input)
    {
        var arguments = input.Children.Select(c => c.Accept(this)).ToList();
        // Hmmm ... how to make Tuple IConvertible ... use string for now
        return $"Tuple({string.Join(",", arguments)})";
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionConstantNull input)
    {
        return Undefined;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionConstantDateTime input)
    {
        return input.ValueDateTime;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionConstantString input)
    {
        return input.Value;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionArray input)
    {
        // TODO: Decide how we want to handle this - what does it mean to have an array at this point in the process?

        if (!input.Children.Any()) return false;
        return input.Children.First().Accept(this);

        // var bodies = input.Children.Select(c => c.Accept(this)).ToArray();
        // return new TokenExpressionArray(bodies.Select(b => TokenExpressionConstant.Create(b)).ToArray());
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionDivide input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => a + "/" + b,      // not really
            (a, b) => a / b,
            (a, b) => false);           // also not really
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenDouble input)
    {
        return input.Value;
    }

    // Use Range with Matches which will Apply the function to the variable
    // Matches(width, Range(1,10))
    // NOTE: This DOES produce a lambda expression p => p >= min, p <= max

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionConstant input)
    {
        // Never called since the specific ones are implemented
        return input.Value;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionConstantBool input)
    {
        return input.Value;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionConstantColor input)
    {
        return input.Value;
    }

    private IConvertible HandleStringBoolOrDouble(TokenExpressionBinary input,
        Func<string, string, IConvertible> stringFun,
        Func<double, double, IConvertible> doubleFun,
        Func<bool, bool, IConvertible> boolFun)
    {
        IConvertible left = input.Left.Accept(this);
        IConvertible right = input.Right.Accept(this);

        if (IsNumber(left) && IsNumber(right))
            return doubleFun(left.ToDouble(CultureInfo.InvariantCulture), right.ToDouble(CultureInfo.InvariantCulture));

        if (IsBoolean(left) && IsBoolean(right))
            return boolFun(left.ToBoolean(CultureInfo.InvariantCulture), right.ToBoolean(CultureInfo.InvariantCulture));

        // Allow some permissive logic comparing bools and numbers, take number to be non zero for true
        if (IsBoolean(left) && IsNumber(right))
            return boolFun(left.ToBoolean(CultureInfo.InvariantCulture), right.ToDouble(CultureInfo.InvariantCulture) > 0);

        if (IsNumber(left) && IsBoolean(right))
            return boolFun(left.ToDouble(CultureInfo.InvariantCulture) > 0, right.ToBoolean(CultureInfo.InvariantCulture));

        if (IsString(left) && IsString(right))
            return stringFun(left.ToString(CultureInfo.InvariantCulture), right.ToString(CultureInfo.InvariantCulture));

        return Undefined;
    }

    private bool HandleBoolOrNumber(IConvertible value)
    {
        if (IsBoolean(value)) return value.ToBoolean(CultureInfo.InvariantCulture);
        if (IsNumber(value)) return value.ToDouble(CultureInfo.InvariantCulture) > 0;
        throw new VisitorException($"Value is not convertible to a boolean '{value}' {source}");
    }

    private static bool IsType(IConvertible value, TypeCode code)
    {
        if (value == Undefined)
        {
            return false;
        }

        return value.GetTypeCode() == code;
    }

    public static bool IsNumber(IConvertible value)
    {
        var typeCode = value.GetTypeCode();

        //double check first (most common)
        if (typeCode == TypeCode.Double ||
            typeCode == TypeCode.Int32 ||
            typeCode == TypeCode.Int64 ||
            typeCode == TypeCode.UInt64 ||
            typeCode == TypeCode.UInt32 ||
            typeCode == TypeCode.Single ||
            typeCode == TypeCode.Decimal ||
            typeCode == TypeCode.Int16 ||
            typeCode == TypeCode.SByte ||
            typeCode == TypeCode.Byte)
        {
            return true;
        }

        return false;
    }

    private static bool IsNumberOrBool(IConvertible value)
    {
        return IsNumber(value) || value.GetTypeCode() == TypeCode.Boolean;
    }

    private static bool IsString(IConvertible value)
    {
        return IsType(value, TypeCode.String);
    }

    private static bool IsDateTime(IConvertible value)
    {
        return IsType(value, TypeCode.DateTime);
    }

    private static bool IsDouble(IConvertible value)
    {
        return IsType(value, TypeCode.Double);
    }

    public static bool IsBoolean(IConvertible value)
    {
        return IsType(value, TypeCode.Boolean);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionIs input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => a.Equals(b, StringComparison.InvariantCulture),
            (a, b) => a.Equals(b),
            (a, b) => a.Equals(b));
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionEquals input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => a.Equals(b, StringComparison.InvariantCulture),
            (a, b) => a.Equals(b),
            (a, b) => a.Equals(b));
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionGreater input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => string.Compare(a, b, StringComparison.Ordinal) > 0,
            (a, b) => a > b,
            (a, b) => a && !b);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionGreaterOrEqual input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => string.Compare(a, b, StringComparison.Ordinal) >= 0,
            (a, b) => a >= b,
            (a, b) => a || !b);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionNot input)
    {
        var child = input.Child.Accept(this);
        bool value = HandleBoolOrNumber(child);
        return !value;
        //return Undefined;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionUnaryMinus input)
    {
        var child = input.Child.Accept(this);
        if (IsDouble(child)) return -child.ToDouble(CultureInfo.InvariantCulture);
        return Undefined;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionLess input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => string.Compare(a, b, StringComparison.Ordinal) < 0,
            (a, b) => a < b,
            (a, b) => b && !a);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionLessOrEqual input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => string.Compare(a, b, StringComparison.Ordinal) <= 0,
            (a, b) => a <= b,
            (a, b) => !a || b);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionNotEquals input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => string.Compare(a, b, StringComparison.Ordinal) != 0,
            (a, b) => !a.Equals(b),
            (a, b) => !a.Equals(b));
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionPower input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => a + "^" + b,      // not really
            (a, b) => Math.Pow(a, b),
            (a, b) => Undefined);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionSubtract input)
    {
        return HandleStringBoolOrDouble(input,
            (a, b) => a + "-" + b,      // not really
            (a, b) => a - b,
            (a, b) => Undefined);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionIdentity input)
    {
        return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionFailed input)
    {
        throw new ArgumentException("Cannot calculate a failed expression");
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionPropertyAccess input)
    {
        var expression = exprVisitorForTSource.Visit(input);

        if (expression is ConstantExpression ce)
        {
            if (ce.Value is Dictionary<string, object> bag)
            {
                return JsonConvert.SerializeObject(bag);
            }

            return ce.Value as IConvertible ?? double.NaN;
        }

        var rightAsExpression = expression as LambdaExpression;
        if (rightAsExpression is null) return Undefined;
        if (rightAsExpression == ExpressionUndefined.Instance) return Undefined;
        if (rightAsExpression.Parameters.Count == 0)
        {
            return (IConvertible)rightAsExpression.Compile().DynamicInvoke()!;
        }

        var inner = input.Child.Accept(this);
        return (IConvertible)rightAsExpression.Compile().DynamicInvoke(inner)!;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionVariableAccess input)
    {
        IConvertible value = (this.variableGetter(this.source, input.VariableName) as IConvertible) ?? double.NaN;
        return value;
    }

    private static readonly Dictionary<string, Func<double, IConvertible>> BuiltInSingleParameterStaticFunctions = new Dictionary<string, Func<double, IConvertible>>(StringComparer.OrdinalIgnoreCase)
    {
        ["ABS"] = (double p1) => Math.Abs(p1),
        ["ACOS"] = (double p1) => Math.Acos(p1),
        ["ASIN"] = (double p1) => Math.Asin(p1),
        ["ATAN"] = (double p1) => Math.Atan(p1),
        ["CEILING"] = (double p1) => Math.Ceiling(p1),
        ["COS"] = (double p1) => Math.Cos(p1),
        // COT, DEGREES, EXP, PI, RADIANS, RAND, SQUARE
        ["FLOOR"] = (double p1) => Math.Floor(p1),
        ["LOG"] = (double p1) => Math.Log(p1),
        ["LOG10"] = (double p1) => Math.Log10(p1),
        ["ROUND"] = (double p1) => Math.Round(p1),
        ["SIGN"] = (double p1) => Math.Sign(p1),
        ["SIN"] = (double p1) => Math.Sin(p1),
        ["SQRT"] = (double p1) => Math.Sqrt(p1),
        ["TAN"] = (double p1) => Math.Tan(p1),
        ["ISNAN"] = (double p1) => double.IsNaN(p1),
    };

    private static readonly Dictionary<string, Func<DateTime, IConvertible>> BuiltInSingleParameterDateFunctions = new Dictionary<string, Func<DateTime, IConvertible>>(StringComparer.OrdinalIgnoreCase)
    {
        ["HOUR"] = (DateTime p1) => WillowMath.Hour(p1),
        ["MINUTE"] = (DateTime p1) => WillowMath.Minute(p1),
        ["DAY"] = (DateTime p1) => WillowMath.Day(p1),
        ["DAYOFWEEK"] = (DateTime p1) => WillowMath.DayOfWeek(p1),
        ["MONTH"] = (DateTime p1) => WillowMath.Month(p1),
    };

    private static readonly Dictionary<string, Func<string, IConvertible>> BuiltInSingleParameterStringFunctions = new Dictionary<string, Func<string, IConvertible>>(StringComparer.OrdinalIgnoreCase)
    {
        ["TOUPPER"] = (string p1) => p1.ToUpper(),
        ["TOUPPERINVARIANT"] = (string p1) => p1.ToUpperInvariant(),
        ["TOLOWER"] = (string p1) => p1.ToLower(),
        ["TOLOWERINVARIANT"] = (string p1) => p1.ToLowerInvariant(),
        ["TRIM"] = (string p1) => p1.Trim(),
    };

    private static readonly Dictionary<string, Func<string, string, IConvertible>> BuiltInTwoParameterStringFunctions = new Dictionary<string, Func<string, string, IConvertible>>(StringComparer.OrdinalIgnoreCase)
    {
        ["CONTAINS"] = (string p1, string p2) => p1.Contains(p2),
        ["ENDSWITH"] = (string p1, string p2) => p1.EndsWith(p2),
        ["STARTSWITH"] = (string p1, string p2) => p1.StartsWith(p2),
    };

    private static readonly Dictionary<string, Func<double, double, IConvertible>> BuiltInTwoParameterFunctions = new Dictionary<string, Func<double, double, IConvertible>>(StringComparer.OrdinalIgnoreCase)
    {
        ["POW"] = (double p1, double p2) => Math.Pow(p1, p2),
        ["ATAN2"] = (double p1, double p2) => Math.Atan2(p1, p2),
        ["IFNAN"] = (double p1, double p2) => double.IsNaN(p1) ? p2 : p1,
        ["RND"] = (double p1, double p2) => WillowMath.Random((int)p1, (int)p2),
    };

    private static readonly Dictionary<string, Func<double, double, double, IConvertible>> BuiltInThreeParameterFunctions = new Dictionary<string, Func<double, double, double, IConvertible>>(StringComparer.OrdinalIgnoreCase)
    {
        ["DEADBAND"] = (double p1, double p2, double p3) => WillowMath.Deadband(p1, p2, p3),
    };

    // IF
    // CHOOSE

    // String functions
    // ASCII
    // CHAR
    // CHARINDEX
    // CONCAT
    // DIFFERENCE
    // FORMAT
    // LEFT
    // LEN          // LENGTH is a Property on Strings, not a function
    // LOWER
    // LTRIM
    // NCHAR
    // PATINDEX
    // QUOTENAME
    // REPLACE
    // REPLICATE
    // REVERSE
    // RIGHT
    // RTRIM
    // SOUNDEX
    // SPACE
    // STR
    // STRING_ESCAPE
    // STRING_SPLIT
    // STUFF
    // SUBSTRING
    // UNICODE
    // UPPER

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionFunctionCall input)
    {
        var children = input.Children;

        // SIN, COS, ... and other SingleParameter math functions

        if (BuiltInSingleParameterStaticFunctions.TryGetValue(input.FunctionName, out var singleParameterFunction))
        {
            if (children.Length != 1) return Undefined;

            var p1 = children[0].Accept(this);

            if (!IsNumberOrBool(p1)) return Undefined;

            return singleParameterFunction(p1.ToDouble(null));
        }

        if (BuiltInTwoParameterFunctions.TryGetValue(input.FunctionName, out var twoParameterFunction))
        {
            if (children.Length != 2) return Undefined;

            var p1 = children[0].Accept(this);
            var p2 = children[1].Accept(this);

            if (!IsNumberOrBool(p1) || !IsNumberOrBool(p2)) return Undefined;

            return twoParameterFunction(p1.ToDouble(null), p2.ToDouble(null));
        }

        if (BuiltInSingleParameterDateFunctions.TryGetValue(input.FunctionName, out var singleParameterDateFunction))
        {
            if (children.Length != 1) return Undefined;

            var p1 = children[0].Accept(this);

            DateTime dateParam;

            if (IsDateTime(p1))
            {
                dateParam = p1.ToDateTime(null);
            }
            else if (IsNumber(p1))
            {
                //assume the value is Ticks
                dateParam = new DateTime(p1.ToInt64(null));
            }
            else
            {
                return Undefined;
            }

            return singleParameterDateFunction(dateParam);
        }

        if (BuiltInSingleParameterStringFunctions.TryGetValue(input.FunctionName, out var singleParameterStringFunction))
        {
            if (children.Length != 1) return Undefined;

            var p1 = children[0].Accept(this);

            return singleParameterStringFunction(p1.ToString(null));
        }

        if (BuiltInTwoParameterStringFunctions.TryGetValue(input.FunctionName, out var twoParameterStringFunction))
        {
            if (children.Length != 2) return Undefined;
            var p1 = children[0].Accept(this);
            var p2 = children[1].Accept(this);

            return twoParameterStringFunction(p1.ToString(null), p2.ToString(null));
        }

        if (BuiltInThreeParameterFunctions.TryGetValue(input.FunctionName, out var threeParameterFunction))
        {
            if (children.Length != 3) return Undefined;

            var p1 = children[0].Accept(this);
            var p2 = children[1].Accept(this);
            var p3 = children[2].Accept(this);

            if (!IsNumberOrBool(p1) || !IsNumberOrBool(p2) || !IsNumberOrBool(p3)) return Undefined;

            return threeParameterFunction(p1.ToDouble(null), p2.ToDouble(null), p3.ToDouble(null));
        }

        var mlModel = mlModelGetter(input.FunctionName);

        if (mlModel is not null)
        {
            var values = input.Children.Select(v =>
            {
                var visitedChild = v.Accept(this);

                return new IConvertible[] { visitedChild };
            }).ToArray();

            return mlModel.Run(values);
        }

        var visitedChildren = input.Children.Select(c => c.Accept(this)).ToList();
        if (visitedChildren.Any(c => c == Undefined)) return Undefined;

        switch (input.FunctionName)
        {
            case "average":
                double average = visitedChildren.Select(c => c.ToDouble(CultureInfo.InvariantCulture)).Average();
                return average;

            default:
                {
                    // Get the function to call from the TSource passed in
                    // TBD: Should this be some other environment?
                    var propertyInfo = typeof(TSource).GetInterfaces()
                        .Concat(Enumerable.Repeat(typeof(TSource), 1))
                        .Select(i => i.GetProperty(input.FunctionName, BindingFlags.Instance | BindingFlags.Public))
                        .FirstOrDefault(x => x != null);

                    if (propertyInfo is null) return Undefined;

                    // The value passed on the source must be an Expression<Func<...>>
                    var fn = (LambdaExpression)propertyInfo.GetValue(this.source, null)!;

                    var compiled = fn.Compile();
                    var methodInfo = compiled.Method;

                    var convertedParameters =
                        visitedChildren.Skip(1)
                            .Zip(methodInfo.GetParameters()
                                .Select(p => p.ParameterType),
                                (p, q) => p.GetType() == q ? p : Convert.ChangeType(p, q));

                    return (IConvertible)compiled.DynamicInvoke(visitedChildren, convertedParameters.ToArray())!;
                }
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionIntersection input)
    {
        throw new NotImplementedException("Need to visit sides, and do a Range or Set union on them");
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionSetUnion input)
    {
        throw new NotImplementedException("Need to visit sides, and do a Range or Set union on them");
    }

    /// <summary>
    /// Create a range test expression
    /// </summary>
    /// <returns></returns>
    protected Expression<Func<DateTime, DateTime, DateTime, bool>> InRange()
    {
        Expression<Func<DateTime, DateTime, DateTime, bool>> expr = (start, end, d) => start <= d && d <= end;
        return expr;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionSum input)
    {
        if (input.Child is TokenExpressionArray array) return array.Children.Sum(c => c.Accept(this).ToDouble(CultureInfo.InvariantCulture));

        return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionCount input)
    {
        if (input.Child is TokenExpressionArray array) return array.Children.Count(c => c.Accept(this).ToBoolean(CultureInfo.InvariantCulture));

        return input.Child.Accept(this).ToBoolean(CultureInfo.InvariantCulture) ? 1 : 0;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionAverage input)
    {
        if (input.Child is TokenExpressionArray array) return array.Children.Average(c => c.Accept(this).ToDouble(CultureInfo.InvariantCulture));

        return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionAny input)
    {
        if (input.Child is TokenExpressionArray array) return array.Children.Any(c => c.Accept(this).ToBoolean(CultureInfo.InvariantCulture));

        return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionAll input)
    {
        if (input.Child is TokenExpressionArray array) return array.Children.All(c => c.Accept(this).ToBoolean(CultureInfo.InvariantCulture));

        return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionFirst input)
    {
        if (input.Child is TokenExpressionArray array) return array.Children.FirstOrDefault()?.Accept(this)!;
        else return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionMin input)
    {
        if (input.Child is TokenExpressionArray array)
        {
            if (array.Children.Length == 0) throw new ArgumentException("MIN applied to an array with no elements");
            return array.Children.Min(c => c.Accept(this).ToDouble(CultureInfo.InvariantCulture));
        }

        return input.Child.Accept(this);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual IConvertible DoVisit(TokenExpressionMax input)
    {
        if (input.Child is TokenExpressionArray array)
        {
            if (array.Children.Length == 0) throw new ArgumentException("MAX applied to an array with no elements");
            return array.Children.Max(c => c.Accept(this).ToDouble(CultureInfo.InvariantCulture));
        }

        return input.Child.Accept(this);
    }

    public virtual IConvertible DoVisit(TokenExpressionEach input)
    {
        throw new Exception("Cannot convert an each expression to a value, did you forget to bind?");
    }

    //protected LambdaExpression ChangeFirstParameter(LambdaExpression lambda, ParameterExpression substitute)
    //{
    //    if (!lambda.Parameters.Any()) return lambda;
    //    var swap = new ExpressionFactory.ExpressionSubstitute(lambda.Parameters[0], substitute);
    //    var parameters = Enumerable.Repeat(substitute,1).Concat(lambda.Parameters.Skip(1)).ToArray();
    //    return Expression.Lambda(swap.Visit(lambda.Body), parameters);
    //}

    /// <inheritdoc />
    public virtual IConvertible DoVisit(TokenExpressionParameter input)
    {
        throw new Exception("Cannot convert a parameter to a value, did you forget to bind?");
    }

    public virtual IConvertible DoVisit(TokenExpressionWrapped input)
    {
        //during simulation we get ruleinstance params that might be wrapped twins (in memory) that aren't capabilities, but they should
        //still be allowed in the expression even if it is not a capability. Once the ri is read from the DB, theses expression should be serialized as variable access tokens
        //which similarily returns Nans if no capability found for the twinid. And because it always
        //returns nan, the buffer in the Actor should not get created
        //throw new Exception("Cannot convert a wrapped object to a value, did you forget to bind?");
        return double.NaN;
    }

    private static readonly Dictionary<string, Func<ITemporalObject, UnitValue, UnitValue, IConvertible>> TemporalFunctions = new Dictionary<string, Func<ITemporalObject, UnitValue, UnitValue, IConvertible>>(StringComparer.OrdinalIgnoreCase)
    {
        ["ALL"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.All(startPeriod, endPeriod),
        ["ANY"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Any(startPeriod, endPeriod),
        ["AVERAGE"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Average(startPeriod, endPeriod),
        ["COUNT"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Count(startPeriod, endPeriod),
        ["COUNTLEADING"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Count(startPeriod, endPeriod),
        ["DELTA"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Delta(startPeriod, endPeriod),
        ["FORECAST"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Forecast(startPeriod, endPeriod),
        ["MAX"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Max(startPeriod, endPeriod),
        ["MIN"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Min(startPeriod, endPeriod),
        ["SLOPE"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.Slope(startPeriod, endPeriod),
        ["STND"] = (ITemporalObject temporal, UnitValue startPeriod, UnitValue endPeriod) => temporal.StandardDeviation(startPeriod, endPeriod)
    };

    public virtual IConvertible DoVisit(TokenExpressionTemporal input)
    {
        if (input.FunctionName == "DELTA" || input.FunctionName == "DELTA_TIME")
        {
            return VisitDelta(input, input.FunctionName == "DELTA_TIME");
        }
        else if (input.FunctionName == "STND")
        {
            return VisitStandardDeviation(input);
        }

        if (input.Child is TokenExpressionArray array)
        {
            throw new VisitorException($"Cannot {input.FunctionName} without time '{input.Serialize()}'");
        }

        //single time series
        if (TryGetTemporalObject(input, out var temporal, out var start, out var end))
        {
            if (TemporalFunctions.TryGetValue(input.FunctionName, out var calc))
            {
                return calc(temporal!, start, end);
            }
        }

        return input.Child.Accept(this);
    }

    private IConvertible VisitDelta(TokenExpressionTemporal input, bool useTime)
    {
        // Delta with no time period is just the difference between last two values
        if (input.TimePeriod is null)
        {
            //the child could be a temporal
            var temporal = input.Child as ITemporalObject;

            //if the child is not a temporal see if we can get a temporal from the env if the child is a variable
            if (temporal is null && VariableAccessVisitor.TryGetVariableName(input.Child, out var variableName))
            {
                temporal = temporalObjectGetter(variableName);
            }

            if (temporal is null) { return 0.0; } // Anything that isn't a timeseries has no delta

            if (useTime)
            {
                return temporal!.DeltaTimeLastAndPrevious(input.UnitOfMeasure != null && Unit.TryGetUnit(input.UnitOfMeasure.ToString()!, out Unit? unitOfMeasure) ? unitOfMeasure : null);
            }

            IConvertible result = temporal!.DeltaLastAndPrevious();
            return result;
        }
        else if (TryGetTemporalObject(input, out var temporal, out var period, out var from))
        {
            IConvertible result = temporal!.Delta(period, from);
            return result;
        }
        else
        {
            // DELTA on something that isn't a time series, let's just define it to be zero
            // eg. DELTA(3) == 0
            return 0.0;
        }
    }

    private IConvertible VisitStandardDeviation(TokenExpressionTemporal input)
    {
        if (input.Child is TokenExpressionArray array) return WillowMath.StandardDeviation(array.Children.Select(c => c.Accept(this).ToDouble(CultureInfo.InvariantCulture)));

        //single time series
        if (TryGetTemporalObject(input, out var temporal, out var period, out var from))
        {
            return temporal!.StandardDeviation(period, from);
        }

        return 0;
    }

    private bool TryGetTemporalObject(
        TokenExpressionTemporal expression,
        out ITemporalObject? temporal,
        out UnitValue startPeriod,
        out UnitValue endPeriod)
    {
        return TryGetTemporalObject(expression.Child, expression.TimePeriod, expression.TimeFrom, out temporal, out startPeriod, out endPeriod);
    }

    private bool TryGetTemporalObject(
        TokenExpression child,
        TokenExpression? timePeriodExpression,
        TokenExpression? timeFromExpression,
        out ITemporalObject? temporal,
        out UnitValue startPeriod,
        out UnitValue endPeriod)
    {
        temporal = null;
        endPeriod = default;

        //temporal is implied if a time period is provided
        if (TryGetUnit(timePeriodExpression, out startPeriod))
        {
            TryGetUnit(timeFromExpression, out endPeriod);

            //the child could be a temporal
            temporal = child.Accept(this) as ITemporalObject;

            string variableName = "";

            //if the child is not a temporal see if we can get a temporal from the env if the child is a variable
            if (temporal is null && VariableAccessVisitor.TryGetVariableName(child, out variableName))
            {
                temporal = temporalObjectGetter(variableName);
            }

            if (temporal is not null)
            {
                (bool ok, TimeSpan buffer) = temporal.IsInRange(startPeriod, endPeriod);

                if (!ok && !string.IsNullOrEmpty(variableName))
                {
                    Success = false;
                    Error = $"Variable '{variableName}' does not have sufficient data for period {buffer}";
                }

                return true;
            }
        }

        return false;
    }

    private bool TryGetUnit(TokenExpression? expression, out UnitValue result)
    {
        result = default;

        int multiplier = 1;

        if (expression is TokenExpressionUnaryMinus minus)
        {
            multiplier = -1;
            expression = minus.Child;
        }

        if (expression is not null &&
            !string.IsNullOrEmpty(expression.Unit) &&
            Unit.TryGetUnit(expression.Unit, out var unit))
        {
            double value = expression.Accept(this).ToDouble(null);

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return false;
            }

            result = new UnitValue(unit!, value * multiplier);

            return true;
        }

        return false;
    }

    public virtual IConvertible DoVisit(TokenExpressionTimer input)
    {
        return Undefined;
    }
}
