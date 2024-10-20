using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Count of an Enumerable of a token expression
    /// </summary>
    public class TokenExpressionCount : TokenExpressionLinq
    {
        /// <summary>
        /// Creates a new instance of the Count class for checking an enumerable of bool
        /// </summary>
        public TokenExpressionCount(TokenExpression input) : base(input)
        {
        }

        public override int Priority => 1;

        public override Type Type => typeof(int);

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"COUNT({this.Child})";
        }
    }
}
