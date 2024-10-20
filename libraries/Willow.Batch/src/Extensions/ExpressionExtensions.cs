#nullable enable

namespace Willow.Batch
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Extension methods for expressions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Applies an AndAlso operator to 2 expressions.
        /// </summary>
        /// <typeparam name="T">The type of the input parameter.</typeparam>
        /// <param name="e1">The first expression.</param>
        /// <param name="e2">The second expression.</param>
        /// <returns>An expression that combines the two input expressions.</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            var swapVisitor = new SwapVisitor(e1.Parameters[0], e2.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(swapVisitor.Visit(e1.Body)!, e2.Body), e2.Parameters);
        }

        /// <summary>
        /// Applies an OrElse operator to 2 expressions.
        /// </summary>
        /// <typeparam name="T">The type of the input parameter.</typeparam>
        /// <param name="e1">The first expression.</param>
        /// <param name="e2">The second expression.</param>
        /// <returns>An expression that combines the two input expressions.</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            var swapVisitor = new SwapVisitor(e1.Parameters[0], e2.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(swapVisitor.Visit(e1.Body)!, e2.Body), e2.Parameters);
        }

        /// <summary>
        /// Returns a true expression.
        /// </summary>
        /// <typeparam name="T">The type of the input object.</typeparam>
        /// <returns>A True expression.</returns>
        public static Expression<Func<T, bool>> True<T>()
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), Expression.Parameter(typeof(T), "_"));
        }

         /// <summary>
        /// Get the member expression even when the property is nested
        /// https://www.codeproject.com/Articles/1079028/Build-Lambda-Expressions-Dynamically.
        /// </summary>
        /// <param name="param">The expression to get the prperty from.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The member expression.</returns>
        public static Expression? GetProperty(this Expression param, string propertyName)
        {
            if (propertyName.Contains("."))
            {
                var childName = propertyName.Substring(0, propertyName.IndexOf("."));
                var childParam = Expression.Property(param, childName);
                return GetProperty(childParam, propertyName.Substring(propertyName.IndexOf(".") + 1));
            }

            try
            {
                return Expression.Property(param, propertyName);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Re-writes the internals of a combined expression to change its parameters.
    /// </summary>
    /// <remarks>
    /// Useful during AndAlso and OrElse combos https://stackoverflow.com/questions/10613514/how-can-i-combine-two-lambda-expressions-without-using-invoke-method/10613631#10613631.
    /// </remarks>
    public class SwapVisitor : ExpressionVisitor
    {
        private readonly Expression from;
        private readonly Expression to;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapVisitor"/> class.
        /// Creates The visitor with the two expressions to be combined.
        /// </summary>
        /// <param name="from">The expression to replace.</param>
        /// <param name="to">The expression to replace with.</param>
        public SwapVisitor(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Rewrite the nodes.
        /// </summary>
        /// <param name="node">The node to rewrite.</param>
        /// <returns>The rewritten expression.</returns>
        public override Expression? Visit(Expression? node)
        {
            return node == from ? to : base.Visit(node);
        }
    }
}
