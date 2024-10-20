using System;
using System.Collections.Generic;

namespace Willow.Expressions
{
    /// <summary>
    /// A binary TokenExpression
    /// </summary>
    public abstract class TokenExpressionBinary : TokenExpressionFormula
    {
        /// <summary>
        /// The left expression
        /// </summary>
        public TokenExpression Left { get; }

        /// <summary>
        /// The right expression
        /// </summary>
        public TokenExpression Right { get; }

        public override IEnumerable<TokenExpression> GetChildren()
        {
            return new[] { Left, Right };
        }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionBinary"/> class
        /// </summary>
        protected TokenExpressionBinary(TokenExpression left, TokenExpression right)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            this.Left = left;
            this.Right = right;
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? obj)
        {
            var other = obj as TokenExpressionBinary;
            if (ReferenceEquals(other, null)) return false;
            if (this.GetType() != other.GetType()) return false;
            return this.Left.Equals(other.Left) && this.Right.Equals(other.Right);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? other)
        {
            return other is TokenExpressionBinary t && Equals(t);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Right);
        }
    }
}
