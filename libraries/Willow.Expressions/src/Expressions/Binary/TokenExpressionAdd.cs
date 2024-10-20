using System;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A TokenExpression for adding two values (doubles or strings)
    /// </summary>
    public class TokenExpressionAdd : TokenExpressionNary
    {
        private readonly Lazy<Type> getType;

        public override int Priority => 10;

        protected override bool IsUnordered { get => true; }

        public override bool Commutative => true;

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => getType.Value;

        private Type CalculateType()
        {
            if (this.Children.Length == 2
                && this.Children.Any(c => c.Type == typeof(DateTime))
                && this.Children.Any(c => c.Type == typeof(TimeSpan)))
            {
                return typeof(DateTime);
            }

            if (this.Children.Length == 2
                && this.Children.Any(c => c.Type == typeof(DateTimeOffset))
                && this.Children.Any(c => c.Type == typeof(TimeSpan)))
            {
                return typeof(DateTimeOffset);
            }

            if (this.Children.Length == 1)
            {
                return this.Children.First().Type;
            }

            if (this.Children.Any(c => c.Type == typeof(string)))
            {
                return typeof(string);
            }

            // Else assume they are all double
            return typeof(double);
        }

        /// <summary>
        /// Creates a new instance of the TokenExpressionAdd class
        /// </summary>
        public TokenExpressionAdd(params TokenExpression[] children)
            : base(children)
        {
            getType = new(() => CalculateType());
        }

        /// <summary>
        /// Creates a new instance of the TokenExpressionAdd class
        /// </summary>
        public TokenExpressionAdd(TokenExpression left, double right)
            : this(left, TokenExpressionConstant.Create(right))
        {
        }

        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        public override string ToString()
        {
            return $"({string.Join(" + ", this.Children.Select(c => c.ToString()))})";
        }

        /// <summary>
        /// Create an Add Expression or Zero if none
        /// </summary>
        public static TokenExpression Create(TokenExpression[] children)
        {
            if (!children.Any()) return new TokenDouble(0.0);
            else if (children.Length == 1) return children.First();
            else return new TokenExpressionAdd(children.ToArray());
        }
    }
}
