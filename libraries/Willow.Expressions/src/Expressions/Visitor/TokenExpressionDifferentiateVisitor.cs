using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// Differentiates any expression with respect to a provided variable
    /// </summary>
    public class TokenExpressionDifferentiateVisitor : TokenExpressionVisitor
    {
        private readonly TokenExpressionVariableAccess x;

        /// <summary>
        /// Creates a new <see cref="TokenExpressionDifferentiateVisitor"/>
        /// </summary>
        public TokenExpressionDifferentiateVisitor(TokenExpressionVariableAccess x) : base()
        {
            this.x = x;
        }

        /// <summary>
        /// Differentiates a power expression (x ^ n, n ^ x) Limitations: no negative numbers yet, cannot implicitly differentiate yet
        /// </summary>
        /// <param name="input">Power expression to differentiate</param>
        /// <returns></returns>
        public override TokenExpression DoVisit(TokenExpressionPower input)
        {
            // Base and exponent are constants
            if (input.Left is TokenDouble && input.Right is TokenDouble) return TokenExpressionConstant.Create(0);

            // Base is constant, exponent is variable
            if (input.Left is TokenDouble constant)
            {
                // Base is 0 or 1 => derivative is 0
                if (constant.ValueDouble == 0 || constant.ValueDouble == 1) return TokenExpressionConstant.Create(0);
                // Attempting to differentiate with respect to another variable
                if (!input.Right.UnboundVariables.Contains(this.x.VariableName)) return input;
                // Derivative of c ^ x = c ^ x * ln(c)
                TokenExpressionFunctionCall naturalLog = new TokenExpressionFunctionCall("ln", typeof(double), constant);
                return new TokenExpressionMultiply(input, naturalLog, input.Right.Accept(this));
            }

            if (input.Right is TokenDouble d)
            {
                if (d.ValueDouble == 1) { return input.Left.Accept(this); }
                else if (d.ValueDouble == 0) { return TokenExpressionConstant.Create(0); }
                else
                {
                    TokenExpressionPower derivedPower = new TokenExpressionPower(input.Left, d.ValueDouble - 1);

                    return new TokenExpressionMultiply(d, derivedPower, input.Left.Accept(this));
                }
            }

            if (input.Right is TokenExpressionUnaryMinus m && m.Child is TokenDouble child)
            {
                if (child.ValueDouble == -1) { return input.Left.Accept(this); }
                else if (child.ValueDouble == 0) { return TokenExpressionConstant.Create(0); }
                else
                {
                    TokenExpressionPower derivedPower = new TokenExpressionPower(input.Left, -child.ValueDouble - 1);

                    return new TokenExpressionMultiply(TokenExpressionConstant.Create(-child.ValueDouble), derivedPower, input.Left.Accept(this));
                }
            }

            throw new NotImplementedException("Too complicated for now");
        }

        /// <summary>
        /// Differentiates a product using the product rule
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override TokenExpression DoVisit(TokenExpressionMultiply input)
        {
            TokenExpression[] derivedChildren = input.Children.Select(c => c.Accept(this)).ToArray();

            return new TokenExpressionAdd(new TokenExpressionMultiply(derivedChildren[0], input.Children[1]), new TokenExpressionMultiply(derivedChildren[1], input.Children[0]));
            /*TokenExpressionMultiply[] products = new TokenExpressionMultiply[input.Children.Length];

            for (int i = 0; i < input.Children.Length; i++)
            {
                TokenExpression[] allChildren = (TokenExpression[]) input.Children.Clone();
                allChildren[i] = derivedChildren[i];

                products[i] = new TokenExpressionMultiply(allChildren);
            }

            return new TokenExpressionAdd(products);*/
        }

        /// <summary>
        /// Differentiates a variable
        /// </summary>
        /// <param name="input">Variable to differentiate</param>
        /// <returns></returns>
        public override TokenExpression DoVisit(TokenExpressionVariableAccess input)
        {
            if (input.VariableName == this.x.VariableName) return TokenExpressionConstant.Create(1);
            return input;
        }

        /// <summary>
        /// Differentiates a constant
        /// </summary>
        /// <param name="input">Constant to differentiate</param>
        /// <returns></returns>
        public override TokenExpression DoVisit(TokenDouble input)
        {
            return new TokenDouble(0);
        }

        /// <summary>
        /// Differentiates an addition expression that can be a combination of power expressions and constants
        /// </summary>
        /// <param name="input">Addition expression</param>
        /// <returns></returns>
        public override TokenExpression DoVisit(TokenExpressionAdd input)
        {
            TokenExpression[] children = input.Children.Select(c => c.Accept(this)).ToArray();
            return new TokenExpressionAdd(children);
        }
    }
}
