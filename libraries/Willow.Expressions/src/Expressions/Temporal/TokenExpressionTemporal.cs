using System;
using System.Collections.Generic;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// An abstract token expression for temporal expressions
    /// </summary>
    public class TokenExpressionTemporal : TokenExpression
    {
        /// <summary>
        /// An optional time period for the expression
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Get the child expression
        /// </summary>
        public TokenExpression Child { get; }

        /// <summary>
        /// An optional time period for the expression
        /// </summary>
        public TokenExpression? TimePeriod { get; }

        /// <summary>
        /// An optional time period for the expression
        /// </summary>
        public TokenExpression? TimeFrom { get; }

        public override IEnumerable<TokenExpression> GetChildren()
        {
            yield return Child;
            if (TimePeriod is not null)
            {
                yield return TimePeriod;
            }
            if (TimeFrom is not null)
            {
                yield return TimeFrom;
            }
        }

        /// <summary>
        /// An optional unit of measure
        /// </summary>
        public TokenExpression? UnitOfMeasure { get; set; }

        /// <summary>
        /// Creates a new unary expression
        /// </summary>
        public TokenExpressionTemporal(string functionName, TokenExpression child, TokenExpression? timePeriod = null, TokenExpression? timeFrom = null, TokenExpression? unitOfMeasure = null)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                throw new ArgumentException($"'{nameof(functionName)}' cannot be null or empty.", nameof(functionName));
            }

            this.FunctionName = functionName;
            this.Child = child ?? throw new ArgumentNullException(nameof(child));
            this.TimePeriod = timePeriod;
            this.TimeFrom = timeFrom;
            this.UnitOfMeasure = unitOfMeasure;
        }

        public override bool Equals(TokenExpression? obj)
        {
            if (obj is not TokenExpressionTemporal other) return false;
            if (this.GetType() != other.GetType()) return false;
            return this.Child.Equals(other.Child);
        }

        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Get the Type that this Expression represents (if known)
        /// </summary>
        public override Type Type => typeof(double);

        /// <summary>
        /// Accepts a visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override int GetHashCode()
        {
            return this.Child.GetHashCode() * -1;
        }

        public override bool Equals(object? other)
        {
            return other is TokenExpressionTemporal t && Equals(t);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            if (TimePeriod is not null)
            {
                return $"{FunctionName}({this.Child}, {this.TimePeriod})";
            }

            if (TimePeriod is not null && TimeFrom is not null)
            {
                return $"{FunctionName}({this.Child}, {this.TimePeriod}, {this.TimeFrom})";
            }

            if (TimePeriod is not null && TimeFrom is not null && UnitOfMeasure is not null)
            {
                return $"{FunctionName}({this.Child}, {this.TimePeriod}, {this.TimeFrom}, {this.UnitOfMeasure})";
            }

            return $"{FunctionName}({this.Child})";
        }
    }
}
