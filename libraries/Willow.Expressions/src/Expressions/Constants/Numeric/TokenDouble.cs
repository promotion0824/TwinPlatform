using System;
using System.Globalization;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A number 1, 2, ...
    ///
    /// Hierarchy
    ///     TokenNumber : TokenDouble : TokenLong : TokenInt : TokenIntWordForm
    ///     TokenNumber : TokenDouble : TokenPercentage
    ///     TokenNumber : TokenDouble : TokenFraction
    ///
    /// So if we get a better derived one first we are done
    ///
    ///
    /// </summary>
    public class TokenDouble : TokenExpressionConstant, IEquatable<TokenDouble>, IComparable<TokenDouble>
    {
        /// <summary>
        /// Gets the double value
        /// </summary>
        public double ValueDouble { get; }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(double);

        /// <summary>
        /// Gets the value double
        /// </summary>
        public double ValueInCanonicalUnit => this.ValueDouble;

        /// <summary>
        /// Describe the value in text
        /// </summary>
        public virtual string DoDescribe(bool metric)
        {
            string result = this.ValueDouble.ToString(CultureInfo.InvariantCulture);
            if (result.EndsWith(".0")) return result.Replace(".0", "");
            return result;
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return System.Convert.ToDecimal(this.ValueDouble).ToString();
        }

        /// <summary>
        /// Creates a new instance of the TokenDouble class
        /// </summary>
        public TokenDouble(double value, string text)
            : base(value)
        {
            this.ValueDouble = value;
            this.Text = text;
        }

        /// <summary>
        /// Creates a new instance of the TokenDouble class
        /// </summary>
        public TokenDouble(double value) : this(value, value.ToString(CultureInfo.InvariantCulture))
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
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? obj)
        {
            return obj is TokenDouble t && Equals(t);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is TokenDouble t && Equals(t);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public bool Equals(TokenDouble? other)
        {
            if (other is null) return false;
            if (this.Value is null) return other.Value is null;
            return this.ValueDouble.Equals(other.ValueDouble);
        }

        /// <summary>
        /// Compare
        /// </summary>
        public int CompareTo(TokenDouble? other)
        {
            if (other is null) return 1;
            return this.ValueDouble.CompareTo(other.ValueDouble);
        }

        /// <summary>
        /// GetHashcode
        /// </summary>
        public override int GetHashCode()
        {
            if (this.Value is null) return 1;
            return this.ValueDouble.GetHashCode();
        }
    }
}
