using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Converts an inner expression to a local datetime. Necessary because "on a wednesday"
    /// means a local datetime Wedesday. BUT this cannot work well for past events because of
    /// daylight savings time changes. Ick.
    /// </summary>
    public class TokenExpressionConvertToLocalDateTime : TokenExpressionUnary
    {
        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(DateTime);

        /// <summary>
        /// Creates a new <see cref="TokenExpressionConvertToLocalDateTime"/>
        /// </summary>
        public TokenExpressionConvertToLocalDateTime(TokenExpression child) : base(child)
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"LocalTime({this.Child})";
        }
    }
}
