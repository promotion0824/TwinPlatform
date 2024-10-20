namespace Willow.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// FilterOperations.
    /// </summary>
    public static class FilterOperations
    {
        private static Dictionary<string, Func<Expression, Expression, object, Expression>> expressions = new Dictionary<string, Func<Expression, Expression, object, Expression>>
        {
            { FilterOperators.EqualsLiteral, (member, constant, propertyValue) => Expression.Equal(member, constant) },
            { FilterOperators.EqualsShort, (member, constant, propertyValue) => Expression.Equal(member, constant) },
            { FilterOperators.Is, (member, constant, propertyValue) => Expression.Equal(member, constant) },
            { FilterOperators.IsNot, (member, constant, propertyValue) => Expression.NotEqual(member, constant) },
            { FilterOperators.NotEquals, (member, constant, propertyValue) => Expression.NotEqual(member, constant) },
            { FilterOperators.GreaterThan, (member, constant, propertyValue) => Expression.GreaterThan(member, constant) },
            { FilterOperators.GreaterThanOrEqual, (member, constant, propertyValue) => Expression.GreaterThanOrEqual(member, constant) },
            { FilterOperators.LessThan, (member, constant, propertyValue) => Expression.LessThan(member, constant) },
            { FilterOperators.LessThanOrEqual, (member, constant, propertyValue) => Expression.LessThanOrEqual(member, constant) },
            { FilterOperators.StartsWith, (member, constant, propertyValue) => Expression.Call(member, startsWithMethod, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase)) },
            { FilterOperators.EndsWith, (member, constant, propertyValue) => Expression.Call(member, endsWithMethod, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase)) },
            { FilterOperators.Contains, (member, constant, propertyValue) => Contains(member, constant, propertyValue) },
            { FilterOperators.NotContains, (member, constant, propertyValue) => NotContains(member, constant, propertyValue) },
            { FilterOperators.Like, (member, constant, propertyValue) => Like(member, constant, propertyValue) },
            { FilterOperators.ContainedIn, (member, constant, propertyValue) => ContainedIn(member, constant, propertyValue) },
            { FilterOperators.Any, (member, constant, propertyValue) => Any(member, constant, propertyValue) },
            { FilterOperators.IsNull, (member, constant, propertyValue) => Expression.Equal(member, Expression.Constant(null)) },
            { FilterOperators.IsNotNull, (member, constant, propertyValue) => Expression.NotEqual(member, Expression.Constant(null)) },
            { FilterOperators.IsEmpty, (member, constant, propertyValue) => Expression.Equal(member, Expression.Constant(string.Empty)) },
            { FilterOperators.IsNotEmpty, (member, constant, propertyValue) => Expression.NotEqual(member, Expression.Constant(string.Empty)) },
            { FilterOperators.In, (member, constant, propertyValue) => ContainedIn(member, constant, propertyValue) },
            { FilterOperators.NotIn, (member, constant, propertyValue) => NotContainedIn(member, constant, propertyValue) },
        };

        private static MethodInfo trimMethod = typeof(string).GetMethod("Trim", new Type[0]);
        private static MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", new Type[0]);
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string), typeof(StringComparison) });

        /// <summary>
        /// Invoke the operation.
        /// </summary>
        /// <param name="operation">The operation to invoke.</param>
        /// <param name="member">The expression to use in the invocation.</param>
        /// <param name="constant">The constant expression.</param>
        /// <param name="propertyValue">The property value to evaluate.</param>
        /// <returns>The resulting expression.</returns>
        /// <exception cref="NotSupportedException">Thrown if the operation is null.</exception>
        public static Expression Invoke(this string operation, Expression member, Expression constant, object propertyValue)
        {
            if (expressions.ContainsKey(operation))
            {
                var expression = expressions[operation].Invoke(member, constant, propertyValue);

                if (operation != FilterOperators.IsNull)
                {
                    if (member.Type == typeof(string))
                    {
                        if (constant.Type == typeof(string))
                        {
                            expression = expressions[operation].Invoke(
                                Expression.Call(Expression.Call(member, trimMethod), toLowerMethod),
                                Expression.Call(Expression.Call(constant, trimMethod), toLowerMethod),
                                propertyValue);
                        }
                        else
                        {
                            expression = expressions[operation].Invoke(Expression.Call(Expression.Call(member, trimMethod), toLowerMethod), constant, propertyValue);
                        }
                    }

                    if (member.Type.IsNullable() || member.Type == typeof(string) || member.Type == typeof(object))
                    {
                        expression = Expression.AndAlso(Expression.NotEqual(member, Expression.Constant(null, typeof(object))), expression);
                    }
                }

                return expression;
            }

            throw new NotSupportedException($"Filter operation {operation}");
        }

        private static Expression Contains(Expression member, Expression constant, object propertyValue)
        {
            var method = typeof(string).GetMethod("Contains", [typeof(string)]);
            return Expression.Call(member, method, constant);
        }
        private static Expression NotContains(Expression member, Expression constant, object propertyValue)
        {
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            return Expression.Not(Expression.Call(member, method, constant, Expression.Constant(StringComparison.OrdinalIgnoreCase)));
        }
        private static Expression Like(Expression member, Expression constant, object propertyValue)
        {
            // https://stackoverflow.com/questions/52210402/make-dynamic-expression-of-ef-core-like-function
            var method = typeof(DbFunctionsExtensions).GetMethod("Like",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(DbFunctions), typeof(string), typeof(string) },
                null);

            var original = propertyValue.ToString().Trim().ToLower();
            var wrapped = Expression.Constant($"%{original}%", typeof(string));

            return Expression.Call(method, Expression.Constant(null, typeof(DbFunctions)), member, wrapped);
        }

        private static Expression ContainedIn(Expression member, Expression constant, object propertyValue)
        {
            var method = propertyValue?.GetType().GetMethod("Contains");
            return Expression.Call(constant, method, member);
        }
        private static Expression NotContainedIn(Expression member, Expression constant, object propertyValue)
        {
            var method = propertyValue?.GetType().GetMethod("Contains");
            return Expression.Not(Expression.Call(constant, method, member));
        }
        private static Expression Any(Expression member, Expression lambda, object propertyValue)
        {
            var method = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "Any" && m.GetParameters().Count() == 2);
            method = method.MakeGenericMethod(propertyValue as Type);
            return Expression.Call(method, member, lambda);
        }
    }
}
