using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for a logical And
    /// </summary>
    public class TokenExpressionAnd : TokenExpressionNary
    {
        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        /// Are the children of this expression un-ordered?
        /// </summary>
        protected override bool IsUnordered { get => true; }

        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionAnd"/> class
        /// </summary>
        public TokenExpressionAnd(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionAnd"/> class
        /// </summary>
        public TokenExpressionAnd(params TokenExpression[] children)
            : base(children)
        {
        }

        /// <summary>
        /// Create a new TokenExpressionAnd unless there is just one value in which case return it
        /// </summary>
        public static TokenExpression CreateAndSimplify(IEnumerable<TokenExpression> expressions)
        {
            if (!expressions.Any()) return TokenExpression.False;
            if (expressions.Count() == 1) return expressions.First();
            return new TokenExpressionAnd(expressions.ToArray()).Simplify();
        }

        /// <summary>
        /// Create a new TokenExpressionAnd unless there is just one value in which case return it
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
            return $"({string.Join(" & ", this.Children.AsEnumerable())})";
        }

        /// <summary>
        /// Factory method to create an And Expression simplifying it as it's created
        /// </summary>
        public static TokenExpression CreateAndSimplify(TokenExpression left, TokenExpression right)
        {
            if (left == TokenExpression.False) return TokenExpression.False;
            if (right == TokenExpression.False) return TokenExpression.False;
            if (left == TokenExpression.True) return right;
            if (right == TokenExpression.True) return left;
            return new TokenExpressionAnd(left, right);
        }
    }
}
