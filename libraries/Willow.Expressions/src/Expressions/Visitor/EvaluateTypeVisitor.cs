using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// Base class for a visitor that evaluates the dotnet type for an expression in an environment
    /// </summary>
    public abstract class EvaluateTypeVisitorBase
    {
        private sealed class UndefinedType { }

        /// <summary>
        /// A singleton Undefined value
        /// </summary>
        public static readonly Type Undefined = typeof(UndefinedType);
    }

    /// <summary>
    /// Evaluates the type of an expression using an environment
    /// </summary>
    public class EvaluateTypeVisitor<TSource> : EvaluateTypeVisitorBase, ITokenExpressionVisitor<Type>
    {
        /// <summary>
        /// A TimeZoneOffset for the local / UTC conversions
        /// </summary>
        public TimeSpan TimeZoneOffset { get; set; } = TimeZoneInfo.Local.BaseUtcOffset;

        /// <summary>
        /// Strict after all temporal sets and ranges have been converted, unstrict treats them as likely bool results
        /// </summary>
        private readonly bool strict;

        /// <summary>
        /// Creates a new instance of the <see cref="EvaluateTypeVisitor{TSource}"/> class
        /// </summary>
        public EvaluateTypeVisitor(bool strict)
        {
            this.strict = strict;
        }

        /// <summary>
        /// A sample Getter for use against Objects (using reflection)
        /// </summary>
        private static Type ObjectGetter(Type type, string variableName)
        {
            // e.g. .On needs to be a access to the On property of a TSource
            var propertyInfo = type.GetInterfaces()
                .Concat(Enumerable.Repeat(type, 1))
                .Select(i => i.GetProperty(variableName, BindingFlags.Instance | BindingFlags.Public))
                .FirstOrDefault(x => x != null);

            if (propertyInfo is null) return Undefined;

            return propertyInfo.PropertyType;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type Visit(TokenExpression input)
        {
            var expr = input.Accept(this);
            return expr;
        }

        /// <summary>
        /// Create a combo from a binary expression
        /// </summary>
        protected virtual Type CreateCombo(TokenExpressionBinary input, Func<Type, Type, Type> create)
        {
            return CreateCombo(input.Left, input.Right, create);
        }

        /// <summary>
        /// Create a combo from an expression
        /// </summary>
        protected virtual Type CreateCombo(TokenExpression leftHalf, TokenExpression rightHalf, Func<Type, Type, Type> create)
        {
            var left = leftHalf.Accept(this);
            if (left is null) return Undefined;
            var right = rightHalf.Accept(this);
            if (right is null) return Undefined;
            return create(left, right);
        }

        /// <summary>
        /// Create a combo from an Nary expression
        /// </summary>
        protected virtual Type CreateCombo(TokenExpressionNary input, Func<Type, Type, Type> create)
        {
            var children = input.Children.Select(c => (c.Accept(this))).ToList();
            if (children.Any(c => c is null)) return Undefined;
            var combined = children.Skip(1).Aggregate(children.First(), create);
            return combined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionConvertToLocalDateTime input)
        {
            var child = input.Child.Accept(this);
            if (!(child == typeof(DateTime)))
            {
                return Undefined;
            }
            else
            {
                return child;       // DateTime
            }
        }

        private static bool IsNumeric(Type t) =>
            t == typeof(int) ||
            t == typeof(long) ||
            t == typeof(double) ||
            t == typeof(decimal);

        private static bool IsIntegral(Type t) => t == typeof(int) || t == typeof(long);

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

        private static bool CanSubtract(Type a, Type b) => ((IsNumeric(a) && IsNumeric(b))) ||
            ((IsDateTime(a) && IsDateTime(b))) || ((IsDateTime(a) && IsTimeSpan(b)));

        private static Type Math(Type a, Type b)
        {
            // string plus anything is a string
            if (IsString(a) || IsString(b)) return typeof(string);
            // double plus anything is a double
            if ((a == typeof(double) || b == typeof(double)) && IsNumeric(a) && IsNumeric(b)) return typeof(double);
            // float plus anything is a float (except a double)
            if ((a == typeof(float) || b == typeof(float)) && IsNumeric(a) && IsNumeric(b)) return typeof(float);
            // decimal plus anything is a decimal (except a double or a float)
            if ((a == typeof(decimal) || b == typeof(decimal)) && IsNumeric(a) && IsNumeric(b)) return typeof(decimal);
            // long plus long or int is long
            if ((a == typeof(long) || b == typeof(long)) && IsIntegral(a) && IsIntegral(b)) return typeof(long);
            return Undefined;
        }

        private static Type BooleanMath(Type a, Type b)
        {
            return a == typeof(bool) && b == typeof(bool) ? typeof(bool) : Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionAdd input)
        {
            return CreateCombo(input, Math);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionMultiply input)
        {
            return CreateCombo(input, Math);
        }

        /// <summary>
        /// Converts match (variable, temporal expression) into an expression returning a bool
        /// </summary>
        public virtual Type DoVisit(TokenExpressionMatches input)
        {
            var left = input.Left.Accept(this);
            if (left == Undefined) return Undefined;

            if (input.Right is TokenDouble)
            {
                return typeof(bool);
            }
            else if (input.Right is TokenExpressionConstantColor)
            {
                return typeof(bool);
            }
            else
            {
                Trace.TraceError($"Match could not be converted to expression: {input.Serialize()}");
                throw new Exception($"Matches can only be used with a TemporalSet, a Range, a Set or a Double, not a {input.Serialize()} in EvaluateType");
            }
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionAnd input)
        {
            return CreateCombo(input, BooleanMath);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionOr input)
        {
            return CreateCombo(input, BooleanMath);
        }

        public virtual Type DoVisit(TokenExpressionTernary input)
        {
            var conditional = input.Conditional.Accept(this);
            if (conditional != typeof(bool)) return Undefined;

            // Special case for when condition is constant
            if (input.Conditional == TokenExpression.True) return input.Truth.Accept(this);

            var left = input.Truth.Accept(this);
            var right = input.Truth.Accept(this);

            if (left == right) return left;

            // Otherwise we have a problem, the two types are different
            // should try to find a common ancestor
            return Math(left, right);                   // hack for now, at least it gets strings and doubles right
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionTuple input)
        {
            // Has multiple types
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionConstantNull input)
        {
            return input.Type;      // object
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionConstantDateTime input)
        {
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionConstantString input)
        {
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionArray input)
        {
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionDivide input)
        {
            return CreateCombo(input, Math);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenDouble input)
        {
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionConstant input)
        {
            // Never called since the specific ones are implemented
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>

        public virtual Type DoVisit(TokenExpressionConstantBool input)
        {
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionConstantColor input)
        {
            return input.Type;
        }

        private Type HandleComparison(TokenExpressionBinary input)
        {
            var left = input.Left.Accept(this);
            var right = input.Right.Accept(this);
            if (left == right) return typeof(bool);             // Clearly compatible

            if (IsNumeric(left) && IsNumeric(right)) return typeof(bool);
            if (IsString(left) && IsString(right)) return typeof(bool);
            if (left == typeof(object)) return typeof(bool);        // Permissively allow fields/vars for which we don't know type
                                                                    // otherwise, really don't know
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionIs input)
        {
            return HandleComparison(input);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionEquals input)
        {
            return HandleComparison(input);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionGreater input)
        {
            return HandleComparison(input);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionGreaterOrEqual input)
        {
            return HandleComparison(input);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionNot input)
        {
            var child = input.Child.Accept(this);
            if (child == typeof(bool)) return child;
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionUnaryMinus input)
        {
            var child = input.Child.Accept(this);
            return child;           // assume it's valid, e.g. -TimePeriod
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionLess input)
        {
            return HandleComparison(input);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionLessOrEqual input)
        {
            return HandleComparison(input);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionNotEquals input)
        {
            return HandleComparison(input);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionPower input)
        {
            var left = input.Left.Accept(this);
            var right = input.Right.Accept(this);
            return Math(left, right);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionSubtract input)
        {
            var left = input.Left.Accept(this);
            var right = input.Right.Accept(this);

            if (left == typeof(DateTime) && right == typeof(DateTime)) return typeof(TimeSpan);
            if (left == typeof(DateTime) && right == typeof(TimeSpan)) return typeof(DateTime);

            if (left == right) return left;         // Most things subtracted against themselves don't change type

            // otherwise assume it's simple math
            return Math(left, right);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionIdentity input)
        {
            return input.Child.Accept(this);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionFailed input)
        {
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionPropertyAccess input)
        {
            return input.Type;
            // TBD, do we want to try something like this, or can we assume expr was build with a set Type
            //Type childType = input.Child.Accept(this);
            //Type propertyType = ObjectGetter(childType, input.PropertyName);
            //// TODO: Really need to create a new visitor that dives into this type to check that input.Child is the right type
            //// But for now, just go a single level deep
            //return propertyType;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionVariableAccess input)
        {
            return input.Type;
            // TBD, do we want to try something like this, or can we assume expr was build with a set Type
            //Type propertyType = ObjectGetter(typeof(TSource), input.VariableName);
            // TODO: Really need to create a new visitor that dives into this type to check that input.Child is the right type
            // But for now, just go a single level deep
            //return propertyType;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionFunctionCall input)
        {
            // Hmmm ... how to know this, especiallu if it's polymorphic?
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionIntersection input)
        {
            throw new NotImplementedException("Need to visit sides, and do a Range or Set union on them");
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionSetUnion input)
        {
            throw new NotImplementedException("Need to visit sides, and do a Range or Set union on them");
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionSum input)
        {
            if (input.Child is TokenExpressionArray array) return CreateCombo(array, Math);
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionCount input)
        {
            if (input.Child is TokenExpressionArray array) return typeof(int);
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionAverage input)
        {
            return typeof(double);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionFirst input)
        {
            if (input.Child is TokenExpressionArray array) return array.Children.FirstOrDefault()?.Accept(this) ?? typeof(double); // really unknown
            return input.Child.Accept(this);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionAny input)
        {
            return typeof(bool);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionEach input)
        {
            // The body defines the type, except it's an array of that type
            return input.Body.Accept(this);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionAll input)
        {
            return typeof(bool);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionMin input)
        {
            if (input.Child is TokenExpressionArray array) return CreateCombo(array, Math);
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionMax input)
        {
            if (input.Child is TokenExpressionArray array) return CreateCombo(array, Math);
            return Undefined;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public virtual Type DoVisit(TokenExpressionParameter input)
        {
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public Type DoVisit(TokenExpressionWrapped input)
        {
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public Type DoVisit(TokenExpressionTemporal input)
        {
            return input.Type;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public Type DoVisit(TokenExpressionTimer input)
        {
            return input.Type;
        }
    }
}
