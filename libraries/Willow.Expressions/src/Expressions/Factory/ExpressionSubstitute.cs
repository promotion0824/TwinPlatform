using System.Linq.Expressions;

namespace Willow.Expressions
{
    /// <summary>
    /// A LINQ ExpressionVisitor for substitution
    /// </summary>
    public class ExpressionSubstitute : ExpressionVisitor
    {
        private readonly Expression from;

        private readonly Expression to;

        /// <summary>
        /// Creates a new <see cref="ExpressionSubstitute"/> visitor
        /// </summary>
        public ExpressionSubstitute(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override Expression Visit(Expression? node)
        {
            if (node == from) return to;
            return base.Visit(node!);
        }

        /// <summary>
        /// Visit
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == from) return to;
            return base.VisitParameter(node);
        }
    }
}
