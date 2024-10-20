using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;

namespace Willow.ExpressionParser;

/// <summary>
/// Serialize/Deserialize TokenExpressions from a string
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Deserialize a <see cref="TokenExpression"/> from a string
    /// </summary>
    /// <remarks>
    /// May throw a parser exception
    /// </remarks>
    public static TokenExpression Deserialize(string input, ParserEnvironment? env = null)
    {
        ParserEnvironment envNotNull = env ?? new ParserEnvironment();

        var tokens = Scan(input, 0).GetEnumerator();
        if (!tokens.MoveNext()) return TokenExpression.True;
        return ParseRecursive(tokens, envNotNull);
    }

    /// <summary>
    /// Serialize a TokenExpression
    /// </summary>
    public static string Serialize(TokenExpression tokenExpression)
    {
        return tokenExpression.Serialize();
    }

    private static TokenExpressionTuple ReadTupleFunction(TokenExpression[] args)
    {
        if (args.Length < 1) throw new ParserException($"Tuple needs at least one argument");
        return new TokenExpressionTuple(args);
    }

    private static TokenExpressionSum ReadSumFunction(TokenExpression[] args)
    {
        if (args.Length < 1) throw new ParserException($"Sum needs at least one argument");

        if (args.Length == 1 && args[0] is TokenExpressionVariableAccess v) return new TokenExpressionSum(v);
        if (args.Length == 1 && args[0] is TokenExpressionPropertyAccess p) return new TokenExpressionSum(p);
        if (args.Length == 1 && args[0] is TokenExpressionArray a) return new TokenExpressionSum(a);
        return new TokenExpressionSum(new TokenExpressionArray(args));
    }

    private static TokenExpressionTernary ReadTernaryFunction(TokenExpression[] args)
    {
        if (args.Length == 3)
            return new TokenExpressionTernary(args[0], args[1], args[2]);
        else
            throw new ParserException($"Ternary IF needs three arguments");
    }

    private static TokenExpressionVariableAccess CoerceParameterToVariableAccess(TokenExpression arg, ParserEnvironment env)
    {
        TokenExpressionVariableAccess variableAccess;
        if (arg is TokenExpressionConstantString cs)
        {
            // LOOK IT UP
            env.TryGetVariable(cs.ValueString, out Type? type);
            variableAccess = new TokenExpressionVariableAccess(cs.ValueString, type);
        }
        else if (arg is TokenExpressionVariableAccess va)
        {
            variableAccess = va;
        }
        else
        {
            throw new ParserException("Missing variable name for first parameter");
        }
        return variableAccess;
    }

    private static TokenExpression ReadEachFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length != 3) throw new ParserException("Each() takes three arguments: an enumeration, a variable name, and a body expression");
        if (args[1] is TokenExpressionVariableAccess va) return new TokenExpressionEach(args[0], va, args[2]);
        throw new ParserException("Each() takes three arguments: an enumeration, a variable name, and a body expression, The second argument must be a simple identifier.");
    }

    private static TokenExpression ReadAnyFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionAny(args[0]);
        }

        return ReadTemporalFunction("ANY", args, env);
    }

    private static TokenExpression ReadAllFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionAll(args[0]);
        }

        return ReadTemporalFunction("ALL", args, env);
    }

    private static TokenExpression ReadFailedFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1 || args.Length == 2)
        {
            return new TokenExpressionFailed(args);
        }

        throw new ParserException($"Failed() does not take {args.Length} arguments");
    }

    private static TokenExpression ReadCountFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionCount(args[0]);
        }

        return ReadTemporalFunction("COUNT", args, env);
    }

    private static TokenExpression ReadSumFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionSum(args[0]);
        }

        throw new ParserException($"SUM() does not take {args.Length} arguments");
    }

    private static TokenExpression ReadAverageFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionAverage(args[0]);
        }

        return ReadTemporalFunction("AVERAGE", args, env);
    }

    private static TokenExpression ReadMinFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionMin(args[0]);
        }

        return ReadTemporalFunction("MIN", args, env);
    }

    private static TokenExpression ReadMaxFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionMax(args[0]);
        }

        return ReadTemporalFunction("MAX", args, env);
    }

    private static TokenExpression ReadFirstFunction(TokenExpression[] args, ParserEnvironment env)
    {
        if (!args.Any()) throw new ParserException("First() must have at least one argument");
        return new TokenExpressionFirst(new TokenExpressionArray(args));
    }

    private static TokenExpression ReadTemporalFunction(string functionName, TokenExpression[] args, ParserEnvironment env)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionTemporal(functionName, child: args[0]);
        }

        if (args.Length == 2)
        {
            return new TokenExpressionTemporal(functionName, child: args[0], timePeriod: args[1]);
        }

        if (args.Length == 3)
        {
            return new TokenExpressionTemporal(functionName, child: args[0], timePeriod: args[1], timeFrom: args[2]);
        }

        if (args.Length == 4)
        {
            return new TokenExpressionTemporal(functionName, child: args[0], timePeriod: args[1], timeFrom: args[2], unitOfMeasure: args[3]);
        }

        throw new ParserException($"{functionName}() does not take {args.Length} arguments");
    }

    private static TokenExpressionTimer ReadTimerFunction(TokenExpression[] args)
    {
        if (args.Length == 1)
        {
            return new TokenExpressionTimer(child: args[0], unitOfMeasure: null);
        }

        if (args.Length == 2)
        {
            return new TokenExpressionTimer(child: args[0], unitOfMeasure: args[1]);
        }

        throw new ParserException($"TIMER() does not take {args.Length} arguments");
    }

    private static TokenExpression ReadDeltaFunction(TokenExpression[] args, ParserEnvironment env)
    {
        return ReadTemporalFunction("DELTA", args, env);
    }

    private static TokenExpression ReadDeltaTimeFunction(TokenExpression[] args, ParserEnvironment env)
    {
        //JK: Will we ever use the DELTA_TIME as temporal with time period and time from?
        if (args.Length == 1)
        {
            return new TokenExpressionTemporal("DELTA_TIME", child: args[0]);
        }

        if (args.Length == 2)
        {
            return new TokenExpressionTemporal("DELTA_TIME", child: args[0], unitOfMeasure: args[1]);
        }

        throw new ParserException($"DELTA_TIME() does not take {args.Length} arguments");
    }

    private static TokenExpression ReadStandardDeviationFunction(TokenExpression[] args, ParserEnvironment env)
    {
        return ReadTemporalFunction("STND", args, env);
    }

    private static TokenExpression ReadSlopeFunction(TokenExpression[] args, ParserEnvironment env)
    {
        return ReadTemporalFunction("SLOPE", args, env);
    }

    private static TokenExpression ReadForecastFunction(TokenExpression[] args, ParserEnvironment env)
    {
        return ReadTemporalFunction("FORCAST", args, env);
    }

    /// <summary>
    /// Unary expressions need a little help as the same - operator has different precedence
    /// We throw a sentinel on the stack to indicate that we are in a unary expression
    /// </summary>
    private static readonly TokenExpression SentinelForUnary = TokenExpressionConstant.Create("Sentinel");

    private static TokenExpression ParseRecursive(IEnumerator<Token> enumerator, ParserEnvironment env)
    {
        var arguments = new Stack<TokenExpression>();
        var operators = new Stack<OperatorToken>();

        bool readingExpression = true;

        // TODO: Implement shunting yard algorithm for operator precedence

        while (readingExpression)
        {
            if (enumerator.Current == Token.LParen)
            {
                if (!enumerator.MoveNext()) throw new ParserException("Parentheses mismatch");
                var innerExpression = ParseRecursive(enumerator, env);
                if (enumerator.Current != Token.RParen) throw new ParserException("Parentheses mismatch");
                arguments.Push(innerExpression);
                if (!enumerator.MoveNext()) break;
            }
            else if (enumerator.Current == Token.LCurlyParen)
            {
                // Used for lists or arrays { 1, 2, 3}
                if (!enumerator.MoveNext()) throw new ParserException("Curly parentheses mismatch");

                var args = new List<TokenExpression>(2);
                bool running = enumerator.Current != Token.RCurlyParen;
                while (running)
                {
                    var argument = ParseRecursive(enumerator, env);
                    args.Add(argument);
                    running = enumerator.Current == Token.Comma;
                    if (running)
                    {
                        enumerator.MoveNext();  // swallow the comma
                    }
                }

                if (enumerator.Current != Token.RCurlyParen) throw new ParserException("Missing closing curly parenthesis on array");

                var innerExpression = new TokenExpressionArray(args.ToArray());
                arguments.Push(innerExpression);
                if (!enumerator.MoveNext()) break;
            }
            /*else if (enumerator.Current == Token.LSquareParen)
            {
                if (!enumerator.MoveNext()) throw new ParserException("Parentheses mismatch");
                var args = ReadCommaSeparatedValuesUpTo(enumerator, Token.RSquareParen, env);
                var innerExpression = new TokenExpressionArray(args.ToArray());
                arguments.Push(innerExpression);
                if (!enumerator.MoveNext()) break;
                break;
            }*/
            else if (enumerator.Current == Token.UnaryNot)
            {
                arguments.Push(SentinelForUnary);
            }
            else if (enumerator.Current == Token.Minus)
            {
                arguments.Push(SentinelForUnary);
            }
            else
            {
                if (enumerator.Current is NumberConstantToken valueConstant)
                {
                    var result = TokenExpressionConstant.Create(valueConstant.Value);

                    if (!enumerator.MoveNext())
                    {
                        // nothing else, ends here
                        arguments.Push(result);
                        break;
                    }

                    // Peek at next value, if it's a Unit of measure, use it
                    // Note % is not an identitifier and is handled below

                    if (enumerator.Current is IdentifierToken identifierToken)
                    {
                        string unit = "";

                        if (Unit.TryGetUnit(identifierToken.Value, out var unitOfMeasure))
                        {
                            unit = unitOfMeasure!.Name;
                        }
                        else
                        {
                            switch (identifierToken.Value.ToUpper())
                            {
                                case "AND":
                                case "OR":
                                case "%":
                                    //these are the only tokens that qualify to parse the identifier
                                    break;
                                default:
                                    {
                                        //otherwise assume it is an unidentified unit
                                        unit = identifierToken.Value;
                                        break;
                                    }
                            }
                        }

                        if (!string.IsNullOrEmpty(unit))
                        {
                            result.Unit = unit;
                            arguments.Push(result);

                            // And swallow this identifier and move to the next
                            if (!enumerator.MoveNext()) break;
                        }
                        else
                        {
                            // did not recognize unit of measure
                            // don't swallow it, continue after pushing just the number
                            arguments.Push(result);
                        }
                    }
                    else
                    {
                        arguments.Push(result);

                        // And don't move on again, already moved
                    }
                }
                else if (enumerator.Current is QuotedStringToken valueQuoted)
                {
                    var result = TokenExpressionConstant.Create(valueQuoted.Value);
                    arguments.Push(result);
                    if (!enumerator.MoveNext()) break;
                }
                else if (enumerator.Current is IdentifierToken identifier)
                {
                    // Could be a function or a variable access, or an expression?

                    // If we are at the very end of the input it can only be a variable (or constant)
                    if (!enumerator.MoveNext())
                    {
                        var tokenExpression = VariableOrSpecialValue(env, identifier.Value);
                        arguments.Push(tokenExpression);
                        break;
                    }

                    // A function call: Identifier(...)
                    if (enumerator.Current == Token.LParen)
                    {
                        if (!enumerator.MoveNext()) throw new ParserException("Missing function body");

                        var functionName = identifier;

                        // Read the arguments to the function and collect them

                        var args = new List<TokenExpression>(2);
                        bool running = enumerator.Current != Token.RParen;
                        while (running)
                        {
                            var argument = ParseRecursive(enumerator, env);
                            args.Add(argument);
                            running = enumerator.Current == Token.Comma;
                            if (running)
                            {
                                enumerator.MoveNext();  // swallow the comma
                            }
                        }

                        if (enumerator.Current != Token.RParen) throw new ParserException("Missing close parenthesis on call to " + functionName);

                        switch (functionName.Value.ToUpperInvariant())
                        {
                            case "FAILED":
                                {
                                    var result = ReadFailedFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "COUNT":
                                {
                                    var result = ReadCountFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "COUNTLEADING":
                                {
                                    var result = ReadTemporalFunction("COUNTLEADING", args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "SUM":
                                {
                                    var result = ReadSumFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "AVERAGE":
                                {
                                    var result = ReadAverageFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "EACH":
                                {
                                    var result = ReadEachFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "ANY":
                                {
                                    var result = ReadAnyFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "ALL":
                                {
                                    var result = ReadAllFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "FIRST":
                                {
                                    var result = ReadFirstFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "MIN":
                                {
                                    var result = ReadMinFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "MAX":
                                {
                                    var result = ReadMaxFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "DELTA":
                                {
                                    var result = ReadDeltaFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "DELTA_TIME":
                                {
                                    var result = ReadDeltaTimeFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "STND":
                                {
                                    var result = ReadStandardDeviationFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "SLOPE":
                                {
                                    var result = ReadSlopeFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "FORECAST":
                                {
                                    var result = ReadForecastFunction(args.ToArray(), env);
                                    arguments.Push(result);
                                    break;
                                }
                            case "IF":
                                {
                                    var result = ReadTernaryFunction(args.ToArray());
                                    arguments.Push(result);
                                    break;
                                }
                            case "TUPLE":
                                {
                                    var result = ReadTupleFunction(args.ToArray());
                                    arguments.Push(result);
                                    break;
                                }
                            case nameof(TokenExpressionSetUnion):
                                {
                                    var result = new TokenExpressionSetUnion(args.ToArray());
                                    arguments.Push(result);
                                    break;
                                }
                            case "DATETIME":
                                {
                                    if (args.Count == 1 && args[0] is TokenExpressionConstantString tcs)
                                    {
                                        var result = TokenExpressionConstant.Create(DateTime.Parse(tcs.ValueString));
                                        arguments.Push(result);
                                    }
                                    else if (args.Count == 3 && args.All(x => x is TokenDouble))
                                    {
                                        var intArgs = args.Cast<TokenDouble>().Select(x => (int)x.ValueDouble).ToList();
                                        var result = TokenExpressionConstant.Create(new DateTime(intArgs[0], intArgs[1], intArgs[2]));
                                        arguments.Push(result);
                                    }
                                    else if (args.Count == 5 && args.All(x => x is TokenDouble))
                                    {
                                        var intArgs = args.Cast<TokenDouble>().Select(x => (int)x.ValueDouble).ToList();
                                        var result = TokenExpressionConstant.Create(new DateTime(intArgs[0], intArgs[1], intArgs[2], intArgs[3], intArgs[4], 0));
                                        arguments.Push(result);
                                    }
                                    else if (args.Count == 6 && args.All(x => x is TokenDouble))
                                    {
                                        var intArgs = args.Cast<TokenDouble>().Select(x => (int)x.ValueDouble).ToList();
                                        var result = TokenExpressionConstant.Create(new DateTime(intArgs[0], intArgs[1], intArgs[2], intArgs[3], intArgs[4], intArgs[5]));
                                        arguments.Push(result);
                                    }
                                    else
                                    {
                                        throw new ParserException("DateTime() takes one argument, a string like 'DateTime(\"2019-11-14T12:34:56\")'");
                                    }
                                    break;
                                }
                            case "DATETIMEOFFSET":
                                {
                                    if (args.Count == 1 && args[0] is TokenExpressionConstantString tcs)
                                    {
                                        var result = TokenExpressionConstant.Create(DateTimeOffset.Parse(tcs.ValueString));
                                        arguments.Push(result);
                                    }

                                    // TODO DateTimeOffset ...
                                    else if (args.Count == 3 && args.All(x => x is TokenDouble))
                                    {
                                        var intArgs = args.Cast<TokenDouble>().Select(x => (int)x.ValueDouble).ToList();
                                        var result = TokenExpressionConstant.Create(new DateTime(intArgs[0], intArgs[1], intArgs[2]));
                                        arguments.Push(result);
                                    }
                                    else if (args.Count == 5 && args.All(x => x is TokenDouble))
                                    {
                                        var intArgs = args.Cast<TokenDouble>().Select(x => (int)x.ValueDouble).ToList();
                                        var result = TokenExpressionConstant.Create(new DateTime(intArgs[0], intArgs[1], intArgs[2], intArgs[3], intArgs[4], 0));
                                        arguments.Push(result);
                                    }
                                    else if (args.Count == 6 && args.All(x => x is TokenDouble))
                                    {
                                        var intArgs = args.Cast<TokenDouble>().Select(x => (int)x.ValueDouble).ToList();
                                        var result = TokenExpressionConstant.Create(new DateTime(intArgs[0], intArgs[1], intArgs[2], intArgs[3], intArgs[4], intArgs[5]));
                                        arguments.Push(result);
                                    }
                                    else
                                    {
                                        throw new ParserException("DateTimeOffset() takes one argument, a string like 'DateTime(\"2019-11-14T12:34:56\")'");
                                    }
                                    break;
                                }
                            case "TIMER":
                                {
                                    var result = ReadTimerFunction(args.ToArray());
                                    arguments.Push(result);
                                    break;
                                }
                            default:
                                Type? type = null;

                                // Try to find the Type of the function from the environment
                                if (env.TryGetFunction(functionName.Value, args.Select(t => t.Type).ToArray(), out type))
                                {
                                    var result = new TokenExpressionFunctionCall(functionName.Value, type, args.ToArray());
                                    arguments.Push(result);
                                }
                                else
                                {
                                    //throw new ParserException($"Could not match {functionName.Value}, with those arguments");
                                    var result = new TokenExpressionFunctionCall(functionName.Value, typeof(object), args.ToArray());
                                    arguments.Push(result);
                                }
                                break;
                        }
                        if (enumerator.Current != Token.RParen)
                            throw new ParserException("Incomplete function, missing closing parenthesis");
                        if (!enumerator.MoveNext()) break;
                    }
                    else
                    {
                        var result = VariableOrSpecialValue(env, identifier.Value);
                        arguments.Push(result);

                        // have already consumed it checking for an `(` so don't move next

                        //                        if (!enumerator.MoveNext()) break;
                    }
                }
            }

            // Now read an operator

            if (!arguments.Any()) throw new ParserException("Should have an argument by this point");

            OperatorToken op1;

            if (enumerator.Current == Token.RParen
                || enumerator.Current == Token.RSquareParen
                || enumerator.Current == Token.RCurlyParen
                || enumerator.Current == Token.Comma)
            {
                break;
            }
            else if (enumerator.Current == Token.SemiColon)
            {
                // ; at end of input can be ignored
                if (!enumerator.MoveNext()) break;

                // Otherwise ; acts like an AND
                op1 = Token.And;
            }
            else if (enumerator.Current == Token.Minus && arguments.First() == SentinelForUnary)
            {
                // Unary minus
                op1 = Token.UnaryMinus;
                if (!enumerator.MoveNext())
                    throw new ParserException($"Incomplete expression after {enumerator.Current} ...");
            }
            else if (enumerator.Current == Token.UnaryNot && arguments.Last() == SentinelForUnary)
            {
                op1 = Token.UnaryNot;
                if (!enumerator.MoveNext())
                    throw new ParserException($"Incomplete expression after {enumerator.Current} ...");
            }
            else if (enumerator.Current is OperatorToken token)
            {
                op1 = token;
                if (!enumerator.MoveNext())
                    throw new ParserException($"Incomplete expression after {enumerator.Current} ...");
            }
            else
            {
                // Check for string valued operators (OR, AND, ...)
                if (enumerator.Current is IdentifierToken identifier)
                {
                    // Can handle any string valued operators here
                    switch (identifier.Value.ToUpper())
                    {
                        case "AND":
                            op1 = Token.And;
                            break;

                        case "OR":
                            op1 = Token.Or;
                            break;

                        default:
                            {
                                // It might be a unit of measure after the last token
                                if (Unit.TryGetUnit(identifier.Value, out var unitOfMeasure))
                                {
                                    var lhs1 = arguments.Peek();
                                    lhs1.Unit = unitOfMeasure!.Name;
                                    op1 = new OperatorToken(unitOfMeasure.Name, 1);
                                    break;
                                }
                                else
                                {
                                    throw new ParserException(
                                    $"Unexpected token '{enumerator.Current}', expected an operator, comma, or end of expression");
                                }
                            }
                    }

                    if (op1 == Token.And || op1 == Token.Or)
                    {
                        // And consume the identifier which we have taken as an operator
                        if (!enumerator.MoveNext())
                            throw new ParserException($"Incomplete expression after {enumerator.Current} ...");
                    }
                    else
                    {
                        // Any postfix operator
                        if (!enumerator.MoveNext()) break;

                        continue;
                    }
                }
                else
                {
                    throw new ParserException(
                    $"Unexpected token '{enumerator.Current}', expected an operator, comma, or end of expression");
                }
            }

            // Now reduce: if this operator is lower priority than the one before then reduce the one before

            void reduceMultiple()
            {
                if (!operators.Any()) return;

                var opTop = operators.Pop();

                var rhs = arguments.Pop();

                if (operators.Any() && operators.Peek().Precedence == opTop.Precedence)
                {
                    // Save the RHS for a moment, go reduce the LHS
                    reduceMultiple();

                    // And now combine THAT with the RHS
                    var lhs = arguments.Pop();
                    arguments.Push(CreateExpression(lhs, opTop, rhs, env));
                }
                else
                {
                    var lhs = arguments.Pop();
                    arguments.Push(CreateExpression(lhs, opTop, rhs, env));
                }
            }

            // See https://en.wikipedia.org/wiki/Operator-precedence_parser
            // ^ This is how it should probably have been written

            while (operators.Any())
            {
                var op2 = operators.Peek();

                // ReSharper disable once PossibleNullReferenceException
                if (op2.Precedence > op1.Precedence)
                {
                    // There may be multiple arguments on the stack this.prop1.prop2
                    // if they have the same precedence as each other they need to
                    // be parsed left to right

                    // e.g. a*b*c + 2 = (a*b)*c + 2
                    // e.g. a+b+c * 2 = (a + b) + (c*2)
                    // + and * are commutative but dot is not: a.b.c == (a.b).c

                    reduceMultiple();
                }
                else
                {
                    break;
                }
            }

            // And finally push the new operator onto the stack
            operators.Push(op1);

            // The operators on the stack are in ascending or equal priority

            if (operators.Count > arguments.Count) throw new ParserException("Internal error, not enough arguments");
        }

        // Reduce the stack, taking anything
        // at this point the operators are in same or ascending order of precedence
        while (operators.Any())
        {
            var opQueue = new Stack<OperatorToken>();
            var argQueue = new Stack<TokenExpression>();

            var op = operators.Pop();
            var rhs = arguments.Pop();

            // If the previous operator has the same priority as this one, then need to evaluate left to right
            // Assume left associative
            while (operators.Any() && op.Precedence == operators.Peek().Precedence)
            {
                opQueue.Push(op);
                argQueue.Push(rhs);
                rhs = arguments.Pop();
                op = operators.Pop();
            }

            if (!arguments.Any())
            {
                throw new ParserException($"Missing argument after {op.Value}");
            }

            // Grab the final item
            var lhs = arguments.Pop();

            // And push the final onto the Queue
            opQueue.Push(op);
            argQueue.Push(rhs);

            while (opQueue.Any())
            {
                lhs = CreateExpression(lhs, opQueue.Pop(), argQueue.Pop(), env);
            }

            // And finally we have an op and a RHS
            arguments.Push(lhs);
        }

        // Should be just one left on the stack
        if (arguments.Count > 1) throw new Exception("Internal error, did not reduce stack");
        return arguments.Pop();
    }

    private static TokenExpression VariableOrSpecialValue(ParserEnvironment env, string identifier)
    {
        // Special constants
        if (identifier.Equals("true", StringComparison.InvariantCultureIgnoreCase))
        {
            return TokenExpression.True;
        }

        if (identifier.Equals("false", StringComparison.InvariantCultureIgnoreCase))
        {
            return TokenExpression.False;
        }

        if (identifier.Equals("null", StringComparison.InvariantCultureIgnoreCase))
        {
            return TokenExpression.Null;
        }

        if (identifier.Equals("pi", StringComparison.InvariantCultureIgnoreCase))
        {
            return TokenExpressionConstant.Create(Math.PI);
        }

        env.TryGetVariable(identifier, out Type? type);
        return new TokenExpressionVariableAccess(identifier, type);
    }

    private static List<TokenExpression> ReadCommaSeparatedValuesUpTo(IEnumerator<Token> enumerator, Token endParenthesis, ParserEnvironment env)
    {
        var args = new List<TokenExpression>(2);
        bool running = enumerator.Current != endParenthesis;
        while (running)
        {
            var argument = ParseRecursive(enumerator, env);
            args.Add(argument);
            running = enumerator.Current == Token.Comma;
            if (running)
            {
                enumerator.MoveNext(); // swallow the comma
            }
        }

        if (enumerator.Current != endParenthesis)
            throw new ParserException($"Missing close parenthesis '{endParenthesis}'");
        return args;
    }

    private static TokenExpression CreateExpression(TokenExpression lhs,
        OperatorToken op,
        TokenExpression rhs,
        ParserEnvironment env)
    {
        if (op is null) throw new ArgumentNullException(nameof(op));
        if (lhs is null) throw new ArgumentNullException(nameof(lhs));
        if (rhs is null) throw new ArgumentNullException(nameof(rhs));

        //Console.WriteLine($"Combined: {lhs} {op} {rhs}");
        // TODO: LOTS of other type checks possible on arguments to these

        if (op == Token.UnaryMinus)
        {
            if (lhs == SentinelForUnary)
            {
                return new TokenExpressionUnaryMinus(rhs);
            }
        }

        if (op == Token.UnaryNot)
        {
            if (lhs == SentinelForUnary)
            {
                return new TokenExpressionNot(rhs);
            }
        }

        if (op == Token.And)
        {
            return new TokenExpressionAnd(lhs, rhs);
        }
        else if (op == Token.Or)
        {
            return new TokenExpressionOr(lhs, rhs);
        }
        else if (op == Token.Plus)
        {
            return new TokenExpressionAdd(lhs, rhs);
        }
        else if (op == Token.Dot)
        {
            if (rhs is TokenExpressionVariableAccess tva)
            {
                var prop = lhs.Type.GetProperty(tva.VariableName);
                var newType = prop?.PropertyType ?? typeof(object);
                var dottedResult = new TokenExpressionPropertyAccess(lhs, newType, tva.VariableName);

                // BUT if the dotted result can be found in the Environment, use that instead
                // e.g. Env says "Sales.Product.Name" is a string field
                if (dottedResult.IsVariableProperty)
                {
                    string dottedName = dottedResult.ToString();       // ToString cheaper than Serialize
                    if (env.TryGetVariable(dottedName, out newType))
                    {
                        return new TokenExpressionVariableAccess(dottedName, newType);
                    }
                }

                return dottedResult;
            }

            // Seeing some parses where the RHS is a property access and the LHS is a variable
            // If these are a full dotted variable name in the env, use that instead (hate dotted variable names!!)
            if (lhs is TokenExpressionVariableAccess tva2 && rhs is TokenExpressionPropertyAccess tep)
            {
                // Try combining them, see if the combined versio
                // Expression is being built from right but for dotted properties, need to rebuild from left
                if (tep.IsVariableProperty)
                {
                    string dottedName = tva2.VariableName + "." + rhs.ToString();       // ToString cheaper than Serialize
                    if (env.TryGetVariable(dottedName, out Type? newType))
                    {
                        return new TokenExpressionVariableAccess(dottedName, newType);
                    }
                }
            }

            // search.isMatch('aaa') is a Lucene-style function call
            if (lhs is TokenExpressionVariableAccess tva3 && rhs is TokenExpressionFunctionCall tfc)
            {
                string dottedFunctionName = tva3.VariableName + "." + tfc.FunctionName;
                if (env.GetFunctions(dottedFunctionName).Any())
                {
                    return new TokenExpressionFunctionCall(dottedFunctionName, typeof(object), tfc.Children);
                }
            }

            // LHS is VariableAccess and Rhs is PropertyAccess
            throw new ParserException($"Invalid expression, cannot use 'dot' operator between " +
                $"{lhs.Serialize()}, a {lhs.GetType().Name} of type {(lhs.Type?.Name ?? "null")} and " +
                $"{rhs.Serialize()}, a {rhs.GetType().Name} of type {(rhs.Type?.Name ?? "null")}");
        }
        else if (op == Token.Minus)
        {
            return new TokenExpressionSubtract(lhs, rhs);
        }
        else if (op == Token.Multiply)
        {
            return new TokenExpressionMultiply(lhs, rhs);
        }
        else if (op == Token.Divide)
        {
            return new TokenExpressionDivide(lhs, rhs);
        }
        else if (op == Token.Power)
        {
            return new TokenExpressionPower(lhs, rhs);
        }
        else if (op == Token.Is)
        {
            return new TokenExpressionIs(lhs, rhs);
        }
        else if (op == Token.Equal)
        {
            return new TokenExpressionEquals(lhs, rhs);
        }
        else if (op == Token.NotEqual)
        {
            return new TokenExpressionNotEquals(lhs, rhs);
        }
        else if (op == Token.Less)
        {
            return new TokenExpressionLess(lhs, rhs);
        }
        else if (op == Token.LessEqual)
        {
            return new TokenExpressionLessOrEqual(lhs, rhs);
        }
        else if (op == Token.Greater)
        {
            return new TokenExpressionGreater(lhs, rhs);
        }
        else if (op == Token.GreaterEqual)
        {
            return new TokenExpressionGreaterOrEqual(lhs, rhs);
        }
        else if (op == Token.MemberOf)
        {
            return new TokenExpressionMatches(lhs, rhs);
        }
        else if (op == Token.Union)
        {
            return new TokenExpressionSetUnion(lhs, rhs);
        }
        else if (op == Token.Intersection)
        {
            return new TokenExpressionIntersection(lhs, rhs);
        }

        //else if (op == Token.Power)
        //{
        //    return new FacetExpressionPower(lhs, rhs);
        //}
        else
        {
            throw new ParserException($"Bad operator {op} {op.GetType().Name}");
        }
    }
}
