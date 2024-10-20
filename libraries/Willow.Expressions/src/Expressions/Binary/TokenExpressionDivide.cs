using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for divide
    /// </summary>
    public class TokenExpressionDivide : TokenExpressionBinary
    {
        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 20;

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(double);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionDivide"/> class
        /// </summary>
        public TokenExpressionDivide(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionDivide"/> class
        /// </summary>
        public TokenExpressionDivide(TokenExpression left, double right)
            : base(left, TokenExpressionConstant.Create(right))
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left} / {this.Right})";
        }
    }
}
