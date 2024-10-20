using System;

namespace Willow.Expressions.Visitor.Sql
{
    /// <summary>
    /// An exception that occurs during a temporal conversion
    /// </summary>
    public class TemporalConversionException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TemporalConversionException"/> class which is thrown for attempts to convert a TemporalExpression directly when it should be on the RHS of a matches expression
        /// </summary>
        public TemporalConversionException(Type type)
            : base($"Cannot convert {type.Name} directly, need to use matches(expr, temporal)")
        {
        }
    }
}
