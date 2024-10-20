using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// TokenExpression for a raised to the power b
    /// </summary>
    public class TokenExpressionPower : TokenExpressionBinary
    {
        public override int Priority => 30;

        public override Type Type => typeof(double);

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionPower"/> class
        /// </summary>
        public TokenExpressionPower(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionPower"/> class
        /// </summary>
        public TokenExpressionPower(TokenExpression left, double right)
            : base(left, TokenExpressionConstant.Create(right))
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left}^{this.Right})";
        }
    }
}
