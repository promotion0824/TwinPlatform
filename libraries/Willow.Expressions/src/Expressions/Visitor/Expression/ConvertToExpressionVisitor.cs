using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Willow.Expressions.Visitor;

/// <summary>
/// Create a query expression against type TSource (e.g. ISong, Env, ...)
/// </summary>
public class ConvertToExpressionVisitor<TSource> : ITokenExpressionVisitor<Expression>
{
    private readonly Func<Expression, string, Expression> getter;

    /// <summary>
    /// Creates a new instance of the <see cref="ConvertToExpressionVisitor{TSource}"/> class
    /// </summary>
    public ConvertToExpressionVisitor(Func<Expression, string, Expression> getter)
    {
        this.getter = getter;
    }

    /// <summary>
    /// The source parameter for constructing Expressions
    /// </summary>
    public static readonly ParameterExpression parameter = Expression.Parameter(typeof(TSource), "source");

    /// Default reference for TemporalSets is UTC NOW, i.e. in the last 3 hours works for a database call
    /// But yesterday needs to be calculated in LOCAL TIME and then bumped back to Utc time
    ///
    /// Should we instead have a TimeZoneOffset (TimeSpan) here?
    /// For relative temporal expressions it's not used: 3 hours ago is reference - 3 hours
    /// For Yesterday, Last week, ... the Utc database time needs to have the TimeZone offset added to it
    /// before comparing for the DatePart etc.
    /// DATEADD ("MINUTES" , tzOffset , date )
    ///
    /// Pass TimeSpan.Zero if the database stored LOCAL datetime values
    /// otherwise pass TimeZoneInfo....
    ///
    /// WHAT IF WE ASSUME ALL DATABASES USE DATETIMEOFFSET
    ///

    private static readonly PropertyInfo ItemProperty = typeof(DataRow)
        .GetProperty("Item",
        bindingAttr: BindingFlags.Public | BindingFlags.Instance,
        binder: null,
        returnType: typeof(object),
        types: new Type[] { typeof(string) },       // Get the indexer that takes a string argument
        modifiers: null)!;

    //            .GetProperties().Where(p => p.Name == "Item").Skip(1).First();

    /// <summary>
    /// A Getter for a DataRow that can be used in the constructor
    /// </summary>
    public static Expression GetterForDataRow(Expression instance, string variableName, DataColumnCollection columns)
    {
        var col = columns[variableName]!;        // column used for data type
        var type = col.DataType;
        var access = Expression.MakeIndex(instance, ItemProperty, new[] { Expression.Constant(variableName) });
        var converted = Expression.Convert(access, type);

        // private static Expression<Func<object, bool>> IsDbNull = (x) => Convert.IsDBNull(x);
        var dbnullCheckMethod = typeof(System.Convert).GetMethod("IsDBNull")!;
        var checkForNull = Expression.Call(dbnullCheckMethod, access);

        // if the expression is null return the default for that type otherwise return the value
        // so DBNull for an int becomes zero but DbNull for an int? becomes null
        var ternaryExpression = Expression.Condition(checkForNull, Expression.Default(type), converted);
        return ternaryExpression;
    }

    // This is effectively what the expression code above is doing:
    //
    //private static T ConvertFromDBVal<T>(object obj)
    //{
    //    if (obj is null || obj == DBNull.Value)
    //    {
    //        return default(T); // returns the default value for the type
    //    }
    //    else
    //    {
    //        return (T)obj;
    //    }
    //}

    /// <summary>
    /// A Getter for an Object that can be used in the constructor
    /// </summary>
    public static Expression GetterForObjectUsingReflection(Expression instance, string variableName)
    {
        // e.g. .On needs to be a access to the On property of a TSource
        var propertyInfo = typeof(TSource).GetInterfaces()
            .Concat(Enumerable.Repeat(typeof(TSource), 1))
            .Select(i => i.GetProperty(variableName, BindingFlags.Instance | BindingFlags.Public))
            .FirstOrDefault(x => x != null);

        if (propertyInfo is null)
            return ExpressionUndefined.Instance;
        //    throw new NullReferenceException($"{input.VariableName} is not defined for type {typeof(TSource).Name}");

        var access = Expression.Property(instance, propertyInfo);
        // The top-level Source parameter is added as a lambda in the Visit() call not here
        return access;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression Visit(TokenExpression source)
    {
        if (source is null) return ExpressionUndefined.Instance; // can't be null

        var expr = source.Accept(this);

        if (expr is not LambdaExpression lambdaExpr)
        {
            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                return Expression.Lambda(expr);
            }

            if (expr.NodeType == ExpressionType.Constant)
            {
                return expr;
            }

            // It has no parameters: coerce it to a Func<TSource,...>
            return Expression.Lambda(expr, parameter);
        }

        if (lambdaExpr.Parameters.Any())
        {
            if (lambdaExpr.Parameters.Count > 1)
                return new ExpressionUndefined();
            //throw new Exception(
            //    $"Expression expects multiple parameters: {string.Join(",", lambdaExpr.Parameters.Select(x => x.Type.Name + " " + x.Name))}");

            if (lambdaExpr.Parameters[0].Type != parameter.Type)
                return new ExpressionUndefined();
            //throw new Exception(
            //        $"Expression expects something other than {typeof (TSource).Name}: {string.Join(",", lambdaExpr.Parameters.Select(x => x.Type.Name + " " + x.Name))}");

            return lambdaExpr;
        }
        else
        {
            // coerce it to a Func<TSource,...> even though it doesn't care what the source is
            return Expression.Lambda(lambdaExpr.Body, parameter);
        }
    }

