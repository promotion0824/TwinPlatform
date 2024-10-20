using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Min of an Enumerable of a token expression
    /// </summary>
    public class TokenExpressionMin : TokenExpressionLinq
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionMin"/> class
        /// </summary>
        public TokenExpressionMin(TokenExpression input) : base(input)
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
            return $"MIN({this.Child})";
        }
    }
}
