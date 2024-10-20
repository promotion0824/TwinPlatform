using System;
using System.Collections.Generic;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// A non-constant expression (not used anywhere)
    /// </summary>
    public abstract class TokenExpressionFormula : TokenExpression
    {
    }

    /// <summary>
    /// A lambda expression
    /// </summary>
    public abstract class TokenExpressionLambda : TokenExpression
    {
    }

    /// <summary>
    /// A parameter for a lambda expression
    /// </summary>
    public class TokenExpressionParameter : TokenExpression
    {
        public override IEnumerable<TokenExpression> GetChildren()
        {
            return Array.Empty<TokenExpression>();
        }

        /// <summary>
        /// Gets the priority for precedence
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Gets the dot net type of this expression
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Name of parameter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new <see cref="TokenExpressionParameter"/>
        /// </summary>
        public TokenExpressionParameter(Type type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        /// <summary>
        /// Acccepts an expression visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public bool Equals(TokenExpressionParameter? other)
        {
            return other is TokenExpressionParameter
                && other.Type == this.Type && other.Name == this.Name;
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? other)
        {
            return other is TokenExpressionParameter t && Equals(t);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return $"Parameter({this.Name})";
        }
    }
}
