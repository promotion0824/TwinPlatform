using System;
using Willow.IoTService.Monitoring.Queries;

namespace Willow.IoTService.Monitoring.Services.AppInsights
{
    public static class CustomDimensionFilterKqlBuilder
    {
        public static string GetKql(CustomDimensionEqualityFilter filter)
        {
            var propName = filter.DimensionName;
            var value = ResolveValue(filter.Value);

            var @operator = ResolveOperator(filter.Operator);

            return $"customDimensions.{propName} {@operator} {value}";
        }

        private static string ResolveOperator(CustomDimensionEqualityFilter.EqualityOperators @operator)
        {
            if (@operator == CustomDimensionEqualityFilter.EqualityOperators.Equal)
            {
                return "==";
            }

            if (@operator == CustomDimensionEqualityFilter.EqualityOperators.NotEqual)
            {
                return "!=";
            }

            if (@operator == CustomDimensionEqualityFilter.EqualityOperators.GreaterThan)
            {
                return ">";
            }

            if (@operator == CustomDimensionEqualityFilter.EqualityOperators.GreaterThanOrEqualTo)
            {
                return ">=";
            }

            if (@operator == CustomDimensionEqualityFilter.EqualityOperators.LessThan)
            {
                return "<";
            }

            if (@operator == CustomDimensionEqualityFilter.EqualityOperators.LessThanOrEqualTo)
            {
                return "<=";
            }

            throw new NotSupportedException("EqualityOperator is not supported");
        }

        private static string? ResolveValue(object value)
        {
            if (value is string)
            {
                return $"'{value}'";
            }

            if (value is DateTime d)
            {
                return $"'{d.ToUniversalTime()}'";
            }

            if (value is Guid)
            {
                return $"'{value}'";
            }

            return value.ToString();
        }
    }
}