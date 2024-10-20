using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Max of an Enumerable of a token expression
    /// </summary>
    public class TokenExpressionMax : TokenExpressionLinq
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionMax"/> class for checking an enumerable of numbers
        /// </summary>
        public TokenExpressionMax(TokenExpression input) : base(input)
        {
        }

        /// <summary>
        /// Gets the relative priority compared to other TokenExpression types
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Get the result type
        /// </summary>
        public override Type Type => typeof(double);

        /// <summary>
        /// Accepts a visitor
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
            return $"MAX({this.Child})";
        }
    }
}