    /// <summary>
    /// Create a Lambda (or not) Expression from a create function
    /// </summary>
    protected virtual Expression CreateCombo(TokenExpressionBinary input,
        Func<Expression, Expression, Expression> create,
        Func<Type, Type, bool> allowedTypesFunc)
    {
        return CreateCombo(input.Left, input.Right, create, allowedTypesFunc);
    }

    /// <summary>
    /// Create a Lambda (or not) Expression from a create function
    /// </summary>
    protected virtual Expression CreateCombo(TokenExpression leftHalf,
        TokenExpression rightHalf,
        Func<Expression, Expression, Expression> create,
        Func<Type, Type, bool> allowedTypesFunc)
    {
        var left = leftHalf.Accept(this);
        var right = rightHalf.Accept(this);

        // left and right might be lambda functions or they might be constants
        var leftType = (left is LambdaExpression lambdaLeft) ? lambdaLeft.ReturnType : left.Type;
        var rightType = (right is LambdaExpression lambdaRight) ? lambdaRight.ReturnType : right.Type;

        // If either side is a Nullable of a value type need to check it's not null and get the value
        // TODO: Not null check missing
        ConvertNullable<DateTime>(ref leftType, ref rightType, ref left, ref right);
        ConvertNullable<TimeSpan>(ref leftType, ref rightType, ref left, ref right);
        ConvertNullable<int>(ref leftType, ref rightType, ref left, ref right);
        ConvertNullable<double>(ref leftType, ref rightType, ref left, ref right);
        ConvertNullable<decimal>(ref leftType, ref rightType, ref left, ref right);
        ConvertNullable<long>(ref leftType, ref rightType, ref left, ref right);

        // ...

        if (!allowedTypesFunc(leftType, rightType)) return ExpressionUndefined.Instance;

        // Do we need to do any conversions
        if (IsNumeric(leftType) && IsNumeric(rightType) && leftType != rightType)
        {
            if (leftType == typeof(double)) right = Expression.Convert(right, leftType);
            else if (rightType == typeof(double)) left = Expression.Convert(left, rightType);
            else if (leftType == typeof(float)) right = Expression.Convert(right, leftType);
            else if (rightType == typeof(float)) left = Expression.Convert(left, rightType);
            else if (leftType == typeof(decimal)) right = Expression.Convert(right, leftType);
            else if (rightType == typeof(decimal)) left = Expression.Convert(left, rightType);
            else if (leftType == typeof(long)) right = Expression.Convert(right, leftType);
            else if (rightType == typeof(long)) left = Expression.Convert(left, rightType);
            else throw new Exception($"Could not find a conversion for {leftType.Name} and {rightType.Name}");
        }

        // need to rewrite to the same parameter
        if (left is LambdaExpression lambdaLeft2 && right is LambdaExpression lambdaRight2)
        {
            var parameter = lambdaLeft2.Parameters.First();
            right = lambdaRight2.Apply(parameter);
        }

        return LambdaOrPlain(left, right, create(left, right));
    }

    /// <summary>
    /// Coerce a nullable type to its non-nullable equivalent for comparisons and math operations
    /// </summary>
    /// <remarks>
    /// Expression will throw if value was null
    /// </remarks>
    private void ConvertNullable<T>(ref Type leftType, ref Type rightType, ref Expression left, ref Expression right)
        where T : struct
    {
        if (leftType == typeof(Nullable<T>))
        {
            left = Expression.Convert(left, typeof(T));
            leftType = typeof(T);
        }

        if (rightType == typeof(Nullable<T>))
        {
            right = Expression.Convert(right, typeof(T));
            rightType = typeof(T);
        }
    }

    /// <summary>
    /// Create a Lambda (or not) Expression from a create function
    /// </summary>
    protected virtual Expression CreateCombo(TokenExpressionNary input,
        Func<Expression, Expression, Expression> create,
        Func<Type, bool> allowedTypesFunc)
    {
        var children = input.Children.Select(c => (c.Accept(this))).ToList();
        if (!children.All(c => allowedTypesFunc(c.Type))) return ExpressionUndefined.Instance;
        var combined = children.Skip(1).Aggregate(children.First(), create);
        return LambdaOrPlain(children, combined);
    }

    // ReSharper disable once StaticMemberInGenericType

    /// <summary>
    /// A True Expression
    /// </summary>
    protected static readonly Expression True = Expression.Constant(true);

    /// <summary>
    /// A False Expression
    /// </summary>
    protected static readonly Expression False = Expression.Constant(false);

    // ReSharper disable once StaticMemberInGenericType

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        var child = input.Child.Accept(this);
        if (child.Type != typeof(DateTime))
        {
            return ExpressionUndefined.Instance;
        }

