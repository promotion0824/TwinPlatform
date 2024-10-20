using System;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// Apply a named function
    /// </summary>
    public class TokenExpressionFunctionCall : TokenExpressionNary
    {
        public override int Priority => 1100;  // higher than property access and most

        protected override bool IsUnordered { get { return false; } }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Get the function name
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionFunctionCall"/> class
        /// </summary>
        public TokenExpressionFunctionCall(string functionName, Type returnType, params TokenExpression[] children)
            : base(children)
        {
            this.FunctionName = functionName;
            // Type of return argument is not known at this point
            this.Type = returnType;
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
            return $"{this.FunctionName}({string.Join(", ", this.Children.Select(x => x.ToString()))})";
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? obj)
        {
            var other = obj as TokenExpressionFunctionCall;
            if (ReferenceEquals(other, null)) return false;
            return this.FunctionName.Equals(other.FunctionName)
                && this.Children.Length == other.Children.Length
                && this.Children.Zip(other.Children, (x, y) => x.Equals(y)).All(x => x);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? other)
        {
            return other is TokenExpressionFunctionCall t && Equals(t);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            // Weak, but OK
            return this.FunctionName.GetHashCode();
        }
    }
}
