using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for an is (equality) comparison
    /// </summary>
    public class TokenExpressionIs : TokenExpressionBinary
    {
        public override int Priority => 6;

        /// <summary>
        /// The .NET type of this TokenExpression
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionIs"/> class
        /// </summary>
        public TokenExpressionIs(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Accepts the visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left} is {this.Right})";
        }
    }
}
