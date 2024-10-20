using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for a logical OR
    /// </summary>
    public class TokenExpressionOr : TokenExpressionNary
    {
        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        /// Are the children unordered?
        /// </summary>
        protected override bool IsUnordered { get => true; }

        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionOr"/> class
        /// </summary>
        public TokenExpressionOr(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionOr"/> class
        /// </summary>
        public TokenExpressionOr(params TokenExpression[] children)
            : base(children)
        {
        }

        /// <summary>
        /// Create a new TokenExpressionOr unless there is just one value in which case return it
        /// </summary>
        public static TokenExpression CreateAndSimplify(IEnumerable<TokenExpression> expressions)
        {
            if (!expressions.Any()) return TokenExpression.False;
            if (expressions.Count() == 1) return expressions.First();

            // But if it's numeric, simplify it

            return new TokenExpressionOr(expressions.ToArray()).Simplify();
        }

        /// <summary>
        /// Create a new TokenExpressionOr unless there is just one value in which case return it
        /// </summary>
        public static TokenExpression CreateAndSimplify(params TokenExpression[] expressions)
        {
            return CreateAndSimplify(expressions.AsEnumerable());
        }

        /// <summary>
        /// Accepts the visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return $"({string.Join(" | ", this.Children.AsEnumerable())})";
        }
    }
}
