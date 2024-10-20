using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for greater than
    /// </summary>
    public class TokenExpressionGreater : TokenExpressionBinary
    {
        public override int Priority => 6;

        public override Type Type => typeof(bool);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionGreater"/> class
        /// </summary>
        public TokenExpressionGreater(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left} > {this.Right})";
        }
    }
}
