using System;
using System.Collections.Generic;
using System.Linq;

namespace Willow.Expressions
{
    /// <summary>
    /// And and Or can take an arbitrary number of parameters which simplifies many things
    /// when projecting to database queries
    /// </summary>
    public abstract class TokenExpressionNary : TokenExpressionFormula
    {
        /// <summary>
        /// Gets the children TokenExpressions
        /// </summary>
        public TokenExpression[] Children { get; }

        public override IEnumerable<TokenExpression> GetChildren()
        {
            return Children;
        }

        /// <summary>
        /// Is this Nary sensitive to the order of its elements or not (for Equals)
        /// </summary>
        protected abstract bool IsUnordered { get; }

        /// <summary>
        /// Creates a new <see cref="TokenExpressionNary"/>
        /// </summary>
        protected TokenExpressionNary(params TokenExpression[] children)
        {
            if (children is null) throw new ArgumentNullException(nameof(children));
            if (children.Any(c => c is null)) throw new ArgumentException("one of the children was null", nameof(children));
            this.Children = children;
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? obj)
        {
            var other = obj as TokenExpressionNary;
            if (ReferenceEquals(other, null)) return false;
            if (this.GetType() != other.GetType()) return false;

            if (this.IsUnordered)
                return IsEqual(this.Children, other.Children);
            else
                return this.Children.SequenceEqual(other.Children);
        }

        /// <summary>
        /// Tests that two lists are Equal (independeny of order)
        /// </summary>
        /// <remarks>
        /// Needs to move to a Utility class
        /// </remarks>
        public static bool IsEqual<T>(IList<T> x1, IList<T> x2)
            where T : notnull
        {
            if (x1.Count != x2.Count) return false;

            var x1Elements = new Dictionary<T, int>();

            foreach (var item in x1)
            {
                int n = 0;
                x1Elements.TryGetValue(item, out n);
                x1Elements[item] = n + 1;
            }

            foreach (var item in x2)
            {
                int n = 0;
                x1Elements.TryGetValue(item, out n);  // cannot just check bool value because it's there even if it's zero!
                if (n == 0) return false;             // this element was in x2 but not x1
                else x1Elements[item] = n - 1;
            }

            // If we successfully managed to remove n items from a list of n then all values must be zero
            return true;
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? other)
        {
            return other is TokenExpressionNary t && Equals(t);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            // not great, but good enough - sum children
            // cannot use Linq Sum because it is checked
            unchecked
            {
                // NOTE: THIS IS ORDER INSENSITIVE - DELIBERATELY
                return this.Children?.Aggregate(0, (sum, obj) => sum + obj.GetHashCode()) ?? 0;
            }
        }
    }
}
