using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Any of an Enumerable of a token expression
    /// </summary>
    public class TokenExpressionAny : TokenExpressionLinq
    {
        /// <summary>
        /// Creates a new instance of the Any class for checking an enumerable of bool
        /// </summary>
        public TokenExpressionAny(TokenExpression input) : base(input)
        {
        }

        /// <summary>
        /// Gets the relative priority compared to other TokenExpression types
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Get the result type
        /// </summary>
        public override Type Type => typeof(bool);

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
            return $"ANY({this.Child})";
        }
    }
}
