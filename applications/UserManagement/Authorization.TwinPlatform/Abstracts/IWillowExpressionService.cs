using Authorization.Common.Models;
using Willow.Expressions;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Willow Expression Service Interface.
/// </summary>
public interface IWillowExpressionService
{
    /// <summary>
    /// Parse and Evaluate the input expression using Willow Expression Library.
    /// </summary>
    /// <typeparam name="T">Type of result.</typeparam>
    /// <param name="expression">String Input expression</param>
    /// <param name="expressionEnvironment">Expression Environment.</param>
    /// <param name="errors">List of errors occurred while evaluation.</param>
    /// <returns>Response.</returns>
    T? Evaluate<T>(string expression, Env expressionEnvironment, out List<string> errors);

    /// <summary>
    /// Parse and Evaluate the input expression using Willow Expression Library.
    /// </summary>
    /// <typeparam name="T">Type of result.</typeparam>
    /// <param name="expression">String Input expression</param>
    /// <param name="errors">List of errors occurred while evaluation.</param>
    /// <returns>Response.</returns>
    T? EvaluateUsingDefaultEnv<T>(string expression, out List<string> errors);

    /// <summary>
    /// Gets the default UM Environment for Willow Expression evaluation.
    /// </summary>
    /// <returns>Willow Expression Environment.</returns>
    Env GetUMDefaultEnvironment();

    /// <summary>
    /// Evaluate and return the expression status and out errors if any
    /// </summary>
    /// <param name="expressionEnv">Expression Environment.</param>
    /// <param name="expression">Expression string to evaluate.</param>
    /// <param name="errors">Out Errors to return after evaluation.</param>
    /// <returns>ExpressionStatus Enum.</returns>
    ExpressionStatus GetExpressionStatus(Env expressionEnv, string? expression, out List<string> errors);
}
