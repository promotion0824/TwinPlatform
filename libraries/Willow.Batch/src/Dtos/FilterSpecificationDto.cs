namespace Willow.Batch;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

/// <summary>
/// Filter specification component used by MUI grid.
/// </summary>
public class FilterSpecificationDto : IValidatableObject
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    [Required]
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the operator.
    /// </summary>
    /// <remarks>Valid values are: "contains", "starts with", "ends with" and "equals".</remarks>
    [Required]
    public string Operator { get; set; }

    /// <summary>
    /// Gets or sets the value for the filter.
    /// </summary>
    /// <remarks>
    /// Valid values are string, int, double and bool.
    /// </remarks>
    public object Value { get; set; }

    // note that the Field may be compound as in StatusLogs[Status] used to indicated an Any request
    private string PropertyName => HasBracket ? Field.Substring(0, Field.IndexOf("[")) : Field;

    private string BracketName => Field.Replace(PropertyName, string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);

    private bool HasBracket => Field.Contains('[') && Field.Contains(']');

    /// <summary>
    /// Validates the value for the <see cref="FilterSpecificationDto"/>.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var supportedOperators = new string[]
        {
                FilterOperators.ContainedIn,
                FilterOperators.Contains,
                FilterOperators.NotContains,
                FilterOperators.StartsWith,
                FilterOperators.EndsWith,
                FilterOperators.EqualsLiteral,
                FilterOperators.EqualsShort,
                FilterOperators.NotEquals,
                FilterOperators.GreaterThan,
                FilterOperators.GreaterThanOrEqual,
                FilterOperators.LessThan,
                FilterOperators.LessThanOrEqual,
                FilterOperators.Is,
                FilterOperators.IsNot,
                FilterOperators.IsEmpty,
                FilterOperators.IsNotEmpty,
                FilterOperators.IsNull,
                FilterOperators.IsNotNull,
                FilterOperators.Like,
                FilterOperators.In,
                FilterOperators.NotIn,
        };

        if (!supportedOperators.Contains(Operator))
        {
            return new ValidationResult[]
            {
                    new ValidationResult($"Unsupported expression of type '{Operator}'. Supported expression are: {string.Join(',', supportedOperators)}"),
            };
        }

        var nullValueOperators = new string[]
        {
                FilterOperators.IsEmpty,
                FilterOperators.IsNotEmpty,
                FilterOperators.IsNull,
                FilterOperators.IsNotNull,
        };

        if (Value == null && !nullValueOperators.Contains(Operator))
        {
            return new ValidationResult[]
            {
                    new ValidationResult($"{Operator} requires value to be assigned."),
            };
        }

        if ((Operator is FilterOperators.ContainedIn or FilterOperators.In or FilterOperators.NotIn) && !Value.HasAny())
        {
            return new ValidationResult[]
            {
                    new ValidationResult($"{Operator} requires value to be a non-empty collection."),
            };
        }

        return Array.Empty<ValidationResult>();
    }

    /// <summary>
    /// Build the expression for the filter.
    /// </summary>
    /// <typeparam name="T">The type of the object to filter.</typeparam>
    /// <returns>A filter expression.</returns>
    public Expression<Func<T, bool>> Build<T>()
    {
        var fields = Field.Split(",").ToList();
        if (fields != null && fields.Any())
        {
            Expression<Func<T, bool>> fieldExpression = null;

            foreach (var field in fields)
            {
                Field = field.Trim();
                fieldExpression = fieldExpression == null ? BuildFieldExpression<T>() : fieldExpression.Or(BuildFieldExpression<T>());
            }

            return fieldExpression;
        }

        return BuildFieldExpression<T>();
    }

    /// <summary>
    /// Creates a T-SQL string for the given filter in the form of '[ColumnName] operator [@sqlParameter]'.
    /// </summary>
    /// <returns>String.</returns>
    /// <example>
    /// RuleId like '%terminal%'.
    /// </example>
    public string BuildSqlExpression()
    {
        var sqlExpression = string.Empty;

        switch (Operator)
        {
            case FilterOperators.EqualsShort:
            case FilterOperators.EqualsLiteral:
            case FilterOperators.Is:
                {
                    sqlExpression = $"{Field} = {Value}";
                    break;
                }

            case FilterOperators.GreaterThan:
                {
                    sqlExpression = $"{Field} > {Value}";
                    break;
                }

            case FilterOperators.GreaterThanOrEqual:
                {
                    sqlExpression = $"{Field} >= {Value}";
                    break;
                }

            case FilterOperators.LessThan:
                {
                    sqlExpression = $"{Field} < {Value}";
                    break;
                }

            case FilterOperators.LessThanOrEqual:
                {
                    sqlExpression = $"{Field} <= {Value}";
                    break;
                }

            case FilterOperators.Contains:
                {
                    sqlExpression = $"{Field} like '{Value}' + '%'";
                    break;
                }

            case FilterOperators.StartsWith:
                {
                    sqlExpression = $"{Field} like '%' + '{Value}'";
                    break;
                }

            case FilterOperators.EndsWith:
                {
                    sqlExpression = $"{Field} like '{Value}' + '%'";
                    break;
                }

            case FilterOperators.IsEmpty:
                {
                    sqlExpression = $"{Field} is null or {Field} = ''";
                    break;
                }

            case FilterOperators.IsNotEmpty:
                {
                    sqlExpression = $"{Field} is not null an {Field} <> ''";
                    break;
                }

            default:
                {
                    throw new NotSupportedException($"Filter expression {Operator}");
                }
        }

        return sqlExpression;
    }

    /// <summary>
    /// Creates a KQL string for the given filter in the form of '[ColumnName] operator [@kqlParameter]'.
    /// </summary>
    /// <returns> string.</returns>
    /// <example>
    /// ExternalId == "WFCU 1-35".
    /// </example>
    public string BuildKqlExpression()
    {
        var kqlExpression = string.Empty;

        switch (Operator)
        {
            case FilterOperators.EqualsShort:
            case FilterOperators.EqualsLiteral:
            case FilterOperators.Is:
                {
                    kqlExpression = $"{Field} == '{Value}'";
                    break;
                }

            case FilterOperators.GreaterThan:
                {
                    kqlExpression = $"{Field} > '{Value}'";
                    break;
                }

            case FilterOperators.GreaterThanOrEqual:
                {
                    kqlExpression = $"{Field} >= '{Value}'";
                    break;
                }

            case FilterOperators.LessThan:
                {
                    kqlExpression = $"{Field} < '{Value}'";
                    break;
                }

            case FilterOperators.LessThanOrEqual:
                {
                    kqlExpression = $"{Field} <= '{Value}'";
                    break;
                }

            case FilterOperators.Contains:
                {
                    kqlExpression = $"{Field} contains '{Value}'";
                    break;
                }

            case FilterOperators.StartsWith:
                {
                    kqlExpression = $"{Field} startswith '{Value}'";
                    break;
                }

            case FilterOperators.EndsWith:
                {
                    kqlExpression = $"{Field} endswith '{Value}'";
                    break;
                }

            case FilterOperators.IsEmpty:
                {
                    kqlExpression = $"{Field} == '' or isnull({Field})";
                    break;
                }

            case FilterOperators.IsNotEmpty:
                {
                    kqlExpression = $"isnotempty({Field})";
                    break;
                }

            default:
                {
                    throw new NotSupportedException($"Filter expression {Operator}");
                }
        }

        return kqlExpression;
    }

    /// <summary>
    /// Creates a ADT query string for the given filter in the form of '[ColumnName] operator [@adtQueryParameter]'.
    /// </summary>
    /// <param name="prependField">Prepend parameter name.</param>
    /// <returns> Query Expression String.</returns>
    /// <example>
    /// ExternalId = "WFCU 1-35".
    /// </example>
    public string BuildAdtQueryExpression(string prependField)
    {
        var adtQueryExpression = string.Empty;

        switch (Operator)
        {
            case FilterOperators.EqualsShort:
            case FilterOperators.EqualsLiteral:
            case FilterOperators.Is:
                {
                    adtQueryExpression = $"{prependField}{Field} = '{Value}'";
                    break;
                }

            case FilterOperators.GreaterThan:
                {
                    adtQueryExpression = $"{prependField}{Field} = '{Value}'";
                    break;
                }

            case FilterOperators.GreaterThanOrEqual:
                {
                    adtQueryExpression = $"{prependField}{Field} >= '{Value}'";
                    break;
                }

            case FilterOperators.LessThan:
                {
                    adtQueryExpression = $"{prependField}{Field} < '{Value}'";
                    break;
                }

            case FilterOperators.LessThanOrEqual:
                {
                    adtQueryExpression = $"{prependField}{Field} <= '{Value}'";
                    break;
                }

            case FilterOperators.Contains:
                {
                    adtQueryExpression = $"CONTAINS({prependField}{Field},'{Value}')";
                    break;
                }

            case FilterOperators.StartsWith:
                {
                    adtQueryExpression = $"STARTSWITH({prependField}{Field},'{Value}')";
                    break;
                }

            case FilterOperators.EndsWith:
                {
                    adtQueryExpression = $"ENDSWITH({prependField}{Field},'{Value}')";
                    break;
                }

            case FilterOperators.IsEmpty:
                {
                    adtQueryExpression = $"IS_NULL({prependField}{Field}) OR {Field} = '' OR NOT IS_DEFINED({Field})";
                    break;
                }

            case FilterOperators.IsNotEmpty:
                {
                    adtQueryExpression = $"NOT IS_NULL({prependField}{Field}) AND IS_DEFINED({prependField}{Field})";
                    break;
                }

            default:
                {
                    throw new NotSupportedException($"Filter expression {Operator}");
                }
        }

        return adtQueryExpression;
    }

    /// https://www.codeproject.com/Articles/1079028/Build-Lambda-Expressions-Dynamically
    /// <summary>
    /// Build Field Expression.
    /// </summary>
    /// <typeparam name="T">Type of Entity.</typeparam>
    /// <returns>Expression.</returns>
    private Expression<Func<T, bool>> BuildFieldExpression<T>()
    {
        if (Validate(null).Any())
        {
            // assume filter passes when it cannot be validated
            return ExpressionExtensions.True<T>();
        }

        var parameter = Expression.Parameter(typeof(T), "x");

        var property = parameter.GetProperty(PropertyName);
        if (property == null)
        {
            // assume filter passes when the property cannot be obtained
            return ExpressionExtensions.True<T>();
        }

        var body = HasBracket ? BuildBracketExpression(property, parameter) : BuildExpression(property);

        body = AppendParentNullCheck(parameter, PropertyName, body);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private Expression BuildExpression(Expression property)
    {
        var propertyValue = property.Type.GetPropertyValue(Value);

        var constant = CreateConstantExpression(property.Type, propertyValue);

        return Operator.Invoke(property, constant, propertyValue);
    }

    private ConstantExpression CreateConstantExpression(Type valueType, object propertyValue)
    {
        // Get Constant Target Type
        var targetType = Operator is FilterOperators.In or FilterOperators.NotIn or FilterOperators.ContainedIn ?
            valueType.MakeArrayType() : valueType;

        if (valueType.IsValueType && (propertyValue is null || string.IsNullOrWhiteSpace(propertyValue.ToString())))
        {
            // // If Value Type Create a default value if the input is null or empty
            propertyValue = targetType.IsArray ?
                Array.CreateInstance(valueType, 0)! :
                Activator.CreateInstance(valueType)!;
        }

        return valueType.IsNullable() && !propertyValue.GetType().IsEnumerable()
            ? Expression.Constant(propertyValue, valueType)
            : Expression.Constant(propertyValue);
    }

    private Expression BuildBracketExpression(Expression property, ParameterExpression parameter)
    {
        // e.g. StatusLogs[Status] when we query for insights that have been previously resolved
        var bracketType = parameter.Type.GetProperty(PropertyName).PropertyType.GetGenericArguments()[0];

        var bracketParameter = Expression.Parameter(bracketType, "i");
        var bracketProperty = BuildExpression(bracketParameter.GetProperty(BracketName));
        var lambda = Expression.Lambda(bracketProperty, bracketParameter);

        return FilterOperators.Any.Invoke(property, lambda, bracketType);
    }

    /// <summary>
    /// Ensure that the parent of a nested property is not null.
    /// </summary>
    private Expression AppendParentNullCheck(Expression param, string propertyName, Expression whereExpression)
    {
        if (propertyName.Contains("."))
        {
            var parentName = propertyName.Substring(0, propertyName.LastIndexOf("."));
            var parent = param.GetProperty(parentName);
            return AppendParentNullCheck(param, parentName, Expression.AndAlso(Expression.NotEqual(parent, Expression.Constant(null)), whereExpression));
        }

        if (HasBracket)
        {
            var parentName = propertyName;
            var parent = param.GetProperty(parentName);
            return Expression.AndAlso(Expression.NotEqual(parent, Expression.Constant(null)), whereExpression);
        }

        return whereExpression;
    }
}
