using System;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Failed is used to wrap failing expressions
    /// </summary>
    public class TokenExpressionFailed : TokenExpressionNary
    {
        public override int Priority => 1000;

        public override Type Type => typeof(bool);

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionFailed"/> class
        /// </summary>
        public TokenExpressionFailed(params TokenExpression[] children)
            : base(children)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionFailed"/> class
        /// </summary>
        public TokenExpressionFailed(string error, TokenExpression child)
            : base(TokenExpressionConstant.Create(error), child)
        {
        }

        protected override bool IsUnordered => false;

        public override string ToString()
        {
            return $"FAILED({string.Join(",", this.Children.Select(x => x.ToString()))})";
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }
    }
}
