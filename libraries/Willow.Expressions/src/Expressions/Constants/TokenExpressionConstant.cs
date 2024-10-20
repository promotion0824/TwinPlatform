using System;
using System.Collections.Generic;
using System.Globalization;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression that has a constant value
    /// </summary>
    /// <remarks>
    /// This is always a leaf node and has no children
    /// </remarks>
    public abstract class TokenExpressionConstant : TokenExpression
    {
        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 1000;

        /// <summary>
        /// Gets the IConvertible value
        /// </summary>
        public IConvertible Value { get; }

        /// <summary>
        /// Creates a new <see cref="TokenExpressionConstant"/>
        /// </summary>
        protected TokenExpressionConstant(IConvertible value)
        {
            this.Value = value;
        }

        public override IEnumerable<TokenExpression> GetChildren()
        {
            return Array.Empty<TokenExpression>();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (this.Value is null) return "null";
            return this.Value.ToString(CultureInfo.InvariantCulture);
        }

        // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
        /// <summary>
        /// Create a new constant token expression from a value
        /// </summary>
        public static TokenExpression Create(object? value)
        {
            if (value is null) return TokenExpressionConstant.Null;
            if (value is string) return new TokenExpressionConstantString((string)value);
            if (value is double) return new TokenDouble((double)value);
            if (value is long) return new TokenDouble((long)value);
            if (value is int) return new TokenDouble((int)value);
            if (value is bool) return Create((bool)value);      // use singletons
                                                                //if (value is TimePeriod) return (TimePeriod) value;

            // BUG: getting TokenExpressions passed in
            if (value is TokenExpression) return (TokenExpression)value;

            throw new ArgumentException($"Could not create a constant of type {value.GetType().Name}");
        }

        /// <summary>
        /// Create a new constant token expression from a value with a unit
        /// </summary>
        public static TokenExpression Create(object? value, string unit)
        {
            var tokenExpression = Create(value);
            tokenExpression.Unit = unit;
            return tokenExpression;
        }

        // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull

        /// <summary>
        /// Create a new <see cref="TokenExpressionConstantBool"/>
        /// </summary>
        public static TokenExpressionConstantBool Create(bool value)
        {
            if (value) return True; else return False;
        }

        /// <summary>
        /// Create a new <see cref="TokenExpressionConstantString"/>
        /// </summary>
        public static TokenExpressionConstantString Create(string value)
        {
            return new TokenExpressionConstantString(value);
        }

        /// <summary>
        /// Create a new <see cref="TokenDouble"/> from a double
        /// </summary>
        public static TokenDouble Create(double value)
        {
            return new TokenDouble(value);
        }

        /// <summary>
        /// Create a new <see cref="TokenDouble"/> from a double
        /// </summary>
        public static TokenDouble Create(double value, string unit)
        {
            return new TokenDouble(value) { Unit = unit };
        }

        /// <summary>
        /// Create a new <see cref="TokenDouble"/> from a long
        /// </summary>
        public static TokenExpression Create(long value)
        {
            return new TokenDouble(value);
        }

        /// <summary>
        /// Create a new <see cref="TokenDouble"/> from an int
        /// </summary>
        public static TokenExpression Create(int value)
        {
            return new TokenDouble(value);
        }
    }
}
