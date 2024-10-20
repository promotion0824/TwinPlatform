using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Identity is used to wrap parenthetical expressions
    /// </summary>
    public class TokenExpressionIdentity : TokenExpressionUnary
    {
        public override int Priority => 1000;

        public override Type Type => this.Child.Type;

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionIdentity"/> class
        /// </summary>
        public TokenExpressionIdentity(TokenExpression wrapped) : base(wrapped)
        {
        }

        public override string ToString()
        {
            return $"({this.Child})";
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }
    }
}
