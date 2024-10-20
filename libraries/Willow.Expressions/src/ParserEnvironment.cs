using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.ExpressionParser;

/// <summary>
/// Parser Environment maps variable names to their actual .NET types
/// allowing TokenVariableAccess to have a correct Type on it
/// </summary>
public readonly struct ParserEnvironment
{
    private readonly Dictionary<string, Type> variableTypes = new();
    private readonly List<RegisteredFunction> functionTypes = new();

    public ParserEnvironment()
    {
    }

    /// <summary>
    /// Add a variable declaration to the Parser Environment
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <param name="type">The type of the variable</param>
    public void AddVariable(string name, Type type)
    {
        if (variableTypes.TryGetValue(name, out Type? existingType))
        {
            if (type != existingType) throw new ArgumentException($"Cannot add {name} as both {type.Name} and {existingType.Name}");

            // but if it's the same, just ignore it
        }
        else
        {
            variableTypes.Add(name, type);
        }
    }

    /// <summary>
    /// Try getting the type for a variable
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool TryGetVariable(string name, out Type? type)
    {
        return variableTypes.TryGetValue(name, out type);
    }

    /// <summary>
    /// Add a function specification to the Parser Environment
    /// </summary>
    public void AddFunction(RegisteredFunction function)
    {
        var sameNames =
            this.functionTypes.Where(x => x.Name.Equals(function.Name, StringComparison.InvariantCultureIgnoreCase));

        var returnTypes = Enumerable.Repeat(function, 1).Concat(sameNames)
            .Select(f => f.ResultType)
            .Distinct().ToList();

        if (returnTypes.Count > 1)
            throw new ArgumentException($"Cannot have two different types for one function {function.Name} -> {string.Join(", ", returnTypes.Select(t => t.Name))}");

        functionTypes.Add(function);
    }

    /// <summary>
    /// Try getting the type for a function
    /// </summary>
    public bool TryGetFunction(string name, Type[] args, out Type type)
    {
        foreach (var function in this.functionTypes)
        {
            if (function.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                function.ArgumentTypes.Length == args.Length)
            {
                // TODO: Check arguments, but issue with DatePart and unknown variable to handle
                //var matched = args.Zip(function.ArgumentTypes, (a, b) => a == b || a==typeof(DatePart) || b==typeof(DatePart)).All(x => x);
                //if (!matched) return false;
                type = function.ResultType;
                return true;
            }
        }
        type = typeof(object);
        return false;
    }

    /// <summary>
    /// Get all matching functions with the same name
    /// </summary>
    /// <param name="name"></param>
    /// <returns>A list of registered functions with this name</returns>
    public RegisteredFunction[] GetFunctions(string name)
    {
        return
            this.functionTypes.Where(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
    }

    /// <summary>
    /// Add multiple functions to the environment
    /// </summary>
    /// <param name="registeredFunctions"></param>
    public void AddFunctions(IEnumerable<RegisteredFunction> registeredFunctions)
    {
        foreach (var function in registeredFunctions)
        {
            AddFunction(function);
        }
    }
}
