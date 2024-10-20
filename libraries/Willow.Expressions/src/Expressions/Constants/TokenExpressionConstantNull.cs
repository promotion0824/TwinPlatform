using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// The Null constant TokenExpression
    /// </summary>
    public class TokenExpressionConstantNull : TokenExpressionConstant
    {
        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(object);

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionConstantNull"/> class
        /// </summary>
        public TokenExpressionConstantNull() : base("null")
        {
        }

        /// <summary>
        /// Accepts the visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? other)
        {
            return other is TokenExpressionConstantNull;
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is TokenExpressionConstantNull;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return 77;
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return "null";
        }
    }
}
