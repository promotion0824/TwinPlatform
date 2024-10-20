using System;
using System.Collections.Generic;

namespace Willow.Expressions
{
    /// <summary>
    /// A unary expression has a single argument
    /// </summary>
    public abstract class TokenExpressionUnary : TokenExpression
    {
        /// <summary>
        /// Get the child expression
        /// </summary>
        public TokenExpression Child { get; }

        public override IEnumerable<TokenExpression> GetChildren()
        {
            return new TokenExpression[] { Child };
        }

        /// <summary>
        /// Creates a new unary expression
        /// </summary>
        protected TokenExpressionUnary(TokenExpression child)
        {
            if (child is null) throw new ArgumentNullException(nameof(child));
            this.Child = child;
        }

        public override bool Equals(TokenExpression? obj)
        {
            if (obj is not TokenExpressionUnary other) return false;
            if (this.GetType() != other.GetType()) return false;
            return this.Child.Equals(other.Child);
        }

        public override int GetHashCode()
        {
            return this.Child.GetHashCode() * -1;
        }

        public override bool Equals(object? other)
        {
            return other is TokenExpressionUnary t && Equals(t);
        }
    }
}
