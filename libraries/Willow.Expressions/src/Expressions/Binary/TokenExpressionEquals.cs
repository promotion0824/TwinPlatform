using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for an equality comparison
    /// </summary>
    public class TokenExpressionEquals : TokenExpressionBinary
    {
        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 6;

        /// <summary>
        /// The .NET type of this TokenExpression
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionEquals"/> class
        /// </summary>
        public TokenExpressionEquals(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left} = {this.Right})";
        }
    }
}