        // rewrite using child's parameters
        Expression<Func<DateTime, DateTime>> convert = d => TimeZoneInfo.ConvertTimeFromUtc(d, TimeZoneInfo.Local);
        return convert.Apply(child);        // This should become a simple Expression
    }

    private static bool IsNumeric(Type t) => t == typeof(int) || t == typeof(long) || t == typeof(double) || t == typeof(decimal);

    private static bool IsString(Type t) => t == typeof(string);

    private static bool IsBoolean(Type t) => t == typeof(bool);

    private static bool IsDateTime(Type t) => t == typeof(DateTime) || t == typeof(DateTimeOffset);

    private static bool IsTimeSpan(Type t) => t == typeof(TimeSpan);

    private static bool BothBoolean(Type a, Type b) => IsBoolean(a) && IsBoolean(b);

    private static bool BothNumeric(Type a, Type b) => IsNumeric(a) && IsNumeric(b);

    private static bool BothString(Type a, Type b) => IsString(a) && IsString(b);

    private static bool CanCompare(Type a, Type b) => BothNumeric(a, b) || BothString(a, b);

    private static bool CanAdd(Type a, Type b) => (IsNumeric(a) && IsNumeric(b)) ||
        ((IsDateTime(a) && IsTimeSpan(b)));

    private static bool CanSubtract(Type a, Type b) => ((IsNumeric(a) && IsNumeric(b))) ||
        ((IsDateTime(a) && IsDateTime(b))) || ((IsDateTime(a) && IsTimeSpan(b)));

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionAdd input)
    {
        return CreateCombo(input, Expression.Add, IsNumeric);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionMultiply input)
    {
        return CreateCombo(input, Expression.Multiply, IsNumeric);
    }

    /// <summary>
    /// Converts match (variable, temporal expression) into an expression returning a bool
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionMatches input)
    {
        var left = input.Left.Accept(this);
        var right = input.Right.Accept(this);

        if (left == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;
        if (right == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;

        // right could be a TemporalSet, a Set, a Range, a single Double Value, ...
        // If it becomes a LambdaExpression, apply that to the left value
        if (right is LambdaExpression lambdaRight)
        {
            // Nullable<DateTime> compared to DateTime needs an extra conversion
            if (left.Type.IsGenericType &&
                left.Type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                (!lambdaRight.Parameters.First().Type.IsGenericType ||
                lambdaRight.Parameters.First().Type.GetGenericTypeDefinition() != typeof(Nullable<>)))
            {
                var nullableType = left.Type.GetGenericArguments().First();
                // Need to rewrite the LHS from Nullable<DateTime> to <DateTime> and assume it's not null

                var arg = lambdaRight.Parameters.First();

                Expression checkNotNull = Expression.Call(left,
                    typeof(Nullable<>).MakeGenericType(nullableType).GetMethod("get_HasValue")!);

                var leftConverted = Expression.Convert(left, nullableType);

                var and = Expression.AndAlso(checkNotNull, lambdaRight.Apply(leftConverted));
                return and;
            }

            // When the field is a DateTimeOffset but we converted the expression to a DateTime Func
            if (left.Type == typeof(DateTimeOffset) &&
                lambdaRight.Parameters.First().Type == typeof(DateTime))
            {
                left = Expression.PropertyOrField(left, nameof(DateTimeOffset.LocalDateTime));
            }

            return lambdaRight.Apply(left);
        }
        else if (input.Right is TokenDouble)
        {
            // Just use equality
            return CreateCombo(input, Expression.Equal, CanCompare);
        }
        else if (input.Right is TokenExpressionConstantColor color)
        {
            // TODO: Expand colors to include close colors (like vs match)
            var c = TokenExpressionConstantColor.GetClosestNamedColor(color.R, color.G, color.B);
            var cname = new TokenExpressionConstantString(c.Text);
            var match = new TokenExpressionMatches(input.Left, cname);
            // Just use equality
            return CreateCombo(match, Expression.Equal, CanCompare);
        }
        else
        {
            Trace.TraceError($"Match could not be converted to expression: {input.Serialize()}");
            throw new Exception($"Matches can only be used with a TemporalSet, a Range, a Set or a Double, not a {input.Serialize()} in ConvertToExpression");
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionAnd input)
    {
        return CreateCombo(input, Expression.AndAlso, IsBoolean);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionOr input)
    {
        return CreateCombo(input, Expression.OrElse, IsBoolean);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionTernary input)
    {
        var condition = input.Conditional.Accept(this);
        if (!IsBoolean(condition.Type)) return ExpressionUndefined.Instance;
        return Expression.Condition(condition, input.Truth.Accept(this), input.Falsehood.Accept(this));
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionTuple input)
    {
        var children = input.Children.Select(c => c.Accept(this)).ToArray();
        var childTypes = children.Select(c => c.Type).ToArray();

        var tuple = Expression.Call(typeof(Tuple), "Create", childTypes, children);
        return tuple;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionConstantNull input)
    {
        return Expression.Constant(input.Value);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionConstantDateTime input)
    {
        return Expression.Constant(input.Value);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionConstantString input)
    {
        return Expression.Constant(input.Value);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionArray input)
    {
        var bodies = input.Children.Select(Expression.Constant).ToList();
        var type = bodies.First().Type;     // Assume compatible types
        var carray = Expression.NewArrayInit(type, bodies);
        return carray;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionDivide input)
    {
        return CreateCombo(input, Expression.Divide, BothNumeric);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenDouble input)
    {
        return Expression.Constant(input.Value);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionConstant input)
    {
        // Never called since the specific ones are implemented
        // Could make the others call through here to may overrides easier
        throw new NotImplementedException("Specific types of constant have their own calls, this should never happen");
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionConstantBool input)
    {
        return Expression.Constant(input.Value);
    }

    /// <summary>
    /// Visit
    /// </summary>
    /// <remarks>
    /// Meaningless?
    /// </remarks>
    public virtual Expression DoVisit(TokenExpressionConstantColor input)
    {
        return Expression.Constant(input.Value);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionIs input)
    {
        return DoVisit(new TokenExpressionEquals(input.Left, input.Right));
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionEquals input)
    {
        // Handle a Nullable LHS compared to null using HasValue instead
        if (input.Left.Type.IsGenericType && input.Left.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var left = input.Left.Accept(this);
            if (input.Right.Equals(TokenExpression.Null))
            {
                return Expression.Not(Expression.Property(left, nameof(Nullable<bool>.HasValue)));
            }
            else
            {
                return Expression.AndAlso(
                    Expression.Property(left, nameof(Nullable<bool>.HasValue)),
                    Expression.Equal(
                        Expression.Property(left, nameof(Nullable<bool>.Value)),
                        input.Right.Accept(this)));
            }
        }

        if (input.Left.Type.IsValueType && input.Right.Equals(TokenExpression.Null)) return Expression.Constant(false);      // value types are never null
        if (input.Right.Type.IsValueType && input.Left.Equals(TokenExpression.Null)) return Expression.Constant(false);      // value types are never null

        return CreateCombo(input, Expression.Equal, CanCompare);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionGreater input)
    {
        //if (LeftIsNullable(input, out TokenExpression replacement)) return replacement.Accept(this);
        return CreateCombo(input, Expression.GreaterThan, CanCompare);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionGreaterOrEqual input)
    {
        //if (LeftIsNullable(input, out TokenExpression replacement)) return replacement.Accept(this);
        return CreateCombo(input, Expression.GreaterThanOrEqual, CanCompare);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionNot input)
    {
        var child = input.Child.Accept(this);
        if (child.Type != typeof(bool)) return ExpressionUndefined.Instance;
        return LambdaOrPlain(child, Expression.Not);
    }

    /// <summary>
    /// If the input is a lambda expression, preserve the args in a new labdaexpression,
    /// otherwise just a body
    /// (a) => fn(a) becomes a => make(fn(a)) but 3 becomes make(3)
    /// </summary>
    private static Expression LambdaOrPlain(Expression child, Func<Expression, Expression> make)
    {
        if (child is LambdaExpression lambda)
        {
            return Expression.Lambda(make(lambda.Body), lambda.Parameters);
        }
        else
        {
            return make(child);
        }
    }

    /// <summary>
    /// If the left or right is a lambda expression, preserve the args
    /// in a new lambdaexpression, otherwise just a body
    /// (a) => fn(a) becomes a => make(fn(a)) but 3 becomes make(3)
    /// </summary>
    private static Expression LambdaOrPlain(Expression left,
        Expression right,
        Expression combined)
    {
        return LambdaOrPlain(new[] { left, right }, combined);
    }

    /// <summary>
    /// If any of the child expressions is a lambda expression
    /// preserve the args in a new lambdaexpression, otherwise just a body
    /// </summary>
    private static Expression LambdaOrPlain(IEnumerable<Expression> children,
        Expression body)
    {
        var parameters = children.OfType<LambdaExpression>()
            .SelectMany(c => c.Parameters)
            .Distinct().ToList();

        return parameters.Any() ? Expression.Lambda(body, parameters) : body;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionUnaryMinus input)
    {
        var lambdaExpression = (LambdaExpression)input.Child.Accept(this);
        if (!IsNumeric(lambdaExpression.ReturnType)) return ExpressionUndefined.Instance;
        var body = Expression.Negate(lambdaExpression.Body);
        return Expression.Lambda(body, lambdaExpression.Parameters);
    }

    ///// <summary>
    ///// If the left expression is nullable, add a check to make sure it has a value
    ///// </summary>
    //private bool LeftIsNullable(TokenExpressionBinary input, out TokenExpression wrapped)
    //{
    //    if (input.Left.Type.IsGenericType && input.Left.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
    //    {
    //        CreateCombo(input, Expression.AndAlso, IsBoolean);
    //        wrapped = new TokenExpressionAnd(
    //            new TokenExpressionFunctionCall(nameof(Nullable<double>.HasValue), typeof(bool), input.Left),
    //            input);
    //        return true;
    //    }
    //    else
    //    {
    //        wrapped = input;
    //        return false;
    //    }
    //}

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionLess input)
    {
        //if (LeftIsNullable(input, out TokenExpression replacement)) return replacement.Accept(this);
        return CreateCombo(input, Expression.LessThan, BothNumeric);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionLessOrEqual input)
    {
        //if (LeftIsNullable(input, out TokenExpression replacement)) return replacement.Accept(this);
        return CreateCombo(input, Expression.LessThanOrEqual, BothNumeric);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionPropertyAccess input)
    {
        var inner = input.Child.Accept(this);

        if (inner is ConstantExpression ce)
        {
            string? textValue = ce.Value as string;

            if (string.IsNullOrEmpty(textValue) && ce.Value is EnvValue envValue)
            {
                textValue = envValue.TextValue;
            }

            if (!string.IsNullOrEmpty(textValue))
            {
                var propertyBag = ToDictionary(JsonConvert.DeserializeObject<JObject>(textValue)!);

                if (propertyBag.TryGetValue(input.PropertyName, out var value))
                {
                    return Expression.Constant(value);
                }
            }

            if (ce.Value is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue(input.PropertyName, out var value))
                {
                    return Expression.Constant(value);
                }
            }
        }

        return LambdaOrPlain(inner, (c) => Expression.Property(c, input.PropertyName.ToString()));
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionVariableAccess input)
    {
        var fetch = getter(parameter, input.VariableName);
        return fetch;
        //            return Expression.Convert(fetch, input.Type);
    }

    private static readonly Dictionary<string, MethodInfo> BuiltInSingleParameterStaticFunctions = new Dictionary<string, MethodInfo>
    {
        ["ABS"] = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) })!,
        ["ACOS"] = typeof(Math).GetMethod(nameof(Math.Acos), new[] { typeof(double) })!,
        ["ASIN"] = typeof(Math).GetMethod(nameof(Math.Asin), new[] { typeof(double) })!,
        ["ATAN"] = typeof(Math).GetMethod(nameof(Math.Atan), new[] { typeof(double) })!,
        ["ATAN2"] = typeof(Math).GetMethod(nameof(Math.Atan2), new[] { typeof(double) })!,
        ["CEILING"] = typeof(Math).GetMethod(nameof(Math.Ceiling), new[] { typeof(double) })!,
        ["COS"] = typeof(Math).GetMethod(nameof(Math.Cos), new[] { typeof(double) })!,
        // COT, DEGREES, EXP, PI, RADIANS, RAND, SQUARE
        ["FLOOR"] = typeof(Math).GetMethod(nameof(Math.Floor), new[] { typeof(double) })!,
        ["LOG"] = typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) })!,
        ["LOG10"] = typeof(Math).GetMethod(nameof(Math.Log10), new[] { typeof(double) })!,
        ["ROUND"] = typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) })!,
        ["SIGN"] = typeof(Math).GetMethod(nameof(Math.Sign), new[] { typeof(double) })!,
        ["SIN"] = typeof(Math).GetMethod(nameof(Math.Sin), new[] { typeof(double) })!,
        ["SQRT"] = typeof(Math).GetMethod(nameof(Math.Sqrt), new[] { typeof(double) })!,
        ["TAN"] = typeof(Math).GetMethod(nameof(Math.Tan), new[] { typeof(double) })!,
    };

    /// <summary>
    /// Add a single parameter function (calls method on source)
    /// </summary>
    public void AddSingleParameterFunction(string name, MethodInfo methodInfo)
    {
        BuiltInSingleParameterStaticFunctions[name] = methodInfo;
    }

    private static readonly Dictionary<string, MethodInfo> BuiltInSingleParameterStringFunctions = new Dictionary<string, MethodInfo>
    {
        ["TOUPPER"] = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!,
        ["TOUPPERINVARIANT"] = typeof(string).GetMethod(nameof(string.ToUpperInvariant), Type.EmptyTypes)!,
        ["TOLOWER"] = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!,
        ["TOLOWERINVARIANT"] = typeof(string).GetMethod(nameof(string.ToLowerInvariant), Type.EmptyTypes)!,
        ["TRIM"] = typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)!,
        ["PADRIGHT"] = typeof(string).GetMethod(nameof(string.PadRight), new[] { typeof(int) })!,
    };

    private static readonly Dictionary<string, MethodInfo> BuiltInTwoParameterFunctions = new Dictionary<string, MethodInfo>
    {
        ["POW"] = typeof(Math).GetMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!,
    };

    private static readonly Dictionary<string, MethodInfo> BuiltInThreeParameterFunctions = new Dictionary<string, MethodInfo>
    {
        ["DEADBAND"] = typeof(WillowMath).GetMethod(nameof(WillowMath.Deadband), new[] { typeof(double), typeof(double), typeof(double) })!,
    };

    private static readonly Dictionary<string, MethodInfo> BuiltInTwoParameterStringFunctions = new Dictionary<string, MethodInfo>
    {
        ["STARTSWITH"] = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!,
        ["ENDSWITH"] = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!,
        ["CONTAINS"] = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
        // HACK, see below
        ["LIKE"] = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) })!,
    };

    private static readonly Dictionary<string, MethodInfo> BuiltInTwoParameterBoolFunctions = new Dictionary<string, MethodInfo>
    {
    };

    /// <summary>
    /// Add a two parameter bool function (calls method on source and passes other arguments)
    /// </summary>
    public void AddTwoParameterBoolFunction(string name, MethodInfo methodInfo)
    {
        BuiltInTwoParameterBoolFunctions[name] = methodInfo;
    }

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
    // INDEXOF

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionFunctionCall input)
    {
        var children = input.Children.Select(c => c.Accept(this)).ToList();

        var childBodies = children.Select(x => (x as LambdaExpression)?.Body ?? x).ToList();
        var childParameters = children.OfType<LambdaExpression>().SelectMany(x => x.Parameters).Distinct().ToList();

        // SIN, COS, ... and other SingleParameter math functions

        foreach (var singleParameterFunction in BuiltInSingleParameterStaticFunctions)
        {
            if (input.FunctionName.Equals(singleParameterFunction.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                var child = children.First();
                if (child == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;
                if (!IsNumeric(child.Type)) return ExpressionUndefined.Instance;
                return LambdaOrPlain(child, c => Expression.Call(singleParameterFunction.Value, c));
            }
        }

        foreach (var singleParameterFunction in BuiltInSingleParameterStringFunctions)
        {
            if (input.FunctionName.Equals(singleParameterFunction.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                var child = children.First();
                var methodInfo = singleParameterFunction.Value;
                // Convert remaining parameters
                var convertedParameters =
                    childBodies.Skip(1)
                        .Zip(methodInfo.GetParameters()
                            .Select(p => p.ParameterType),
                            (p, q) => p.Type == q ? p : Expression.Convert(p, q));

                if (child == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;
                // convert numerics to string?? if (IsNumeric(lambdaExpression.ReturnType)) return ExpressionUndefined.Instance;
                return LambdaOrPlain(child,
                    c => Expression.Call(c, singleParameterFunction.Value, convertedParameters));

                //Need to take remaining children and add them to the Expression.Call
            }
        }

        // TODO: Built-in POW function

        foreach (var twoParameterFunction in BuiltInTwoParameterStringFunctions)
        {
            if (input.FunctionName.Equals(twoParameterFunction.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                var child = children.First();
                var methodInfo = twoParameterFunction.Value;
                // Convert remaining parameters
                var convertedParameters =
                    childBodies.Skip(1)
                        .Zip(methodInfo.GetParameters()
                            .Select(p => p.ParameterType),
                            (p, q) => p.Type == q ? p : Expression.Convert(p, q));

                if (child == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;
                // convert numerics to string?? if (IsNumeric(lambdaExpression.ReturnType)) return ExpressionUndefined.Instance;

                // HACK
                if (input.FunctionName.Equals("LIKE", StringComparison.InvariantCultureIgnoreCase))
                {
                    convertedParameters = convertedParameters.Concat(new[]
                    {
                            Expression.Constant(StringComparison.InvariantCultureIgnoreCase)
                    });
                }

                return LambdaOrPlain(child,
                    c => Expression.Call(c, twoParameterFunction.Value, convertedParameters));

                //Need to take remaining children and add them to the Expression.Call
            }
        }

        foreach (var twoParameterFunction in BuiltInTwoParameterBoolFunctions)
        {
            if (input.FunctionName.Equals(twoParameterFunction.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                var child = children.First();
                var methodInfo = twoParameterFunction.Value;
                // Convert remaining parameters
                var convertedParameters =
                    childBodies.Skip(1)
                        .Zip(methodInfo.GetParameters()
                            .Select(p => p.ParameterType),
                            (p, q) => p.Type == q ? p : Expression.Convert(p, q));

                if (child == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;
                // convert numerics to string?? if (IsNumeric(lambdaExpression.ReturnType)) return ExpressionUndefined.Instance;

                return LambdaOrPlain(child,
                    c => Expression.Call(c, twoParameterFunction.Value, convertedParameters));

                //Need to take remaining children and add them to the Expression.Call
            }
        }

        foreach (var threeParameterFunction in BuiltInThreeParameterFunctions)
        {
            if (input.FunctionName.Equals(threeParameterFunction.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                var child = children.First();
                var methodInfo = threeParameterFunction.Value;
                // Convert remaining parameters
                var convertedParameters =
                    childBodies.Skip(1)
                        .Zip(methodInfo.GetParameters()
                            .Select(p => p.ParameterType),
                            (p, q) => p.Type == q ? p : Expression.Convert(p, q));

                if (child == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;
                // convert numerics to string?? if (IsNumeric(lambdaExpression.ReturnType)) return ExpressionUndefined.Instance;

                return LambdaOrPlain(child,
                    c => Expression.Call(c, threeParameterFunction.Value, convertedParameters));

                //Need to take remaining children and add them to the Expression.Call
            }
        }

        switch (input.FunctionName)
        {
            case "average":
                // This is Average for params double[]...
                // What about actual array values in TokenExpression?
                // e.g. a linked property: product.Dimensions (double[])

                var carray = Expression.NewArrayInit(typeof(double), childBodies);

                //var converted = Expression.Convert(bodies2, typeof (IEnumerable<double>));
                var method = typeof(Enumerable).GetMethod(nameof(Enumerable.Average), new[] { typeof(IEnumerable<double>) })!;
                var bodyAverage = Expression.Call(method, carray);
                //return Expression.Lambda(bodyAverage, childParameters);
                return LambdaOrPlain(children, bodyAverage);

            default:
                {
                    // Get the function to call from the TSource passed in
                    // TBD: Should this be some other environment?

                    var methodInfo = typeof(TSource).GetInterfaces()
                        .Concat(Enumerable.Repeat(typeof(TSource), 1))
                        .Select(i => i.GetMethod(input.FunctionName, BindingFlags.Instance | BindingFlags.Public))
                        .FirstOrDefault(x => x != null);

                    if (methodInfo is null)
                    {
                        var debug = typeof(TSource).GetInterfaces().Concat(Enumerable.Repeat(typeof(TSource), 1)).ToList();
                        var debug2 = debug.SelectMany(t => t.GetMethods());
                        return ExpressionUndefined.Instance;
                    }

                    var child = children.First();

                    // Convert remaining parameters
                    var convertedParameters =
                        childBodies.Skip(1)
                            .Zip(methodInfo.GetParameters()
                                .Select(p => p.ParameterType),
                                (p, q) => p.Type == q ? p : Expression.Convert(p, q));

                    if (child == ExpressionUndefined.Instance) return ExpressionUndefined.Instance;
                    // convert numerics to string?? if (IsNumeric(lambdaExpression.ReturnType)) return ExpressionUndefined.Instance;

                    return LambdaOrPlain(child,
                        c => Expression.Call(c, methodInfo, convertedParameters));

                    // Types need to come from the (fn) parameter types
                    //var p0 = Expression.Parameter(typeof (double), "p0");

                    // TODO: Some x's may not need any parameters, some might
                    // TODO: x could be a constant expression, no need to invoke that!
                }
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionNotEquals input)
    {
        // Handle a Nullable LHS compared to null using HasValue instead
        if (input.Left.Type.IsGenericType && input.Left.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var left = input.Left.Accept(this);
            if (input.Right.Equals(TokenExpression.Null))
            {
                return Expression.Property(left, nameof(Nullable<bool>.HasValue));
            }
            else
            {
                return Expression.AndAlso(
                    Expression.Property(left, nameof(Nullable<bool>.HasValue)),
                    Expression.NotEqual(
                        Expression.Property(left, nameof(Nullable<bool>.Value)),
                        input.Right.Accept(this)));
            }
        }

        if (input.Left.Type.IsValueType && input.Right.Equals(TokenExpression.Null)) return Expression.Constant(true);      // value types are never null
        if (input.Right.Type.IsValueType && input.Left.Equals(TokenExpression.Null)) return Expression.Constant(true);      // value types are never null

        return CreateCombo(input, Expression.NotEqual, CanCompare);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionPower input)
    {
        return CreateCombo(input, Expression.Power, BothNumeric);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public virtual Expression DoVisit(TokenExpressionSubtract input)
    {
        return CreateCombo(input, Expression.Subtract, CanSubtract);
    }

    public virtual Expression DoVisit(TokenExpressionIdentity input)
    {
        return input.Child.Accept(this);
    }

    public virtual Expression DoVisit(TokenExpressionFailed input)
    {
        throw new NotImplementedException("Cannot convert a failed expression");
    }

    public virtual Expression DoVisit(TokenExpressionIntersection input)
    {
        throw new NotImplementedException("Need to visit sides, and do a Range or Set union on them");
    }

    public virtual Expression DoVisit(TokenExpressionSetUnion input)
    {
        throw new NotImplementedException("Need to visit sides, and do a Range or Set union on them");
    }

    /// <summary>
    /// Create a range test lambda expression
    /// </summary>
    protected Expression<Func<DateTime, DateTime, DateTime, bool>> InRange()
    {
        Expression<Func<DateTime, DateTime, DateTime, bool>> expr = (start, end, d) => start <= d && d < end;
        return expr;
    }

    /// <summary>
    /// Create a conjunction of two lambda expressions
    /// </summary>
    protected LambdaExpression CreateConjunction(IEnumerable<Expression<Func<DateTime, bool>>> children, Func<Expression, Expression, Expression> create)
    {
        var dateTimeParameter = Expression.Parameter(typeof(DateTime), "d");
        var bodies = children.Select(x => ChangeFirstParameter(x, dateTimeParameter))
            .Select(x => x.Body)
            .ToList();
        Expression current = bodies.First();
        current = bodies.Skip(1).Aggregate(current, create);
        return Expression.Lambda(current, dateTimeParameter);
    }

    private static readonly Expression<Func<DateTime, bool>> NoOp = p => false;

    private Expression<Func<DateTime, bool>> GenerateFragment(int h, int m, bool greaterThan)
    {
        if (m == 0)
        {
            if (greaterThan)
            {
                if (h == 0)
                    return NoOp;                                      // Greater than midnight is always true
                else if (h == 23)
                    return p => p.Hour == h;      // Nothing > 23
                else
                    return p => p.Hour >= h;
            }
            else
            {
                if (h == 0)
                    return NoOp;                                      // No-op (nothing in this range)
                else if (h == 1)
                    return p => p.Hour == 0;        // Nothing <1 but zero
                else
                    return p => p.Hour < h;
            }
        }

        if (greaterThan)
        {
            if (h == 23)
            { return p => p.Hour == h && p.Minute >= m; }
            else
            { return p => p.Hour > h || (p.Hour == h && p.Minute >= m); }
        }
        else
        {
            if (h == 0)
            { return p => p.Hour == h && p.Minute < m; }
            else
            { return p => p.Hour < h || (p.Hour == h && p.Minute < m); }
        }
    }

    /// <summary>
    /// Change the first parameter of a lambda expression
    /// </summary>
    protected LambdaExpression ChangeFirstParameter(LambdaExpression lambda, ParameterExpression substitute)
    {
        if (!lambda.Parameters.Any()) return lambda;
        var swap = new ExpressionSubstitute(lambda.Parameters[0], substitute);
        var parameters = Enumerable.Repeat(substitute, 1).Concat(lambda.Parameters.Skip(1)).ToArray();
        return Expression.Lambda(swap.Visit(lambda.Body), parameters);
    }

    public virtual Expression DoVisit(TokenExpressionSum input)
    {
        var child = input.Child.Accept(this);
        var childType = new[] { child.Type };       // remove the IEnumerable<> on it ??? TODO
        var result = Expression.Call(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.Sum), childType, child);
        return result;
    }

    private static MethodInfo averageMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.Average), new[] { type })!;

    private static MethodInfo allMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.All), new[] { type })!;

    private static MethodInfo anyMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.Any), new[] { type })!;

    private static MethodInfo countMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.Count), new[] { type })!;

    private static MethodInfo sumMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.Sum), new[] { type })!;

    private static MethodInfo minMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.Min), new[] { type })!;

    private static MethodInfo maxMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.Max), new[] { type })!;

    private static MethodInfo firstMethodInfo(Type type) => typeof(Enumerable).GetMethod(nameof(Enumerable.First), new[] { type })!;

    public Expression DoVisit(TokenExpressionCount input)
    {
        var child = input.Child.Accept(this);
        var childType = typeof(IEnumerable<>).MakeGenericType(new[] { child.Type });
        var result = Expression.Call(countMethodInfo(childType), child);
        return result;
    }

    public Expression DoVisit(TokenExpressionAverage input)
    {
        var child = input.Child.Accept(this);
        var childType = typeof(IEnumerable<>).MakeGenericType(new[] { child.Type });
        var result = Expression.Call(averageMethodInfo(childType), child);
        return result;
    }

    public Expression DoVisit(TokenExpressionAny input)
    {
        var child = input.Child.Accept(this);
        var childType = typeof(IEnumerable<>).MakeGenericType(new[] { child.Type });
        var result = Expression.Call(anyMethodInfo(childType), child);
        return result;
    }

    public Expression DoVisit(TokenExpressionAll input)
    {
        var child = input.Child.Accept(this);
        var childType = typeof(IEnumerable<>).MakeGenericType(new[] { child.Type });
        var result = Expression.Call(allMethodInfo(childType), child);
        return result;
    }

    public Expression DoVisit(TokenExpressionEach input)
    {
        throw new Exception("Not implemented, did you forget to Bind?");
    }

    public Expression DoVisit(TokenExpressionMin input)
    {
        var child = input.Child.Accept(this);
        var childType = typeof(IEnumerable<>).MakeGenericType(new[] { child.Type });
        var result = Expression.Call(minMethodInfo(childType), child);
        return result;
    }

    public Expression DoVisit(TokenExpressionMax input)
    {
        var child = input.Child.Accept(this);
        var childType = typeof(IEnumerable<>).MakeGenericType(new[] { child.Type });
        var result = Expression.Call(maxMethodInfo(childType), child);
        return result;
    }

    public Expression DoVisit(TokenExpressionFirst input)
    {
        var child = input.Child.Accept(this);
        var childType = typeof(IEnumerable<>).MakeGenericType(new[] { child.Type });
        var result = Expression.Call(firstMethodInfo(childType), child);
        return result;
    }

    public Expression DoVisit(TokenExpressionParameter input)
    {
        return Expression.Parameter(input.Type, input.Name);
    }

    public Expression DoVisit(TokenExpressionWrapped input)
    {
        return Expression.Constant(input.BareObject, input.Type);
    }

    public Expression DoVisit(TokenExpressionTemporal input)
    {
        var child = input.Accept(this);

        var children = new Expression[] { input.TimePeriod?.Accept(this) ?? Expression.Constant(TimeSpan.Zero) };
        var childTypes = new Type[] { typeof(TimeSpan) };

        var delta = Expression.Call(child, input.FunctionName, childTypes, children);
        return delta;
    }

    public Expression DoVisit(TokenExpressionTimer input)
    {
        throw new NotImplementedException();
    }

    private static IDictionary<string, object> ToDictionary(JObject @object)
    {
        var result = @object.ToObject<Dictionary<string, object>>()!;

        var objectKeys = (from r in result
                          let key = r.Key
                          let value = r.Value
                          where value.GetType() == typeof(JObject)
                          select key).ToList();

        var arrayKeys = (from r in result
                         let key = r.Key
                         let value = r.Value
                         where value.GetType() == typeof(JArray)
                         select key).ToList();

        arrayKeys.ForEach(key => result[key] = ((JArray)result[key]).Values().Select(x => ((JValue)x).Value).ToArray());

        objectKeys.ForEach(key => result[key] = ToDictionary((JObject)result[key]));

        return result;
    }
}
