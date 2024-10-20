using System;
using System.Collections.Generic;

namespace Willow.Expressions
{
    /// <summary>
    /// Interface for a TokenExpression
    /// </summary>
    public interface ITokenExpression
    {
        /// <summary>
        /// Convert the expression to a double
        /// </summary>
        double? ToDouble<TSource>(TSource obj);

        /// <summary>
        /// Convert the expression to a DateTime
        /// </summary>
        DateTime? ToDateTime<TSource>(TSource obj);

        /// <summary>
        /// Convert the expression to a string
        /// </summary>
        string? ToString<TSource>(TSource obj);

        /// <summary>
        /// Convert the expression to a bool
        /// </summary>
        bool? ToBool<TSource>(TSource obj);

        /// <summary>
        /// Get the unbound variables used in the expression
        /// </summary>
        IEnumerable<string> UnboundVariables { get; }

        /// <summary>
        /// Get the unbound functions used in the expression
        /// </summary>
        IEnumerable<string> UnboundFunctions { get; }

        /// <summary>
        /// Bind a variable name to a value
        /// </summary>
        ITokenExpression Bind(string variableName, object value);

        /// <summary>
        /// Concrete tokens (i.e. ones we create using factories have actual text values)
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Serialize the object to a string
        /// </summary>
        /// <returns></returns>
        string Serialize();
    }
}
