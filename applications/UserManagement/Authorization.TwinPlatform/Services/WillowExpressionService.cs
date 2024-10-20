using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Willow.ExpressionParser;
using Willow.Expressions;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Willow Expression Service Implementation.
/// </summary>
public class WillowExpressionService: IWillowExpressionService
{
    /// <summary>
    /// Willow Expression Service Constructor.
    /// </summary>
    public WillowExpressionService() { }

    /// <summary>
    /// Parse and Evaluate the input expression using Willow Expression Library.
    /// </summary>
    /// <typeparam name="T">Type of result.</typeparam>
    /// <param name="expression">String Input expression</param>
    /// <param name="expressionEnvironment">Expression Environment.</param>
    /// <param name="errors">List of errors occurred while evaluation.</param>
    /// <returns>Response.</returns>
    public T? Evaluate<T>(string expression, Env expressionEnvironment, out List<string> errors)
    {
        errors = [];
        try
        {
            expression = expression.Trim();
            var parsedExpression = Parser.Deserialize(expression);
            var result = parsedExpression.EvaluateDirectUsingEnv(expressionEnvironment);

            if (result.HasValue && result.Value is T convertedResult)
            {
                return convertedResult;
            }
            else
            {
                errors.Add(result.HasValue ? $"Expression evaluation returned {result.Value.GetType()} different from {typeof(T)}."
                    : "Expression did not return any value.");
            }
        }
        catch (ParserException pEx)
        {
            errors.Add($"Unable to parse the expression. {pEx.Message}");
        }
        catch (Exception)
        {
            errors.Add("Error occurred while evaluating the expression.");
        }

        return default;
    }

    /// <summary>
    /// Parse and Evaluate the input expression using Willow Expression Library.
    /// </summary>
    /// <typeparam name="T">Type of result.</typeparam>
    /// <param name="expression">String Input expression</param>
    /// <param name="errors">List of errors occurred while evaluation.</param>
    /// <returns>Response.</returns>
    public T? EvaluateUsingDefaultEnv<T>(string expression, out List<string> errors)
    {
        var defaultEnv = GetUMDefaultEnvironment();
        return Evaluate<T>(expression, defaultEnv, out errors);
    }

    /// <summary>
    /// Gets the default UM Environment for Willow Expression evaluation.
    /// </summary>
    /// <returns>Willow Expression Environment.</returns>
    public Env GetUMDefaultEnvironment()
    {
        var expressionEnvironment = Env.Empty.Push();
        AssignDynamicUMVariables(expressionEnvironment);
        return expressionEnvironment;
    }

    /// <summary>
    /// Evaluate and return the expression status and out errors if any
    /// </summary>
    /// <param name="expressionEnv">Expression Environment.</param>
    /// <param name="expression">Expression string to evaluate.</param>
    /// <param name="errors">Out Errors to return after evaluation.</param>
    /// <returns>ExpressionStatus Enum.</returns>
    public ExpressionStatus GetExpressionStatus(Env expressionEnv, string? expression, out List<string> errors)
    {
        // if empty expression return active
        if (string.IsNullOrWhiteSpace(expression))
        {
            errors = [];
            return ExpressionStatus.Active;
        }

        var expResult = Evaluate<bool>(expression, expressionEnv, out errors);

        return errors.Count != 0 ? ExpressionStatus.Error : (expResult ? ExpressionStatus.Active : ExpressionStatus.Inactive);
    }

    private static void AssignDynamicUMVariables(Env environment)
    {
        environment.Assign("UTCNOW", DateTime.UtcNow);
        environment.Assign("NOW", DateTime.Now);
    }
}
