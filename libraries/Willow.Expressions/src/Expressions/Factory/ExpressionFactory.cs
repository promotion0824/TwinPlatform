using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Willow.Expressions
{
    /// <summary>
    /// A factory class for creating Expressions
    /// </summary>
    public static class ExpressionFactory
    {
        /// <summary>
        /// Create an expression for dt >= start and dt &lt; end
        /// where start and end are constant values
        /// </summary>
        public static Expression<Func<T, bool>> BetweenExpression<T>(T start, T end)
        {
            // dt => dt >= start && dt < end;
            var parameter = Expression.Parameter(typeof(T), "dt");
            var binaryExpression1 = Expression.GreaterThanOrEqual(parameter, Expression.Constant(start));
            var binaryExpression2 = Expression.LessThan(parameter, Expression.Constant(end));
            var binaryExpressionAnd = Expression.AndAlso(binaryExpression1, binaryExpression2);
            return (Expression<Func<T, bool>>)Expression.Lambda(binaryExpressionAnd, parameter);
        }

        /// <summary>
        /// Create an expression for dt >= start
        /// where start is a constant value
        /// </summary>
        public static Expression<Func<T, bool>> After<T>(T start)
        {
            // dt => dt >= start;
            var parameter = Expression.Parameter(typeof(T), "dt");
            var binaryExpression1 = Expression.GreaterThanOrEqual(parameter, Expression.Constant(start));
            return (Expression<Func<T, bool>>)Expression.Lambda(binaryExpression1, parameter);
        }

        /// <summary>
        /// Create an expression for dt &lt; end
        /// where end is a constant value
        /// </summary>
        public static Expression<Func<T, bool>> Before<T>(T end)
        {
            // dt => dt < end;
            var parameter = Expression.Parameter(typeof(T), "dt");
            var binaryExpression2 = Expression.LessThan(parameter, Expression.Constant(end));
            return (Expression<Func<T, bool>>)Expression.Lambda(binaryExpression2, parameter);
        }

        /// <summary>
        /// Create an expression for equal where value is a constant
        /// </summary>
        public static Expression<Func<T, bool>> Equal<T>(T value)
        {
            var parameter = Expression.Parameter(typeof(T), "v");
            var binaryExpression2 = Expression.Equal(parameter, Expression.Constant(value));
            return (Expression<Func<T, bool>>)Expression.Lambda(binaryExpression2, parameter);
        }

        /// <summary>
        /// Get an expression that scales a value and adds an offset
        /// but simplified for cases where scale is 1 and/or offset is zero
        /// </summary>
        public static Expression<Func<double, double>> ForScaleAndOffset(double scale, double offset)
        {
            var p = Expression.Parameter(typeof(double), "p");
            if (offset.Equals(0.0))
            {
                return scale.Equals(1.0) ?
                    Expression.Lambda<Func<double, double>>(p, p) :
                    Expression.Lambda<Func<double, double>>(Expression.Multiply(p, Expression.Constant(scale)), p);
            }
            else
            {
                return Expression.Lambda<Func<double, double>>(Expression.Add(Expression.Multiply(p, Expression.Constant(scale)), Expression.Constant(offset)), p);
            }
        }

        /// <summary>
        /// Get an inverse expression that scales a value and adds an offset
        /// but simplified for cases where scale is 1 and/or offset is zero
        /// </summary>
        public static Expression<Func<double, double>> ForInverseScaleAndOffset(double scale, double offset)
        {
            var p = Expression.Parameter(typeof(double), "p");
            if (offset.Equals(0.0))
            {
                return scale.Equals(1.0) ?
                    Expression.Lambda<Func<double, double>>(p, p) :
                    Expression.Lambda<Func<double, double>>(Expression.Divide(p, Expression.Constant(scale)), p);
            }
            else
            {
                return Expression.Lambda<Func<double, double>>(Expression.Divide(Expression.Subtract(p, Expression.Constant(offset)), Expression.Constant(scale)), p);
            }
        }

        /// <summary>
        /// Partial application of first parameter
        /// </summary>
        public static Expression<Func<U, V, W, X, bool>> Apply<T, U, V, W, X>(this Expression<Func<T, U, V, W, X, bool>> input, T value)
        {
            var swap = new ExpressionSubstitute(input.Parameters[0], Expression.Constant(value));
            var lambda = Expression.Lambda<Func<U, V, W, X, bool>>(swap.Visit(input.Body), input.Parameters[1], input.Parameters[2], input.Parameters[3], input.Parameters[4]);
            return lambda;
        }

        /// <summary>
        /// Partial application of first parameter
        /// </summary>
        public static Expression<Func<U, V, W, bool>> Apply<T, U, V, W>(this Expression<Func<T, U, V, W, bool>> input, T value)
        {
            var swap = new ExpressionSubstitute(input.Parameters[0], Expression.Constant(value));
            var lambda = Expression.Lambda<Func<U, V, W, bool>>(swap.Visit(input.Body), input.Parameters[1], input.Parameters[2], input.Parameters[3]);
            return lambda;
        }

        /// <summary>
        /// Partial application of first parameter
        /// </summary>
        public static Expression<Func<U, V, bool>> Apply<T, U, V>(this Expression<Func<T, U, V, bool>> input, T value)
        {
            var swap = new ExpressionSubstitute(input.Parameters[0], Expression.Constant(value));
            var lambda = Expression.Lambda<Func<U, V, bool>>(swap.Visit(input.Body), input.Parameters[1], input.Parameters[2]);
            return lambda;
        }

        /// <summary>
        /// Partial application of first and second parameter
        /// </summary>
        public static Expression<Func<V, bool>> Apply<T, U, V>(this Expression<Func<T, U, V, bool>> input, T value, U secondValue)
        {
            var swap1 = new ExpressionSubstitute(input.Parameters[0], Expression.Constant(value));
            var swap2 = new ExpressionSubstitute(input.Parameters[1], Expression.Constant(secondValue));

            var firstApplied = swap1.Visit(input.Body);
            var secondApplied = swap2.Visit(firstApplied);

            var lambda = Expression.Lambda<Func<V, bool>>(secondApplied, input.Parameters[2]);
            return lambda;
        }

        /// <summary>
        /// Partially apply a value (or function) to an expression
        /// </summary>
        public static Expression Apply(this LambdaExpression input, LambdaExpression value)
        {
            if (!input.Parameters.Any()) return Expression.Lambda(input.Body, value.Parameters);
            var swap = new ExpressionParameterSubstitute(input.Parameters[0], value);
            var lambda = swap.Visit(input);
            //var lambda = Expression.Lambda(swap.Visit(input.Body), input.Parameters.Skip(1).Concat(value.Parameters).ToArray());
            return lambda;
        }

        /// <summary>
        /// Partially apply a value (or function) to an expression
        /// </summary>
        public static Expression Apply(this LambdaExpression input, Expression value)
        {
            if (!input.Parameters.Any()) throw new ArgumentException("input has no parameters to substitute");
            var swap = new ExpressionParameterSubstitute(input.Parameters[0], value);
            var lambda = swap.Visit(input);
            //var lambda = Expression.Lambda(swap.Visit(input.Body), input.Parameters.Skip(1).Concat(value.Parameters).ToArray());
            return lambda;
        }

        /// <summary>
        /// Partially apply a value to an expression
        /// </summary>
        public static Expression<Func<U, bool>> Apply<T, U>(this Expression<Func<T, U, bool>> input, T value)
        {
            //return input.Apply(Expression.Constant(value))
            var swap = new ExpressionSubstitute(input.Parameters[0], Expression.Constant(value));
            var lambda = Expression.Lambda<Func<U, bool>>(swap.Visit(input.Body), input.Parameters[1]);
            return lambda;
        }
    }
}
