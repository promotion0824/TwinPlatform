using System;

namespace Willow.Expressions.Visitor.Sql
{
    /// <summary>
    /// A parameter for a call to SQL
    /// </summary>
    public class SqlParameter
    {
        /// <summary>
        /// Name of parameter
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The .NET Type of this parameter
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public object? ParameterValue { get; }

        /// <summary>
        /// Create a new instance of the <see cref="SqlParameter"/> class
        /// </summary>
        public SqlParameter(string parameterName, Type type, object? parameterValue)
        {
            this.ParameterName = parameterName;
            this.Type = type;
            this.ParameterValue = parameterValue;
        }
    }
}
