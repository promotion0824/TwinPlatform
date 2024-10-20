using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Willow.Rules.DTO;

namespace Willow.Rules.Repository;

public static class ExpressionExtensions
{
	class ReplaceVisitor : ExpressionVisitor
	{
		private readonly ParameterExpression from, to;
		public ReplaceVisitor(ParameterExpression from, ParameterExpression to)
		{
			this.from = from;
			this.to = to;
		}
		protected override Expression VisitParameter(ParameterExpression node)
		{
			return node == from ? to : base.VisitParameter(node);
		}
	}

	public static Expression Replace(this Expression expression,
		ParameterExpression searchEx, ParameterExpression replaceEx)
	{
		return new ReplaceVisitor(searchEx, replaceEx).Visit(expression);
	}


	public static Expression<Func<T, bool>> CombineExpressions<T>(
		IEnumerable<Expression<Func<T, bool>>> expressions)
	{
		if (expressions is null || expressions.Count() == 0)
		{
			return t => true;
		}
		ParameterExpression param = Expression.Parameter(typeof(T), "x");
		var combined = expressions
						.Select(func => func.Body.Replace(func.Parameters[0], param))
						.Aggregate((a, b) => Expression.AndAlso(a, b));

		return Expression.Lambda<Func<T, bool>>(combined, param);
	}
	public static Expression<Func<T, bool>> CombineExpressions<T>(
		params Expression<Func<T, bool>>[] expressions)
	{
		if (expressions is null || expressions.Count() == 0)
		{
			return t => true;
		}
		ParameterExpression param = Expression.Parameter(typeof(T), "x");
		var combined = expressions
						.Select(func => func.Body.Replace(func.Parameters[0], param))
						.Aggregate((a, b) => Expression.AndAlso(a, b));

		return Expression.Lambda<Func<T, bool>>(combined, param);
	}

	/// <summary>
	/// A or (B and C)
	/// </summary>
	/// <remarks>
	/// use this to generate (x.foo > y.foo) or ((x.foo == y) and (x.id > y.id))
	/// </remarks> 
	public static Expression<Func<T, bool>> AorBandC<T>(
		Expression<Func<T, bool>> expressionA, Expression<Func<T, bool>> expressionB, Expression<Func<T, bool>> expressionC)
	{
		ParameterExpression param = Expression.Parameter(typeof(T), "x");
		var aBody = expressionA.Body.Replace(expressionA.Parameters[0], param);
		var bBody = expressionB.Body.Replace(expressionB.Parameters[0], param);
		var cBody = expressionC.Body.Replace(expressionC.Parameters[0], param);

		var combined = Expression.Or(aBody, Expression.And(bBody, cBody));

		return Expression.Lambda<Func<T, bool>>(combined, param);
	}

	/// <summary>
	/// An extension for string to cleanup expression calls for nullable strings that needs case insensitve filter
	/// </summary>
	public static string ToUpperOrDefault(this string value)
	{
		return (value ?? string.Empty).ToUpperInvariant();
	}

	/// <summary>
	/// Merge a new and existing expression using a filter logical operator
	/// </summary>
	public static Expression<Func<T, bool>>? ApplyLogicalOperator<T>(this FilterSpecificationDto filter,
		Expression<Func<T, bool>>? currentExpression, Expression<Func<T, bool>>? newExpression)
	{
		if (newExpression is not null)
		{
			if (currentExpression is null)
			{
				currentExpression = newExpression;
			}
			else
			{
				if (filter.logicalOperator == "OR")
				{
					currentExpression = currentExpression.Or(newExpression);
				}
				else
				{
					currentExpression = currentExpression.And(newExpression);
				}
			}
		}

		return currentExpression;
	}

