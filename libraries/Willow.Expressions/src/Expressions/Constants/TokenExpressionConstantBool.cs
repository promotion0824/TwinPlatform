using System;
using System.Globalization;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression representing a constant boolean value
    /// </summary>
    public class TokenExpressionConstantBool : TokenExpressionConstant
    {
        /// <summary>
        /// The boolean value
        /// </summary>
        public bool ValueBool { get; }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionConstantBool"/> class
        /// </summary>
        public TokenExpressionConstantBool(bool value) : base(value)
        {
            this.ValueBool = value;
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
            return string.IsNullOrEmpty(this.Text) ? $"{this.Value.ToString(CultureInfo.InvariantCulture)}" : this.Text;
        }

        /// <summary>
        /// Compare to another TokenExpression
        /// </summary>
        public override bool Equals(TokenExpression? other)
        {
            return (other is TokenExpressionConstantBool boolExpr && boolExpr.ValueBool.Equals(this.ValueBool));
        }

        /// <summary>
        /// Compare to another TokenExpression
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is TokenExpressionConstantBool t && Equals(t);
        }

        /// <summary>
        /// Get hash code
        /// </summary>
        public override int GetHashCode()
        {
            return this.ValueBool.GetHashCode();
        }
    }
}
