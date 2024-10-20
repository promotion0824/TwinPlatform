using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Queries
{
    public class CustomDimensionEqualityFilter : IMetricQueryFilter
    {
        public string DimensionName { get; set; } = string.Empty;

        public object Value { get; set; } = 0;

        public EqualityOperators Operator { get; set; }

        public static IMetricQueryFilter EqualsFilter(string dimension, object value)
        {
            return new CustomDimensionEqualityFilter
            {
                DimensionName = dimension,
                Operator = EqualityOperators.Equal,
                Value = value
            };
        }

        public static CustomDimensionEqualityFilter NotEqualsFilter(string dimension, object value)
        {
            return new CustomDimensionEqualityFilter
            {
                DimensionName = dimension,
                Operator = EqualityOperators.NotEqual,
                Value = value
            };
        }

        public static IMetricQueryFilter LessThanOrEqualToFilter(string dimension, decimal value)
        {
            return new CustomDimensionEqualityFilter
            {
                DimensionName = dimension,
                Operator = EqualityOperators.LessThanOrEqualTo,
                Value = value
            };
        }

        public static IMetricQueryFilter GreaterThanOrEqualToFilter(string dimension, decimal value)
        {
            return new CustomDimensionEqualityFilter
            {
                DimensionName = dimension,
                Operator = EqualityOperators.GreaterThanOrEqualTo,
                Value = value
            };
        }

        public enum EqualityOperators
        {
            Equal,
            LessThan,
            LessThanOrEqualTo,
            GreaterThan,
            GreaterThanOrEqualTo,
            NotEqual
        }
    }
}