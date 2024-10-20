using System;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for multiply
    /// </summary>
    public class TokenExpressionMultiply : TokenExpressionNary
    {
        public override int Priority => 20;

        protected override bool IsUnordered { get => true; }

        public override bool Commutative => true;

        public override Type Type => typeof(double);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionMultiply"/> class
        /// </summary>
        public TokenExpressionMultiply(params TokenExpression[] children)
            : base(children)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionMultiply"/> class
        /// </summary>
        public TokenExpressionMultiply(TokenExpression left, double right)
            : base(left, TokenExpressionConstant.Create(right))
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({string.Join(" * ", this.Children.Select(c => c.ToString()))})";
        }

        /// <summary>
        /// Create a multiply expression or 1.0
        /// </summary>
        public static TokenExpression Create(TokenExpression[] children)
        {
            if (!children.Any()) return new TokenDouble(1.0);
            else if (children.Length == 1) return children.First();
            else return new TokenExpressionMultiply(children.ToArray());
        }
    }
}
