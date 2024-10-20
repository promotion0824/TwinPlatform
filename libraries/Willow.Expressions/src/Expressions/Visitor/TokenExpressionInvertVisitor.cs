using System;
using System.Linq;

namespace Willow.Expressions.Visitor;

/// <summary>
/// Invert a token expression (only works when there is a single x on the RHS)
/// </summary>
public class TokenExpressionInvertVisitor : TokenExpressionVisitor
{
    // y = f (x) is the input
    // x = f'(y) is the output

    // Essentially we look for the variable 'x' in the tree and then try to move everthing
    // else to the other side.

    private readonly TokenExpressionVariableAccess x;
    private readonly TokenExpression y;

    /// <summary>
    /// Create a new instance of the <see cref="TokenExpressionInvertVisitor"/> class
    /// </summary>
    public TokenExpressionInvertVisitor(TokenExpression y, TokenExpressionVariableAccess x)
    {
        this.x = x;
        this.y = y;
    }

    private bool ContainsInputVariable(TokenExpression t)
    {
        // Ideally would not use symbols but actual TokenExpressions
        return t.UnboundVariables.Contains(x.VariableName);
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionMultiply input)
    {
        var containingX = input.Children.Where(ContainsInputVariable).ToList();
        var notContainingX = input.Children.Except(containingX).ToArray();

        if (containingX.Count() == 0) throw new Exception($"Cannot invert {input.Serialize()} yet");
        if (containingX.Count() > 1) throw new Exception($"Cannot invert {input.Serialize()} yet");

        // y = 3 * f(x)
        // f(x) = y / 3
        // f'(x) = invert(y / 3)

        var step = new TokenExpressionDivide(this.y, new TokenExpressionMultiply(notContainingX));
        var fx = containingX.Single();

        var fprimex = new TokenExpressionInvertVisitor(step, x).Visit(fx);
        return fprimex;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionVariableAccess input)
    {
        if (input.VariableName == x.VariableName) return this.y;
        // Otherwise leave it alone, we don't know what this variable is
        return input;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionFunctionCall input)
    {
        // TODO: Invert function if it contains X
        return new TokenExpressionFunctionCall(input.FunctionName, input.Type, input.Children.Select(c => c.Accept(this)).ToArray()) { Text = input.Text };
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionPower input)
    {
        // TODO: Invert power
        throw new NotImplementedException("Power cannot be inverted yet");
        //return new TokenExpressionPower(input.Left.Accept(this), input.Right.Accept(this)) { Original = input.Original };
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionSubtract input)
    {
        if (ContainsInputVariable(input.Left) == ContainsInputVariable(input.Right))
            throw new Exception($"Cannot invert {input.Serialize()}");

        if (ContainsInputVariable(input.Left))
        {
            // y = f(x) - 3
            // f(x) = y + 3
            // f'(x) = invert(y + 3)
            var step = new TokenExpressionAdd(this.y, input.Right);
            var fprimex = new TokenExpressionInvertVisitor(step, x).Visit(input.Left);
            return fprimex;
        }
        else
        {
            // y = 3 - f(x)
            // f(x) = 3 - y
            // f'(x) = invert(3 - y)
            var step = new TokenExpressionSubtract(input.Left, this.y);
            var fprimex = new TokenExpressionInvertVisitor(step, x).Visit(input.Right);
            return fprimex;
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionAdd input)
    {
        var containingX = input.Children.Where(ContainsInputVariable).ToList();
        var notContainingX = input.Children.Except(containingX).ToArray();

        if (containingX.Count == 0) throw new Exception($"Cannot invert {input.Serialize()} yet");
        if (containingX.Count > 1) throw new Exception($"Cannot invert {input.Serialize()} yet");

        // y = 3 + f(x)
        // f(x) = y - 3
        // f'(x) = invert(y - 3)

        var step = new TokenExpressionSubtract(this.y, new TokenExpressionAdd(notContainingX));
        var fx = containingX.Single();

        var fprimex = new TokenExpressionInvertVisitor(step, x).Visit(fx);
        return fprimex;
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionDivide input)
    {
        if (ContainsInputVariable(input.Left) == ContainsInputVariable(input.Right))
            throw new Exception($"Cannot invert {input.Serialize()}");

        if (ContainsInputVariable(input.Left))
        {
            // y = f(x) / 3
            // f(x) = y * 3
            // f'(x) = invert(y * 3)
            var step = new TokenExpressionMultiply(this.y, input.Right);
            var fprimex = new TokenExpressionInvertVisitor(step, x).Visit(input.Left);
            return fprimex;
        }
        else
        {
            // y = 3 / f(x)
            // f(x) = 3 / y
            // f'(x) = invert(3 / y)
            var step = new TokenExpressionDivide(input.Left, this.y);
            var fprimex = new TokenExpressionInvertVisitor(step, x).Visit(input.Right);
            return fprimex;
        }
    }

    /// <summary>
    /// Visit
    /// </summary>
    public override TokenExpression DoVisit(TokenExpressionUnaryMinus input)
    {
        if (ContainsInputVariable(input))
        {
            // y = -f(x)
            // f(x) = -y
            // f'(x) = f'(-y)
            var negativeY = new TokenExpressionUnaryMinus(y);
            return new TokenExpressionInvertVisitor(negativeY, x).Visit(input.Child);
        }
        // Otherwise, leave it
        return input;
    }
}
