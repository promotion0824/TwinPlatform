using System.Linq;

namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// Use this base-class visitor when you don't want to have to implement every method from the interface
    /// </summary>
    public class TokenExpressionVisitor : ITokenExpressionVisitor<TokenExpression>
    {
        /// <summary>
        /// Creates a new instance of the TokenExpresion Visitor with no separate TemporalSetVisitor
        /// </summary>
        public TokenExpressionVisitor()
        {
        }

        /// <inheritdoc />
        public TokenExpression Visit(TokenExpression source)
        {
            return Visited(source, source.Accept(this));
        }

        /// <inheritdoc />
        public virtual TokenExpression DoVisit(TokenExpressionMultiply input)
        {
            return Visited(input, new TokenExpressionMultiply(input.Children.Select(c => c.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionPropertyAccess input)
        {
            return Visited(input, new TokenExpressionPropertyAccess(input.Child.Accept(this), input.Type, input.PropertyName));
        }

        public virtual TokenExpression DoVisit(TokenExpressionVariableAccess input)
        {
            return Visited(input, input);
        }

        public virtual TokenExpression DoVisit(TokenExpressionFunctionCall input)
        {
            return Visited(input, new TokenExpressionFunctionCall(input.FunctionName, input.Type, input.Children.Select(c => c.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionNotEquals input)
        {
            return Visited(input, new TokenExpressionNotEquals(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionOr input)
        {
            var children = input.Children.Select(c => c.Accept(this)).ToArray();
            return Visited(input, new TokenExpressionOr(children));
        }

        public virtual TokenExpression DoVisit(TokenExpressionTernary input)
        {
            return Visited(input, new TokenExpressionTernary(input.Conditional.Accept(this), input.Truth.Accept(this), input.Falsehood.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionTuple input)
        {
            return Visited(input, new TokenExpressionTuple(input.Children.Select(c => c.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionPower input)
        {
            return Visited(input, new TokenExpressionPower(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionSubtract input)
        {
            return Visited(input, new TokenExpressionSubtract(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionIdentity input)
        {
            return Visited(input, new TokenExpressionIdentity(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionFailed input)
        {
            return Visited(input, new TokenExpressionFailed(input.Children.Select(x => x.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionSetUnion input)
        {
            return Visited(input, new TokenExpressionSetUnion(input.Children.Select(c => c.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionIntersection input)
        {
            return Visited(input, new TokenExpressionIntersection(input.Children.Select(c => c.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionConstant input)
        {
            return Visited(input, input);
        }

        public virtual TokenExpression DoVisit(TokenDouble input)
        {
            return Visited(input, DoVisit((TokenExpressionConstant)input));
        }

        public virtual TokenExpression DoVisit(TokenExpressionConvertToLocalDateTime input)
        {
            return Visited(input, new TokenExpressionConvertToLocalDateTime(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionAdd input)
        {
            return Visited(input, new TokenExpressionAdd(input.Children.Select(c => c.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionMatches input)
        {
            return Visited(input, new TokenExpressionMatches(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionAnd input)
        {
            var children = input.Children.Select(c => c.Accept(this)).ToArray();
            return Visited(input, new TokenExpressionAnd(children));
        }

        public virtual TokenExpression DoVisit(TokenExpressionConstantNull input)
        {
            return Visited(input, DoVisit((TokenExpressionConstant)input));
        }

        public virtual TokenExpression DoVisit(TokenExpressionConstantDateTime input)
        {
            return Visited(input, DoVisit((TokenExpressionConstant)input));
        }

        public virtual TokenExpression DoVisit(TokenExpressionConstantString input)
        {
            return Visited(input, DoVisit((TokenExpressionConstant)input));
        }

        public virtual TokenExpression DoVisit(TokenExpressionDivide input)
        {
            return Visited(input, new TokenExpressionDivide(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionConstantBool input)
        {
            return Visited(input, DoVisit((TokenExpressionConstant)input));
        }

        public virtual TokenExpression DoVisit(TokenExpressionConstantColor input)
        {
            return Visited(input, DoVisit((TokenExpressionConstant)input));
        }

        public virtual TokenExpression DoVisit(TokenExpressionIs input)
        {
            return Visited(input, new TokenExpressionIs(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionEquals input)
        {
            return Visited(input, new TokenExpressionEquals(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionGreater input)
        {
            return Visited(input, new TokenExpressionGreater(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionGreaterOrEqual input)
        {
            return Visited(input, new TokenExpressionGreaterOrEqual(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionNot input)
        {
            return Visited(input, new TokenExpressionNot(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionUnaryMinus input)
        {
            return Visited(input, new TokenExpressionUnaryMinus(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionLess input)
        {
            return Visited(input, new TokenExpressionLess(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionLessOrEqual input)
        {
            return Visited(input, new TokenExpressionLessOrEqual(input.Left.Accept(this), input.Right.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionArray input)
        {
            return Visited(input, new TokenExpressionArray(input.Children.Select(c => c.Accept(this)).ToArray()));
        }

        public virtual TokenExpression DoVisit(TokenExpressionSum input)
        {
            return Visited(input, new TokenExpressionSum(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionCount input)
        {
            return Visited(input, new TokenExpressionCount(input.Child.Accept(this)) { Unit = "scalar" });
        }

        public virtual TokenExpression DoVisit(TokenExpressionAverage input)
        {
            return Visited(input, new TokenExpressionAverage(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionAny input)
        {
            return Visited(input, new TokenExpressionAny(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionFirst input)
        {
            return Visited(input, new TokenExpressionFirst(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionEach input)
        {
            var enumerable = input.EnumerableArgument.Accept(this);
            var variableName = input.VariableName;
            var body = input.Body.Accept(this);
            return Visited(input, new TokenExpressionEach(enumerable, variableName, body));
        }

        public virtual TokenExpression DoVisit(TokenExpressionAll input)
        {
            return Visited(input, new TokenExpressionAll(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionMin input)
        {
            return Visited(input, new TokenExpressionMin(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionMax input)
        {
            return Visited(input, new TokenExpressionMax(input.Child.Accept(this)));
        }

        public virtual TokenExpression DoVisit(TokenExpressionParameter input)
        {
            return Visited(input, input);
        }

        public virtual TokenExpression DoVisit(TokenExpressionWrapped input)
        {
            return Visited(input, input);
        }

        public virtual TokenExpression DoVisit(TokenExpressionTemporal input)
        {
            var timePeriod = input.TimePeriod;
            var timeFrom = input.TimeFrom;

            if (input.TimePeriod is not null)
            {
                timePeriod = input.TimePeriod.Accept(this);

                if (string.IsNullOrEmpty(timePeriod.Unit))
                {
                    timePeriod.Unit = input.TimePeriod.Unit;
                }
            }

            if (input.TimeFrom is not null)
            {
                timeFrom = input.TimeFrom.Accept(this);

                if (string.IsNullOrEmpty(timeFrom.Unit))
                {
                    timeFrom.Unit = input.TimeFrom.Unit;
                }
            }

            return Visited(input, new TokenExpressionTemporal(input.FunctionName, input.Child.Accept(this), timePeriod, timeFrom, input.UnitOfMeasure)
            {
                Text = input.Text
            });
        }

        public virtual TokenExpression DoVisit(TokenExpressionTimer input)
        {
            return Visited(input, new TokenExpressionTimer(input.Child.Accept(this), input.UnitOfMeasure));
        }

        /// <summary>
        /// Called before visiting any expression
        /// </summary>
        protected TokenExpression Visited(TokenExpression input, TokenExpression expression)
        {
            if (string.IsNullOrEmpty(expression.Unit))
            {
                expression.Unit = input.Unit;
            }

            if (string.IsNullOrEmpty(expression.Text))
            {
                expression.Text = input.Text;
            }

            return OnVisited(input, expression);
        }

        /// <summary>
        /// Called before visiting any expression
        /// </summary>
        protected virtual TokenExpression OnVisited(TokenExpression input, TokenExpression expression)
        {
            return expression;
        }
    }
}