	/// <summary>
	/// Combines a MemberExpressions, BinaryExpression (=,>,< etc...) and ConstantExpression, eg Id = "4" for the given <see cref="FilterSpecificationDto"/>
	/// </summary>
	public static Expression<Func<T, bool>> CreateExpression<T, TKey>(this FilterSpecificationDto filter,
		Expression<Func<T, TKey>> selector,
		TKey value,
		bool isSql = true,
		bool isFlag = false)
	{
		var parameter = Expression.Parameter(typeof(T));

		var swapVisitor = new SwapVisitor(selector.Parameters[0], parameter);
		var expressionParameter = swapVisitor.Visit(selector.Body)!;
		Expression? body = null;

		switch (filter.@operator)
		{
			case FilterSpecificationDto.EqualsShort:
			case FilterSpecificationDto.EqualsLiteral:
			case FilterSpecificationDto.Is:
				{
					if (isFlag)
					{
						//TKey must be int for flag to work
						body = Expression.And(expressionParameter, Expression.Constant(value, typeof(TKey)));
						body = Expression.Equal(body, Expression.Constant(value, typeof(TKey)));
					}
					else
					{
						body = Expression.Equal(expressionParameter, Expression.Constant(value, typeof(TKey)));
					}
					
					break;
				}
			case FilterSpecificationDto.NotEquals:
			case FilterSpecificationDto.NotEqualsLiteral:
			case FilterSpecificationDto.Not:
				{
					if (isFlag)
					{
						//TKey must be int for flag to work
						body = Expression.And(expressionParameter, Expression.Constant(value, typeof(TKey)));
						body = Expression.NotEqual(body, Expression.Constant(value, typeof(TKey)));
					}
					else
					{
						body = Expression.NotEqual(expressionParameter, Expression.Constant(value, typeof(TKey)));
					}

					break;
				}
			case FilterSpecificationDto.GreaterThan:
			case FilterSpecificationDto.After:
				{
					body = Expression.GreaterThan(expressionParameter, Expression.Constant(value, typeof(TKey)));
					break;
				}
			case FilterSpecificationDto.GreaterThanOrEqual:
			case FilterSpecificationDto.OnOrAfter:
				{
					body = Expression.GreaterThanOrEqual(expressionParameter, Expression.Constant(value, typeof(TKey)));
					break;
				}
			case FilterSpecificationDto.LessThan:
			case FilterSpecificationDto.Before:
				{
					body = Expression.LessThan(expressionParameter, Expression.Constant(value, typeof(TKey)));
					break;
				}
			case FilterSpecificationDto.LessThanOrEqual:
			case FilterSpecificationDto.OnOrBefore:
				{
					body = Expression.LessThanOrEqual(expressionParameter, Expression.Constant(value, typeof(TKey)));
					break;
				}
			case FilterSpecificationDto.Contains:
			case FilterSpecificationDto.NotContains:
				{
					// EF does not support translation of case insensitive overloads but SQL is case insensitive so we can omit them for SQL
					if (isSql)
					{
						var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
						var someValue = Expression.Constant(value, typeof(string));
						body = Expression.Call(expressionParameter, method!, someValue);
					}
					else
					{
						var method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
						var someValue = Expression.Constant(value, typeof(string));
						body = Expression.Call(expressionParameter, method!, someValue, Expression.Constant(StringComparison.OrdinalIgnoreCase));
					}

					if (filter.@operator == FilterSpecificationDto.NotContains)
					{
						body = Expression.Not(body);
					}

					break;
				}
			case FilterSpecificationDto.StartsWith:
				{
					// EF does not support translation of case insensitive overloads but SQL is case insensitive so we can omit them for SQL
					if (isSql)
					{
						var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
						var someValue = Expression.Constant(value, typeof(string));
						body = Expression.Call(expressionParameter, method!, someValue);
					}
					else
					{
						var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) });
						var someValue = Expression.Constant(value, typeof(string));
						body = Expression.Call(expressionParameter, method!, someValue, Expression.Constant(StringComparison.OrdinalIgnoreCase));
					}

