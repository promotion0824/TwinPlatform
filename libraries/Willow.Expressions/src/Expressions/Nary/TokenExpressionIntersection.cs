using System;
using System.Linq;
using Willow.Expressions.Visitor;
using Willow.Units;

namespace Willow.Expressions
{
    /// <summary>
    /// Intersection of multiple ranges or sets
    /// </summary>
    public class TokenExpressionIntersection : TokenExpressionNary
    {
        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 6;

        /// <summary>
        /// Are the children unordered?
        /// </summary>
        protected override bool IsUnordered { get => true; }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => this.Children.First().Type;

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionIntersection"/> class
        /// </summary>
        public TokenExpressionIntersection(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TokenExpressionIntersection"/> class
        /// </summary>
        public TokenExpressionIntersection(TokenExpression[] children)
            : base(children)
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
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return $"({string.Join(" ∩ ", this.Children.AsEnumerable())})";
        }
    }
}
