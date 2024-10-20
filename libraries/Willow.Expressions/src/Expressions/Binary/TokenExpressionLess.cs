using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for less than
    /// </summary>
    public class TokenExpressionLess : TokenExpressionBinary
    {
        public override int Priority => 6;

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionLess"/> class
        /// </summary>
        public TokenExpressionLess(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left} < {this.Right})";
        }
    }
}
