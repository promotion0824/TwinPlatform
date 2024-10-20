using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Willow.Common;

namespace WorkflowCore
{
    public delegate IOrderedQueryable<TSource> OrderByDelegate<TSource, TKey>(IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);
    public delegate IOrderedQueryable<TSource> ThenByDelegate<TSource, TKey>(IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

    public static class IQueryableExtensions1
    {
        private static object Private(this object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        private static T Private<T>(this object obj, string privateField) => (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, KeyValuePair<string, string> field, OrderByDelegate<T, object> orderByDelegate)
        {
            var keys = field.Key.Split('.');

            if (typeof(T).GetCustomAttributes(typeof(TableAttribute), true).Any())
            {
                if (keys.Length == 1)
                {
                    return orderByDelegate(query, x => EF.Property<object>(x, keys[0]));
                }
                else if (keys.Length == 2)
                {
                    return orderByDelegate(query, x => EF.Property<object>(EF.Property<object>(x, keys[0]), keys[1]));
                }
                else if (keys.Length == 3)
                {
                    return orderByDelegate(query, x => EF.Property<object>(EF.Property<object>(EF.Property<object>(x, keys[0]), keys[1]), keys[2]));
                }

                throw new ArgumentException().WithData(new { OrderByFieldId = field.Key });
            }
            else
            {
                return orderByDelegate(query, x => typeof(T).GetPropertyValue(keys, x)); 
            }
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, KeyValuePair<string, string> field)
        {
            return field.Value.Equals("desc", StringComparison.OrdinalIgnoreCase) ? OrderBy(query, field, Queryable.OrderByDescending) : OrderBy(query, field, Queryable.OrderBy);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, KeyValuePair<string, string> field, ThenByDelegate<T, object> orderByDelegate)
        {
            var keys = field.Key.Split('.');

            if (typeof(T).GetCustomAttributes(typeof(TableAttribute), true).Any())
            {
                if (keys.Length == 1)
                {
                    return orderByDelegate(query, x => EF.Property<object>(x, keys[0]));
                }
                else if (keys.Length == 2)
                {
                    return orderByDelegate(query, x => EF.Property<object>(EF.Property<object>(x, keys[0]), keys[1]));
                }
                else if (keys.Length == 3)
                {
                    return orderByDelegate(query, x => EF.Property<object>(EF.Property<object>(EF.Property<object>(x, keys[0]), keys[1]), keys[2]));
                }

                throw new ArgumentException().WithData(new { OrderByFieldId = field.Key });
            }
            else
            {
                return orderByDelegate(query, x => typeof(T).GetPropertyValue(keys, x));
            }
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, KeyValuePair<string, string> field)
        {
            return field.Value.Equals("desc", StringComparison.OrdinalIgnoreCase) ? ThenBy(query, field, Queryable.ThenByDescending) : ThenBy(query, field, Queryable.ThenBy);
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string csv)
        {
            var orderBy = csv.CsvToDictionary();

            IOrderedQueryable<T> orderedQuery = null;

            foreach (var field in orderBy)
            {
                if (typeof(T).GetProperty(field.Key.Split('.')) == null)
                {
                    throw new ArgumentException().WithData(new { OrderByField = field.Key });
                }

                orderedQuery = (orderedQuery == null) ? query.OrderBy(field) : orderedQuery.ThenBy(field);
            }

            return orderedQuery ?? query;
        }

        public static PropertyInfo GetProperty(this Type type, string[] nestedKeys)
        {
            if (nestedKeys != null && nestedKeys.Length > 0)
            {
                var p = type.GetProperty(nestedKeys[0]);

                return (nestedKeys.Length > 1) ? p.PropertyType.GetProperty(nestedKeys.Skip(1).ToArray()) : p;
            }

            return null;
        }

        public static object GetPropertyValue(this Type type, string[] nestedKeys, object obj)
        {
            if (nestedKeys != null && nestedKeys.Length > 0)
            {
                var p = type.GetProperty(nestedKeys[0]);

                return (nestedKeys.Length > 1) ? p.PropertyType.GetPropertyValue(nestedKeys.Skip(1).ToArray(), p.GetValue(obj, null)) : p.GetValue(obj, null); 
            }

            return null;
        }
    }
}