					break;
				}
			case FilterSpecificationDto.EndsWith:
				{
					// EF does not support translation of case insensitive overloads but SQL is case insensitive so we can omit them for SQL
					if (isSql)
					{
						var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
						var someValue = Expression.Constant(value, typeof(string));
						body = Expression.Call(expressionParameter, method!, someValue);
					}
					else
					{
						var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string), typeof(StringComparison) });
						var someValue = Expression.Constant(value, typeof(string));
						body = Expression.Call(expressionParameter, method!, someValue, Expression.Constant(StringComparison.OrdinalIgnoreCase));
					}

					break;
				}
			case FilterSpecificationDto.IsEmpty:
				{
					body = Expression.Equal(expressionParameter, Expression.Constant(null, typeof(TKey)));

					if (typeof(TKey) == typeof(string))
					{
						body = Expression.OrElse(body, Expression.Equal(expressionParameter, Expression.Constant(string.Empty, typeof(TKey))));
					}

					break;
				}
			case FilterSpecificationDto.IsNotEmpty:
				{
					body = Expression.NotEqual(expressionParameter, Expression.Constant(null, typeof(TKey)));

					if (typeof(TKey) == typeof(string))
					{
						body = Expression.AndAlso(body, Expression.NotEqual(expressionParameter, Expression.Constant(string.Empty, typeof(TKey))));
					}

					break;
				}
		}

		if (body is null)
		{
			throw new NotSupportedException($"Filter expression {filter.@operator}");
		}

		return Expression.Lambda<Func<T, bool>>(body, parameter);
	}

	/// <summary>
	/// Creates a T-SQL string for the given filter in the form of '[ColumnName] operator [@sqlParameter]'
	/// </summary>
	/// <example>
	/// RuleId like '%terminal%'
	/// </example>
	public static string CreateSqlExpression(this FilterSpecificationDto filter, string columnName, string sqlParameter)
	{
		var sqlExpression = string.Empty;

		switch (filter.@operator)
		{
			case FilterSpecificationDto.EqualsShort:
			case FilterSpecificationDto.EqualsLiteral:
			case FilterSpecificationDto.Is:
				{
					sqlExpression = $"{columnName} = {sqlParameter}";
					break;
				}
			case FilterSpecificationDto.GreaterThan:
				{
					sqlExpression = $"{columnName} > {sqlParameter}";
					break;
				}
			case FilterSpecificationDto.GreaterThanOrEqual:
				{
					sqlExpression = $"{columnName} >= {sqlParameter}";
					break;
				}
			case FilterSpecificationDto.LessThan:
				{
					sqlExpression = $"{columnName} < {sqlParameter}";
					break;
				}
			case FilterSpecificationDto.LessThanOrEqual:
				{
					sqlExpression = $"{columnName} <= {sqlParameter}";
					break;
				}
			case FilterSpecificationDto.Contains:
				{
					sqlExpression = $"{columnName} like '%' + {sqlParameter} + '%'";
					break;
				}
			case FilterSpecificationDto.StartsWith:
				{
					sqlExpression = $"{columnName} like '%' + {sqlParameter}";
					break;
				}
			case FilterSpecificationDto.EndsWith:
				{
					sqlExpression = $"{columnName} like {sqlParameter} + '%'";
					break;
				}
			case FilterSpecificationDto.IsEmpty:
				{
					sqlExpression = $"{columnName} is null or {columnName} = ''";
					break;
				}
			case FilterSpecificationDto.IsNotEmpty:
				{
					sqlExpression = $"{columnName} is not null an {columnName} <> ''";
					break;
				}
			default:
				{
					throw new NotSupportedException($"Filter expression {filter.@operator}");
				}
		}

		return sqlExpression;
	}

	/// <summary>
	/// Applies an OrElse operator to 2 expressions
	/// </summary>
	public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> e1,
														Expression<Func<T, bool>> e2)
	{
		var swapVisitor = new SwapVisitor(e1.Parameters[0], e2.Parameters[0]);
		var lambda = Expression.Lambda<Func<T, bool>>(Expression.OrElse(swapVisitor.Visit(e1.Body)!, e2.Body), e2.Parameters);
		return lambda;
	}
	/// <summary>
	/// An inline shortcut filter alternative to the switch statement approach
	/// </summary>
	public static Expression<Func<T, bool>>? AddFilter<T, TKey>(this IQueryable<T> queryable,
		FilterSpecificationDto[] filterSpecifications,
		string field,
		Expression<Func<T, TKey>> selector,
		Func<FilterSpecificationDto, TKey> value,
		Expression<Func<T, bool>>? result = null,
		bool isFlag = false)
	{
		foreach(var filterSpecification in filterSpecifications.Where(v => v.field == field))
		{
			var expression = filterSpecification.CreateExpression(selector, value(filterSpecification), isFlag: isFlag);

			result = filterSpecification.ApplyLogicalOperator(result, expression);
		}

		return result;
	}

	/// <summary>
	/// Applies an AndAlso operator to 2 expressions
	/// </summary>
	public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> e1,
														 Expression<Func<T, bool>> e2)
	{
		if(e1 is null)
		{
			return e2;
		}

		if(e2 is null)
		{
			return e1;
		}

		var swapVisitor = new SwapVisitor(e1.Parameters[0], e2.Parameters[0]);
		var lambda = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(swapVisitor.Visit(e1.Body)!, e2.Body), e2.Parameters);
		return lambda;
	}

	public static IOrderedQueryable<T> AddSortAscending<T, U>(this IQueryable<T> queryable,
		IOrderedQueryable<T> orderedQueryable,
		bool first, Expression<Func<T, U>> fieldSelector)
	{
		var result = first ? queryable.OrderBy(fieldSelector) : orderedQueryable.ThenBy(fieldSelector);
		return result;
	}

	public static IOrderedQueryable<T> AddSortDescending<T, U>(this IQueryable<T> queryable,
		IOrderedQueryable<T> orderedQueryable,
		bool first, Expression<Func<T, U>> fieldSelector)
	{
		var result = first ? queryable.OrderByDescending(fieldSelector) : orderedQueryable.ThenByDescending(fieldSelector);
		return result;
	}

	public static IOrderedQueryable<T> AddSort<T, U>(this IQueryable<T> queryable,
		IOrderedQueryable<T> orderedQueryable,
		bool first, Expression<Func<T, U>> fieldSelector, string direction)
	{
		switch (direction.ToUpperInvariant())
		{
			case "ASC": return AddSortAscending(queryable, orderedQueryable, first, fieldSelector);
			case "DESC": return AddSortDescending(queryable, orderedQueryable, first, fieldSelector);
			default: throw new Exception("Sort direction must be ASC or DESC");
		}
	}

	/// <summary>
	/// An inline shortcut sort alternative to the switch statement approach
	/// </summary>
	public static IOrderedQueryable<T> AddSort<T, U>(this IQueryable<T> queryable,
		IOrderedQueryable<T> ordered,
		SortSpecificationDto[] sortSpecifications,
		string field,
		Expression<Func<T, U>> fieldSelector)
	{
		var sortSpecification = sortSpecifications.FirstOrDefault(v => v.field == field);

		return AddSort(queryable, ordered, sortSpecification, fieldSelector);
	}

	/// <summary>
	/// An inline shortcut sort alternative to the switch statement approach
	/// </summary>
	public static IOrderedQueryable<T> AddSort<T, U>(this IQueryable<T> queryable,
		IOrderedQueryable<T> ordered,
		SortSpecificationDto? sortSpecification,
		Expression<Func<T, U>> fieldSelector)
	{
		if (sortSpecification != null)
		{
			ordered = AddSort(queryable, ordered!, ordered == null, fieldSelector, sortSpecification.sort);
		}

		return ordered;
	}

	public static IQueryable<T> Page<T>(this IQueryable<T> queryable, int? page, int? take, out int skipped)
	{
		skipped = 0;

		if (page.HasValue && take.HasValue && page.Value > 0)
		{
			skipped = (page.Value - 1) * take.Value;

			queryable = queryable.Skip(skipped);
		}

		if (take.HasValue && take.Value > 0 && take.Value < 1000000)
		{
			queryable = queryable.Take(take.Value);
		}

		return queryable;
	}
}
