using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for the logical Not operator
    /// </summary>
    public class TokenExpressionNot : TokenExpressionUnary
    {
        public override int Priority => 2;  // higher than AND/OR

        public override Type Type => typeof(bool);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionNot"/> class
        /// </summary>
        public TokenExpressionNot(TokenExpression child) : base(child)
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"!{this.Child}";
        }
    }
}
