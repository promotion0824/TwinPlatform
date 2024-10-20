using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace WillowRules.Visitors;


/// <summary>
/// A visitor fopr global variables
/// </summary>
public class BindGlobalVisitor : TokenExpressionVisitor
{
	private readonly Env env;
	private readonly ILogger logger;
	private Func<string, (bool ok, RegisteredFunction registeredFunction)> getGlobal;
	public bool Success { get; private set; } = true;

	/// <summary>
	/// Creates a new <see cref="BindGlobalVisitor"/>
	/// </summary>
	/// <param name="env">This starts with the global variables,maybe</param>
	public BindGlobalVisitor(Env env,
		Func<string, (bool ok, RegisteredFunction registeredFunction)> getGlobal,
		ILogger logger,
		string[]? ignoredIdentifiers = null) : base()
	{
		this.env = env ?? throw new ArgumentNullException(nameof(env));
		this.getGlobal = getGlobal ?? throw new ArgumentNullException(nameof(getGlobal));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.ignoredIdentifiers = ignoredIdentifiers ?? Array.Empty<string>();
	}

	/// <summary>
	/// Wrap an expression up in a Function delcaring it to be a failure
	/// </summary>
	/// <param name="wrapped"></param>
	/// <returns></returns>
	private TokenExpression Fail(TokenExpression wrapped, string? error = null)
	{
		Success = false;
		if (!string.IsNullOrEmpty(error))
		{
			// Not really a warning, bindings failed all the time
			// logger.LogWarning("Error visiting expression {wrapped} for twin {twinId}. {error}", wrapped, root.Id, error);
			return new TokenExpressionFailed(error, wrapped);
		}
		return new TokenExpressionFailed("Failed", wrapped);
	}

	public override TokenExpression DoVisit(TokenExpressionFailed input)
	{
		Success = false;
		return new TokenExpressionFailed(input.Children); // no need to visit children
	}

	private string[] ignoredIdentifiers;

	/// <summary>
	/// Visit a variable
	/// </summary>
	public override TokenExpression DoVisit(TokenExpressionVariableAccess variableAccess)
	{
		string identifier = variableAccess.VariableName;

		if (ignoredIdentifiers.Contains(identifier))
		{
			return variableAccess;
		}

		if (this.env.TryGet(variableAccess.VariableName, out TokenExpression? tokenExpression))
		{
			// If we load a failed expression from the environment we should fail too
			if (tokenExpression is TokenExpressionFailed) Success = false;
			return tokenExpression!;
		}

		(bool ok, RegisteredFunction registeredFunction) = getGlobal(variableAccess.VariableName);

		if (ok && registeredFunction.Body is not null)
		{
			//check failed expressions on registered functions in case
			if (registeredFunction.Body is TokenExpressionFailed)
			{
				Success = false;
				return registeredFunction.Body;
			}

			if (registeredFunction.Arguments.Length > 0)
			{
				return Fail(variableAccess, $"{registeredFunction.Arguments.Length} parameters expected for global");
			}

			return registeredFunction.Body!;
		}

		return variableAccess;
	}

	/// <summary>
	/// Visit function calls and convert the OPTIONS call to the first expression that can be bound
	/// </summary>
	public override TokenExpression DoVisit(TokenExpressionFunctionCall func)
	{
		(bool ok, RegisteredFunction function) = getGlobal(func.FunctionName);

		if (ok && function.Body is not null)
		{
			//check failed expressions on registered functions in case
			if (function.Body is TokenExpressionFailed)
			{
				Success = false;
				return function.Body;
			}

			if (func.Children.Length != function.Arguments.Length)
			{
				return Fail(func, error: $"Parameter count mismatch source count {func.Children.Length} and function count {function.Arguments.Length}");
			}

			var result = function.Body!;

			//replace variables in the macro with the expression parameters of this incoming function
			for (int i = 0; i < function.Arguments.Length; i++)
			{
				var argument = function.Arguments[i];
				var child = func.Children[i];
				var childVisitor = new BindGlobalVisitor(env, getGlobal, logger, ignoredIdentifiers);
				var visited = childVisitor.Visit(child);

				Success &= childVisitor.Success;

				var visitor = new VariableTokenReplacementVisitor(argument.Name, visited);
				result = visitor.Visit(result);
			}

			return result;
		}

		return base.DoVisit(func);
	}
}
