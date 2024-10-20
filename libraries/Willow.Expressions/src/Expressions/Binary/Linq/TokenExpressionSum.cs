using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Sum of an Enumerable of a token expression
    /// </summary>
    public class TokenExpressionSum : TokenExpressionLinq
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionSum"/> class
        /// </summary>
        public TokenExpressionSum(TokenExpression input) : base(input)
        {
        }

        public override int Priority => 1;

        public override Type Type => typeof(double);

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"SUM({this.Child})";
        }
    }
}
