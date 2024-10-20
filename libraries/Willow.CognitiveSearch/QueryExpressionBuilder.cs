namespace Willow.CognitiveSearch;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azure.Search.Documents;

/// <summary>
/// Base interface for Azure Cognitive Search query expression builder.
/// </summary>
public interface IQueryExpressionBuilder
{
    /// <summary>
    /// Get the constructed query.
    /// </summary>
    /// <returns>Query expression as string.</returns>
    public string GetQuery();

    /// <summary>
    /// Appends an 'and' logic operator to the query;
    /// Operation will be ignored if the existing query string is empty.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder And();

    /// <summary>
    /// Appends a 'or' logic operator to the query;
    /// Operation will be ignored if the existing query string is empty.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder Or();

    /// <summary>
    /// Appends a 'not' logic operator to the query;
    /// Operation will be ignored if the existing query string is empty.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder Not();

    /// <summary>
    /// Appends an '(' opening parenthesis to the query;.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder OpenParenthesis();

    /// <summary>
    /// Appends an ')' closing parenthesis to the query;.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder CloseParenthesis();
}

/// <summary>
/// Query Expression builder for ACS query string $filter parameter.
/// </summary>
public interface IQueryFilter : IQueryExpressionBuilder
{
    /// <summary>
    /// Appends a condition string that filter by matching the index field with the list of input values.
    /// </summary>
    /// <param name="field">Name of the index field; Should not be a collection.</param>
    /// <param name="values">Array of input values.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter SearchIn(string field, IEnumerable<string> values);

    /// <summary>
    /// Appends a condition string that filter by matching the index collection field values with the list of input values.
    /// </summary>
    /// <param name="collectionField">Name of the index field; Should be a collection.</param>
    /// <param name="values">Array of input values.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter SearchInArray(string collectionField, IEnumerable<string> values);

    /// <summary>
    /// Appends a condition string that matches index field equal to input value.
    /// </summary>
    /// <param name="field">Name of the index field.</param>
    /// <param name="value">Value to compare. Method automatically quote or unquote based on the value type.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter FilterEqual(string field, string value);

    /// <summary>
    /// Appends a condition string that matches index field not equal to input value.
    /// </summary>
    /// <param name="field">Name of the index field.</param>
    /// <param name="value">Value to compare. Method automatically quote or unquote based on the value type.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter FilterNotEqual(string field, string value);

    /// <summary>
    /// Appends a condition string that matches index collection field containing input value.
    /// </summary>
    /// <param name="collectionField">Name of the index field. Should be a collection type.</param>
    /// <param name="value">Value to compare. Method automatically quote or unquote based on the value type.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter FilterContains(string collectionField, string value);
}

/// <summary>
/// Query Expression builder for ACS query string $search parameter.
/// </summary>
public interface IQuerySearch : IQueryFilter
{
}

/// <summary>
/// Implementation for <see cref="IQueryExpressionBuilder"/>.
/// </summary>
public class QueryExpressionBuilder : IQuerySearch
{
    private readonly StringBuilder queryBuilder;
    private const char Delimiter = ',';

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryExpressionBuilder"/> class.
    /// </summary>
    private QueryExpressionBuilder()
    {
        queryBuilder = new StringBuilder();
    }

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <returns>Instance of <see cref="QueryExpressionBuilder"/>.</returns>
    public static IQueryExpressionBuilder Create()
    {
        return new QueryExpressionBuilder();
    }

    /// <summary>
    /// Get the constructed query.
    /// </summary>
    /// <returns>Query expression as string.</returns>
    public string GetQuery()
    {
        return queryBuilder.ToString();
    }

    /// <summary>
    /// Appends an 'and' logic operator to the query;
    /// Operation will be ignored if the existing query string is empty.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder And()
    {
        if (queryBuilder.Length != 0)
        {
            queryBuilder.Append(" and ");
        }

        return this;
    }

    /// <summary>
    /// Appends a 'not' logic operator to the query;
    /// Operation will be ignored if the existing query string is empty.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder Not()
    {
        if (queryBuilder.Length != 0)
        {
            queryBuilder.Append(" not ");
        }

        return this;
    }

    /// <summary>
    /// Appends a 'or' logic operator to the query;
    /// Operation will be ignored if the existing query string is empty.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder Or()
    {
        if (queryBuilder.Length != 0)
        {
            queryBuilder.Append(" or ");
        }

        return this;
    }

    /// <summary>
    /// Appends an '(' opening paranthesis to the query;.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder OpenParenthesis()
    {
        queryBuilder.Append('(');
        return this;
    }

    /// <summary>
    /// Appends an ')' closing paranthesis to the query;.
    /// </summary>
    /// <returns>Implementation of <see cref="IQueryExpressionBuilder"/>.</returns>
    public IQueryExpressionBuilder CloseParenthesis()
    {
        queryBuilder.Append(')');
        return this;
    }

    /// <summary>
    /// Appends a condition string that filter by matching the index field with the list of input values.
    /// </summary>
    /// <param name="field">Name of the index field; Should not be a collection.</param>
    /// <param name="values">Array of input values.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter SearchIn(string field, IEnumerable<string> values)
    {
        ArgumentException.ThrowIfNullOrEmpty(field);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Any())
        {
            var filterExpressionValues = SearchFilter.Create($"{string.Join(Delimiter, values)}");
            queryBuilder.Append($"search.in({field}, {filterExpressionValues}, '{Delimiter}')");
        }

        return this;
    }

    /// <summary>
    /// Appends a condition string that filter by matching the index collection field values with the list of input values.
    /// </summary>
    /// <param name="collectionField">Name of the index field; Should be a collection.</param>
    /// <param name="values">Array of input values.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter SearchInArray(string collectionField, IEnumerable<string> values)
    {
        ArgumentException.ThrowIfNullOrEmpty(collectionField);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Any())
        {
            var filterExpressionValues = SearchFilter.Create($"{string.Join(Delimiter, values)}");
            queryBuilder.Append($"{collectionField}/any(f: search.in(f,{filterExpressionValues},'{Delimiter}'))");
        }

        return this;
    }

    /// <summary>
    /// Appends a condition string that matches index field equal to input value.
    /// </summary>
    /// <param name="field">Name of the index field.</param>
    /// <param name="value">Value to compare. Method automatically quote or unquote based on the value type.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter FilterEqual(string field, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(field);
        ArgumentException.ThrowIfNullOrEmpty(value);
        queryBuilder.Append($"{field} eq " + SearchFilter.Create($"{value}"));
        return this;
    }

    /// <summary>
    /// Appends a condition string that matches index field not equal to input value.
    /// </summary>
    /// <param name="field">Name of the index field.</param>
    /// <param name="value">Value to compare. Method automatically quote or unquote based on the value type.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter FilterNotEqual(string field, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(field);
        ArgumentException.ThrowIfNullOrEmpty(value);
        queryBuilder.Append($"{field} ne " + SearchFilter.Create($"{value}"));
        return this;
    }

    /// <summary>
    /// Appends a condition string that matches index collection field containing input value.
    /// </summary>
    /// <param name="collectionField">Name of the index field. Should be a collection type.</param>
    /// <param name="value">Value to compare. Method automatically quote or unquote based on the value type.</param>
    /// <returns>Implementation of <see cref="IQueryFilter"/>.</returns>
    public IQueryFilter FilterContains(string collectionField, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(collectionField);
        ArgumentException.ThrowIfNullOrEmpty(value);
        queryBuilder.Append($"{collectionField}/any(f: f eq " + SearchFilter.Create($"{value})"));
        return this;
    }
}
