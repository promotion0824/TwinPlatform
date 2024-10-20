using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// An abstract token expression for linq expressions
    /// </summary>
    public abstract class TokenExpressionLinq : TokenExpressionUnary
    {
        /// <summary>
        /// Creates a new <see cref="TokenExpressionLinq"/>
        /// </summary>
        protected TokenExpressionLinq(TokenExpression input) : base(input)
        {
        }
    }
}
