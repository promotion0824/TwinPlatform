using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Matches is a fuzzier equals that allows matching of ranges, sets, datetime expressions, ...
    /// </summary>
    /// <remarks>
    /// For now LEFT should be a VariableAccess and RIGHT should be a TemporalSet
    /// [Decided that TemporalSets behave like constants NOT functions, 'match' is the function]
    /// </remarks>
    public class TokenExpressionMatches : TokenExpressionBinary
    {
        public override int Priority => 6;

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionMatches"/> class
        /// </summary>
        public TokenExpressionMatches(TokenExpression left, TokenExpression right)
            : base(left, right)
        {
            if (!MatchCouldMakeSense(left, right)) throw new ArgumentException($"Cannot use {left} as LHS for matches, rhs = {right}");
        }

        /// <summary>
        /// Returns true if this could make sense as a match expression
        /// </summary>
        public static bool MatchCouldMakeSense(TokenExpression left, TokenExpression right)
        {
            if (left is TokenExpressionMatches) return false;
            if (left is TokenExpressionAnd) return false;
            if (left is TokenExpressionOr) return false;
            if (left is TokenExpressionNot) return false;
            if (left is TokenExpressionEquals) return false;
            if (left is TokenExpressionNotEquals) return false;
            if (left is TokenExpressionLess) return false;
            if (left is TokenExpressionLessOrEqual) return false;
            if (left is TokenExpressionGreater) return false;
            if (left is TokenExpressionGreaterOrEqual) return false;
            if (left.Type == typeof(bool)) return false;         // (genre == 'pop') is under 30 seconds)

            return true;
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({this.Left} ∈ {this.Right})";
        }

        /// <summary>
        /// Static method to create a Match OR a simple inequality
        /// </summary>
        public static TokenExpression CreateMatchOrInequaity(TokenExpression left, TokenExpression right)
        {
            //// If the left has a Unit and the right does not, remove the unit
            //// from the left (easiest way to do that is to apply the inverse function)
            //if (left.Unit != null && !right.Unit.HasUnits)
            //{
            //    var inverseCanonicalize = left.Unit.InverseCanonicalizeTokenExpression;
            //    left = inverseCanonicalize(left);
            //}

            var match = new TokenExpressionMatches(left, right);
            return match;
        }
    }
}
