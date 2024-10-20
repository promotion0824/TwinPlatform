using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for subtraction
    /// </summary>
    public class TokenExpressionSubtract : TokenExpressionBinary
    {
        private readonly Lazy<Type> getType;

        public override int Priority => 10;

        public override Type Type => getType.Value;

        private Type CalculateType()
        {
            if (this.Left.Type == typeof(DateTime))
            {
                if (this.Right.Type == typeof(DateTime))
                    return typeof(TimeSpan);
                if (this.Right.Type == typeof(TimeSpan))
                    return typeof(DateTime);
            }
            if (this.Left.Type == typeof(DateTimeOffset))
            {
                if (this.Right.Type == typeof(DateTimeOffset))
                    return typeof(TimeSpan);
                if (this.Right.Type == typeof(TimeSpan))
                    return typeof(DateTimeOffset);
            }
            return typeof(double);
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionSubtract"/> class
        /// </summary>
        public TokenExpressionSubtract(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
            getType = new(() => CalculateType());
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionSubtract"/> class
        /// </summary>
        public TokenExpressionSubtract(TokenExpression left, double right)
            : this(left, TokenExpressionConstant.Create(right))
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left} - {this.Right})";
        }
    }
}
