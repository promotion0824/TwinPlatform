using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for a unary minus operator
    /// </summary>
    public class TokenExpressionUnaryMinus : TokenExpressionUnary
    {
        public override int Priority => 11;

        public override Type Type => typeof(double);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionUnaryMinus"/> class
        /// </summary>
        public TokenExpressionUnaryMinus(TokenExpression child) : base(child)
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"-{this.Child}";
        }
    }
}
