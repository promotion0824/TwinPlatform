using System.Collections.Generic;

namespace Willow.Expressions
{
    /// <summary>
    /// An equality comparer for TokenExpressions (relies on TokenExpression itself being IEquatable
    /// </summary>
    public class TokenExpressionEqualityComparer : IEqualityComparer<TokenExpression>
    {
        /// <summary>
        /// Compare two TokenExpressions for equality
        /// </summary>
        public bool Equals(TokenExpression? x, TokenExpression? y)
        {
            return (x is null && y is null) || (x is TokenExpression && x.Equals(y));
        }

        /// <summary>
        /// Get the hashcode
        /// </summary>
        public int GetHashCode(TokenExpression obj)
        {
            return obj.GetHashCode();
        }
    }
}
