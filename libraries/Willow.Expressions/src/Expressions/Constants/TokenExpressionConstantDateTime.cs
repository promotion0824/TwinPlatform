using System;
using System.Globalization;
using Willow.Expressions.Visitor;
using Willow.Units;

namespace Willow.Expressions
{
    /// <summary>
    /// A constant date time TokenExpression
    /// </summary>
    public class TokenExpressionConstantDateTime : TokenExpressionConstant
    {
        /// <summary>
        /// Gets the DateTime
        /// </summary>
        public DateTime ValueDateTime { get; }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(DateTime);

        /// <summary>
        /// Creates a new <see cref="TokenExpressionConstantDateTime"/>
        /// </summary>
        public TokenExpressionConstantDateTime(DateTime value) : base(value)
        {
            this.ValueDateTime = value;
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
            return $"'{this.Value.ToString(CultureInfo.InvariantCulture)}'";
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? other)
        {
            var constant = other as TokenExpressionConstantDateTime;
            if (constant is null) return false;
            if (this.Value is null) return constant.Value is null;
            return this.ValueDateTime.Equals(constant.ValueDateTime);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is TokenExpressionConstantDateTime t && Equals(t);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            if (this.Value is null) return 1;
            return this.Value.GetHashCode();
        }
    }
}
