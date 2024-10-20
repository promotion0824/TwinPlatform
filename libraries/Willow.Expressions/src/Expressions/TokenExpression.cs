using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Willow.Expressions.Visitor;
using Willow.Units;
using Willow.Units.Expressions.Visitor;
using Willow.Units.Utility;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression can represent a constant, a numeric expression, a temporal expression, ...
    /// AboditNLP handles both numeric types and types with units (km, celcius, ...), time and temporal expressions
    /// </summary>
    /// <remarks>
    /// Use TokenInt when you want an actual number like '3'
    /// Use TokenExpression when you want either a value or an expression
    /// All VALUES are EXPRESSIONS, just very simple ones
    /// TBD: Expressions that evaluate to an INT vs a DOUBLE??
    /// </remarks>
    [DebuggerDisplay("{this.GetType().Name,nq} {this.Serialize(),nq}")]
    public abstract class TokenExpression : IEquatable<TokenExpression>
    {
        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Is commutatitive?
        /// </summary>
        /// <remarks>
        /// A commutative expression can be reordered without changing the result
        /// Defaults to false for safety
        /// Commutative operators include +, *, AND, OR, ...
        /// </remarks>
        public virtual bool Commutative => false;

        /// <summary>
        /// Get the Type that this Expression represents (if known)
        /// </summary>
        /// <remarks>
        /// Could this eventually become an IUnit? and have units for string, boolean etc.?
        /// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        /// No unit special string, see serializer
        /// </summary>
        public const string NOUNIT = "";

        /// <summary>
        /// Unit of measure
        /// </summary>
        public virtual string Unit { get; set; } = NOUNIT;

        /// <summary>
        /// An expression simplifier (singleton)
        /// </summary>
        public static readonly TokenExpressionSimplifier Simplifier = new TokenExpressionSimplifier();

        /// <summary>
        /// An expression serializer (singleton)
        /// </summary>
        public static readonly TokenExpressionSerializer Serializer = new TokenExpressionSerializer();

        /// <summary>
        /// An expression serializer (singleton)
        /// </summary>
        private static readonly GetUnboundVariablesVisitor UnboundVariablesVisitor = new GetUnboundVariablesVisitor();

        /// <summary>
        /// An expression to english visitor (singleton)
        /// </summary>
        public static readonly TokenExpressionEnglishVisitor EnglishMetricVisitor = new TokenExpressionEnglishVisitor(true);

        /// <summary>
        /// An expression to english visitor (singleton)
        /// </summary>
        public static readonly TokenExpressionEnglishVisitor EnglishImperialVisitor = new TokenExpressionEnglishVisitor(false);

        /// <summary>
        /// Gets all child expressions for this expression
        /// </summary>
        public abstract IEnumerable<TokenExpression> GetChildren();

        /// <summary>
        /// Evaluate the expression to get a value of type T
        /// If the expression does not evaluate to that type returns a Maybe not.
        /// </summary>
        public Maybe<TDest> Evaluate<TSource, TDest>(TSource obj)
        {
            var converter = new ConvertToExpressionVisitor<TSource>(
                (expr, variableName) => ConvertToExpressionVisitor<TSource>.GetterForObjectUsingReflection(expr, variableName.ToString()));
            var result = converter.Visit(this) as LambdaExpression;
            if (result is null) return new Maybe<TDest>();
            // result will be an Expression<Func<TSource,TSomethingElse>>(), e.g. double
            var finalResult = result as Expression<Func<TSource, TDest>>;
            if (finalResult is null)
            {
                // Add conversion step
                result = Expression.Lambda(Expression.Convert(result.Body, typeof(TDest)), result.Parameters);
                finalResult = result as Expression<Func<TSource, TDest>>;
            }
            return new Maybe<TDest> { HasValue = true, Value = finalResult!.Compile()(obj) };
        }

        /// <summary>
        /// Evaluate the expression to get a value of type T by evaluating the TokenExpression directly
        /// i.e. not using Expression and compiling it
        /// If the expression does not evaluate to that type returns a Maybe not.
        /// </summary>
        public Maybe<IConvertible> EvaluateDirect<TSource>(TSource obj, Func<string, ITemporalObject?>? temporalObjectGetter = null, Func<string, IMLRuntime?>? mlModelGetter = null)
        {
            var converter = new ConvertToValueVisitor<TSource>(obj,
                    ConvertToValueVisitor<TSource>.ObjectGetter, temporalObjectGetter, mlModelGetter);
            var result = converter.Visit(this);
#pragma warning disable CS8601 // Possible null reference assignment.
            return new Maybe<IConvertible> { HasValue = result != null, Value = result };
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        /// <summary>
        /// Evaluate the expression to get a value of type T by evaluating the TokenExpression directly
        /// i.e. not using Expression and compiling it
        /// If the expression does not evaluate to that type returns a Maybe not.
        /// </summary>
        public Maybe<IConvertible> EvaluateDirectUsingEnv(Env env, Func<string, ITemporalObject?>? temporalObjectGetter = null, Func<string, IMLRuntime?>? mlModelGetter = null)
        {
            return EvaluateDirectUsingEnv(env, out _, temporalObjectGetter, mlModelGetter);
        }

        /// <summary>
        /// Evaluate the expression to get a value of type T by evaluating the TokenExpression directly
        /// i.e. not using Expression and compiling it
        /// If the expression does not evaluate to that type returns a Maybe not.
        /// </summary>
        public Maybe<IConvertible> EvaluateDirectUsingEnv(Env env, out ConvertToValueVisitor<Env> converter, Func<string, ITemporalObject?>? temporalObjectGetter = null, Func<string, IMLRuntime?>? mlModelGetter = null)
        {
            converter = new ConvertToValueVisitor<Env>(env, ConvertToValueVisitor<Env>.ObjectGetterFromEnvironment, temporalObjectGetter, mlModelGetter);
            var result = converter.Visit(this);
#pragma warning disable CS8601 // Possible null reference assignment.
            return new Maybe<IConvertible> { HasValue = result != null, Value = result };
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        /// <summary>
        /// A Maybe may have a value or not, similar to Nullable (except it allows for T to be a struct or a class)
        /// </summary>
        public struct Maybe<T>
        {
            /// <summary>
            /// Gets the value
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// True if this has a value
            /// </summary>
            public bool HasValue { get; set; }

            /// <summary>
            /// Equals
            /// </summary>
            public override bool Equals(object? obj)
            {
                return obj is Maybe<T> m && this.HasValue.Equals(m.HasValue) && EqualityComparer<T>.Default.Equals(this.Value, m.Value);
            }

            /// <summary>
            /// GetHashCode
            /// </summary>
            public override int GetHashCode()
            {
                return (this.HasValue, this.Value).GetHashCode();
            }
        }

        /// <summary>
        /// Convert to a double value (or null if not one)
        /// </summary>
        public double? ToDouble()
        {
            var result = this.Evaluate<object, double>(new object());
            return result.HasValue ? result.Value : (double?)null;
        }

        /// <summary>
        /// Convert to a double value (or null if not one)
        /// </summary>
        public double? ToDouble<TSource>(TSource obj)
        {
            var result = this.Evaluate<TSource, double>(obj);
            return result.HasValue ? result.Value : (double?)null;
        }

        /// <summary>
        /// Convert to a bool value (or null if not one)
        /// </summary>
        public bool? ToBool<TSource>(TSource obj)
        {
            var result = this.Evaluate<TSource, bool>(obj);
            return result.HasValue ? (bool?)result.Value : (bool?)null;
        }

        /// <summary>
        /// Convert to a bool value (or null if not one)
        /// </summary>
        public bool? ToBool()
        {
            var result = this.Evaluate<object, bool>(new object());
            return result.HasValue ? (bool?)result.Value : (bool?)null;
        }

        /// <summary>
        /// Convert to a string value (or null if not one)
        /// </summary>
        public string? ToString<TSource>(TSource obj)
        {
            var result = this.Evaluate<TSource, string>(obj);
            return result.HasValue ? result.Value : null;
        }

        /// <summary>
        /// Convert to a datetime value (or null if not one)
        /// </summary>
        public DateTime? ToDateTime<TSource>(TSource obj)
        {
            var result = this.Evaluate<TSource, DateTime>(obj);
            return result.HasValue ? result.Value : (DateTime?)null;
        }

        /// <summary>
        /// Try converting to an expression of a given type, or return null if wrong type
        /// </summary>
        public Expression<Func<TSource, TDest>> Convert<TSource, TDest>()
        {
            var converter = new ConvertToExpressionVisitor<TSource>(
                ConvertToExpressionVisitor<TSource>.GetterForObjectUsingReflection);
            var result = converter.Visit(this);
            if (result is Expression<Func<TSource, TDest>> castResult) return castResult;
            throw new Exception($"Could not convert to {typeof(TSource).Name}, {typeof(TDest).Name}, got {result} which is {TypeUtility.CreateTypeStringFromType(result?.Type)}");
            //return castResult;
        }

        /// <summary>
        /// If y = f(x), find f'(y)
        /// </summary>
        public TokenExpression Invert(TokenExpression y, TokenExpressionVariableAccess x)
        {
            return new TokenExpressionInvertVisitor(y, x).Visit(this);
        }

        /// <summary>
        /// Simplify this TokenExpression
        /// </summary>
        public virtual TokenExpression Simplify()
        {
            return Simplifier.Visit(this);
        }

        /// <summary>
        /// Serialize this TokenExpression
        /// </summary>
        public string Serialize()
        {
            return Serializer.Visit(this);
        }

        /// <summary>
        /// Returns an english version of this
        /// </summary>
        /// <returns></returns>
        public virtual string Describe(bool metric)
        {
            return metric ? EnglishMetricVisitor.Visit(this) : EnglishImperialVisitor.Visit(this);
        }

        // TODO: This could be a visitor too

        /// <summary>
        /// Gets the unbound variables or fields
        /// </summary>
        public IEnumerable<string> UnboundVariables => UnboundVariablesVisitor.Visit(this).Where(x => !x.IsFunction).Select(x => x.Name).Distinct();

        /// <summary>
        /// Gets the unbound functions
        /// </summary>
        public IEnumerable<string> UnboundFunctions => UnboundVariablesVisitor.Visit(this).Where(x => x.IsFunction).Select(x => x.Name).Distinct();

        /// <summary>
        /// Gets the unbound variables or functions
        /// </summary>
        [Obsolete("User GetUnboundVariables")]
        public IEnumerable<UnboundVariableOrFunction> GetUnboundVariablesRecursive()
        {
            return GetUnboundVariables();
        }

        /// <summary>
        /// Gets the unbound variables or functions
        /// </summary>
        public IEnumerable<UnboundVariableOrFunction> GetUnboundVariables()
        {
            return UnboundVariablesVisitor.Visit(this).Distinct();
        }

        /// <summary>
        /// Binds a variable to a value and returns a new TokenExpression with the substitution in place
        /// </summary>
        public TokenExpression Bind(string variableName, object value)
        {
            // convert any variable access for this value to the constant value itself
            var visitor = new TokenExpressionRebinder(Env.Empty.Push().Assign(variableName, value));
            return this.Accept<TokenExpression>(visitor);
        }

        /// <summary>
        /// Gets the original text string
        /// </summary>
        public string Text { get; set; } = "";

        private static readonly Lazy<TokenExpressionConstantNull> nullInstance = new Lazy<TokenExpressionConstantNull>(() => new TokenExpressionConstantNull());
        private static readonly Lazy<TokenExpressionConstantBool> trueInstance = new Lazy<TokenExpressionConstantBool>(() => new TokenExpressionConstantBool(true));
        private static readonly Lazy<TokenExpressionConstantBool> falseInstance = new Lazy<TokenExpressionConstantBool>(() => new TokenExpressionConstantBool(false));

        /// <summary>
        /// A Null TokenExpression
        /// </summary>
        public static TokenExpressionConstantNull Null => nullInstance.Value;

        /// <summary>
        /// A True TokenExpression
        /// </summary>
        public static TokenExpressionConstantBool True => trueInstance.Value;

        /// <summary>
        /// A False TokenExpression
        /// </summary>
        public static TokenExpressionConstantBool False => falseInstance.Value;

        /// <summary>
        /// All TokenExpressions need to implement Equals() so that EqualityComparer can work
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool Equals(TokenExpression? other);

        // Use this to check that they do, but don't keep it here as it messes up check that all implement it
        ///// <summary>
        ///// All TokenExpressions need to implement GetHashCode() explicitly so that EqualityComparer can work
        ///// </summary>

        /// <summary>
        /// Accepts the visitor, visiting it with this object
        /// </summary>
        public abstract T Accept<T>(ITokenExpressionVisitor<T> visitor);

        /// <summary>
        /// Creates an AndExpression
        /// </summary>
        public static TokenExpression CombineAndExpressions(params TokenExpression[] expressions)
        {
            if (!expressions.Any()) return TokenExpression.True;
            var nonNull = expressions.Where(x => x != null).ToList();
            if (!nonNull.Any()) return TokenExpression.True;
            if (nonNull.Count == 1) return nonNull.First();
            return new TokenExpressionAnd(nonNull.ToArray());
        }

        /// <summary>
        /// And with another expression
        /// </summary>
        public TokenExpression And(TokenExpression other)
        {
            if (other is null) return this;
            if (this == True) return other;
            if (other == True) return this;
            if (this == False) return this;
            if (other == False) return other;
            return new TokenExpressionAnd(this, other);
        }

        /// <summary>
        /// Or with another expression
        /// </summary>
        public TokenExpression Or(TokenExpression other)
        {
            if (this == True) return this;
            if (other == True) return other;
            if (this == False) return other;
            if (other == False) return this;
            return new TokenExpressionOr(this, other);
        }

        /// <summary>
        /// Create a new TokenExpression for Add
        /// </summary>
        public TokenExpression Add(TokenExpression right)
        {
            return new TokenExpressionAdd(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Subtract
        /// </summary>
        public TokenExpression Subtract(TokenExpression right)
        {
            return new TokenExpressionSubtract(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Multiply
        /// </summary>
        public TokenExpression Multiply(TokenExpression right)
        {
            return new TokenExpressionMultiply(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Divide
        /// </summary>
        public TokenExpression Divide(TokenExpression right)
        {
            return new TokenExpressionDivide(this, right);
        }

        /// <summary>
        /// Raise this TokenExpression to a power
        /// </summary>
        public TokenExpression Power(TokenExpression right)
        {
            return new TokenExpressionPower(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Equals
        /// </summary>
        public TokenExpression Equal(TokenExpression right)
        {
            return new TokenExpressionEquals(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for not equals
        /// </summary>
        public TokenExpression NotEqual(TokenExpression right)
        {
            return new TokenExpressionNotEquals(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Greater
        /// </summary>
        public TokenExpression Greater(TokenExpression right)
        {
            return new TokenExpressionGreater(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Greater or Equals
        /// </summary>
        public TokenExpression GreaterOrEqual(TokenExpression right)
        {
            return new TokenExpressionGreaterOrEqual(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Less
        /// </summary>
        public TokenExpression Less(TokenExpression right)
        {
            return new TokenExpressionLess(this, right);
        }

        /// <summary>
        /// Create a new TokenExpression for Less or equals
        /// </summary>
        public TokenExpression LessOrEqual(TokenExpression right)
        {
            return new TokenExpressionLessOrEqual(this, right);
        }
    }
}
