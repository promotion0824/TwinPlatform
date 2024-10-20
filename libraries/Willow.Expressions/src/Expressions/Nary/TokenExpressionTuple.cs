using System;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A Tuple is a tuple of TokenExpressions grouped together as one unit
    /// e.g. Tuple (var:Name, var:Height, var:Width)
    /// </summary>
    public class TokenExpressionTuple : TokenExpressionNary
    {
        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(Tuple);

        /// <summary>
        /// Are the children unordered?
        /// </summary>
        protected override bool IsUnordered { get => false; }

        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionTuple"/> class
        /// </summary>
        public TokenExpressionTuple(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionTuple"/> class
        /// </summary>
        public TokenExpressionTuple(params TokenExpression[] children)
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
            return $"{{{string.Join(", ", this.Children.AsEnumerable())}}}";
        }
    }
}
