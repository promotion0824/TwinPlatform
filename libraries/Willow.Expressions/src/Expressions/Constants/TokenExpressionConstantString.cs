using System;
using System.Globalization;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A constant string TokenExpression
    /// </summary>
    public class TokenExpressionConstantString : TokenExpressionConstant
    {
        /// <summary>
        /// Gets the string value
        /// </summary>
        public string ValueString { get; }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(string);

        /// <summary>
        /// Creates a new instance of <see cref="TokenExpressionConstantString"/>
        /// </summary>
        public TokenExpressionConstantString(string value) : base(value)
        {
            this.ValueString = value;
        }

        /// <summary>
        /// Accepts the visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return $"'{this.Value.ToString(CultureInfo.InvariantCulture)}'";
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? other)
        {
            var constant = other as TokenExpressionConstantString;
            if (constant is null) return false;
            if (this.Value is null) return constant.Value is null;
            return this.ValueString.Equals(constant.ValueString);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is TokenExpressionConstantString tecs && Equals(tecs);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            if (this.Value is null) return 1;
            return this.ValueString.GetHashCode();
        }
    }
}
