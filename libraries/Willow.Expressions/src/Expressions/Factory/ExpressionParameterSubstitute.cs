using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Willow.Expressions
{
    /// <summary>
    /// An ExpressionVisitor for parameter substitution
    /// </summary>
    internal class ExpressionParameterSubstitute : ExpressionVisitor
    {
        private readonly ParameterExpression from;
        private readonly Expression to;

        /// <summary>
        /// Creates a new instance of the <see cref="ExpressionParameterSubstitute"/> visitor
        /// </summary>
        public ExpressionParameterSubstitute(ParameterExpression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Visit a Lambda Expression
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.Parameters.All(p => p != this.from))
                return node;

            // We need to replace the `from` parameter, but in its place we need the `to` parameter(s)
            // e.g. F<DateTime,Bool> subst F<Source,DateTime> => F<Source,bool>
            // e.g. F<DateTime,Bool> subst F<Source1,Source2,DateTime> => F<Source1,Source2,bool>

            if (to is LambdaExpression toLambda)
            {
                var substituteParameters = toLambda?.Parameters ?? Enumerable.Empty<ParameterExpression>();

                ReadOnlyCollection<ParameterExpression> substitutedParameters
                    = new ReadOnlyCollection<ParameterExpression>(node.Parameters
                        .SelectMany(p => p == this.from ? substituteParameters : Enumerable.Repeat(p, 1))
                        .ToList());

                var updatedBody = this.Visit(node.Body); // which will convert parameters to 'to'
                return Expression.Lambda(updatedBody, substitutedParameters);
            }
            else
            {
                // to is not a lambda expression so simple substitution can work
                ReadOnlyCollection<ParameterExpression> substitutedParameters
                    = new ReadOnlyCollection<ParameterExpression>(node.Parameters
                        .Where(p => p != this.from)
                        .ToList());

                var updatedBody = this.Visit(node.Body); // which will convert parameters to 'to'

                if (substitutedParameters.Any())
                    return Expression.Lambda(updatedBody, substitutedParameters);
                else
                    return updatedBody;
            }
        }

        /// <summary>
        /// Visit a ParameterExpression
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            var toLambda = to as LambdaExpression;
            if (node == from) return toLambda?.Body ?? to;
            return base.VisitParameter(node);
        }

        // This is not available to override
        //protected override Expression VisitUnary(Expression node)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
