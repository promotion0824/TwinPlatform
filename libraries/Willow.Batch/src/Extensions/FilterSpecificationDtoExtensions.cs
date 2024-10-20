using System.Collections.Generic;
using System.Linq;

namespace Willow.Batch;

/// <summary>
/// Extensions supporting the <see cref="FilterSpecificationDto"/>.
/// </summary>
public static class FilterSpecificationDtoExtensions
{
    /// <summary>
    /// Get the first filter specification for the field.
    /// </summary>
    /// <param name="specs">Collection of the FilterSpecifications.</param>
    /// <param name="field">The field to evaluate.</param>
    /// <param name="op">The operation to perform on the field.</param>
    /// <returns>The matching filter.</returns>
    public static FilterSpecificationDto FirstOrDefault(this IEnumerable<FilterSpecificationDto> specs, string field, string op = null)
    {
        return specs?.FirstOrDefault(x => x.Field.ToLower() == field.ToLower() && (string.IsNullOrEmpty(op) || x.Operator == op));
    }

    /// <summary>
    /// Remove the filter specification for the field.
    /// </summary>
    /// <param name="specs">Collection of the FilterSpecifications.</param>
    /// <param name="field">The field to evaluate.</param>
    /// <returns>Collection of the FilterSpecifications with the matching value removed.</returns>
    public static FilterSpecificationDto[] RemoveFilters(this IEnumerable<FilterSpecificationDto> specs, string field)
    {
        var result = specs?.ToList();

        if (result != null && !string.IsNullOrEmpty(field))
        {
            result.RemoveAll(x => x.Field.ToLower() == field.ToLower());
        }

        return result.ToArray();
    }

    /// <summary>
    /// Rename the filter specification for the field.
    /// </summary>
    /// <param name="specs">Collection of the FilterSpecifications.</param>
    /// <param name="field">The field to evaluate.</param>
    /// <param name="newField">The new field to evaluate.</param>
    /// <returns>The updated collection of the FilterSpecifications.</returns>
    public static FilterSpecificationDto[] RenameFilter(this IEnumerable<FilterSpecificationDto> specs, string field, string newField)
    {
        var filter = specs?.FirstOrDefault(field);

        return specs?.ReplaceFilter(filter, newField, filter?.Operator, filter?.Value);
    }

    /// <summary>
    /// Replace the filter specification for the field.
    /// </summary>
    /// <param name="specs">Collection of the FilterSpecifications.</param>
    /// <param name="field">The field to evaluate.</param>
    /// <param name="newField">The new field to evaluate.</param>
    /// <param name="op">The operation to perform on the field.</param>
    /// <param name="value">The value to compare with.</param>
    /// <returns>The updated collection of the FilterSpecifications.</returns>
    public static FilterSpecificationDto[] ReplaceFilter(this IEnumerable<FilterSpecificationDto> specs, string field, string newField, string op, object value)
    {
        return specs?.ReplaceFilter(specs?.FirstOrDefault(field), newField, op, value);
    }

    /// <summary>
    /// Replace a filter in the collection.
    /// </summary>
    /// <param name="specs">Collection of the FilterSpecifications.</param>
    /// <param name="filter">The filter to replace to evaluate.</param>
    /// <param name="newField">The new field to evaluate.</param>
    /// <param name="op">The operation to perform on the field.</param>
    /// <param name="value">The value to compare with.</param>
    /// <returns>The updated collection of the FilterSpecifications.</returns>
    public static FilterSpecificationDto[] ReplaceFilter(this IEnumerable<FilterSpecificationDto> specs, FilterSpecificationDto filter, string newField, string op, object value)
    {
        specs = specs?
            .RemoveFilters(filter?.Field)
            .Upsert(newField, op, value);

        return specs.ToArray();
    }

    /// <summary>
    /// Add or update the filter specification for the field.
    /// </summary>
    /// <param name="specs">Collection of the FilterSpecifications.</param>
    /// <param name="field">The field to replace to evaluate.</param>
    /// <param name="op">The operation to perform on the field.</param>
    /// <param name="value">The value to compare with.</param>
    /// <returns>The updated collection of the FilterSpecifications.</returns>
    public static FilterSpecificationDto[] Upsert(this IEnumerable<FilterSpecificationDto> specs, string field, string op, object value)
    {
        var filter = new FilterSpecificationDto { Field = field, Operator = op, Value = value };

        if (!filter.Validate(null).Any())
        {
            var result = specs?.ToList();
            if (result != null)
            {
                result.RemoveFilters(field);
                result.Add(filter);

                return result.ToArray();
            }
        }

        return specs.ToArray();
    }

    /// <summary>
    /// Add or update the filter specification for the field.
    /// </summary>
    /// <typeparam name="T">The type of the object the fitler is applied to.</typeparam>
    /// <param name="specs">Collection of the FilterSpecifications.</param>
    /// <param name="field">The field to replace to evaluate.</param>
    /// <param name="value">The type of the object in the filter.</param>
    /// <returns>The updated collection of the FilterSpecifications.</returns>
    public static FilterSpecificationDto[] Upsert<T>(this IEnumerable<FilterSpecificationDto> specs, string field, T value)
    {
        if (value.IsEnumerable())
        {
            return value.HasAny() ? specs.Upsert(field, FilterOperators.ContainedIn, value) : specs.ToArray();
        }

        return specs.Upsert(field, FilterOperators.EqualsLiteral, value);
    }
}
