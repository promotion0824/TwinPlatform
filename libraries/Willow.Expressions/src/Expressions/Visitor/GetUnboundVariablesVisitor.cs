using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;

namespace Willow.Units.Expressions.Visitor
{
    /// <summary>
    /// A visitor for getting all the unbound variables in a TokenExpression
    /// </summary>
    internal class GetUnboundVariablesVisitor : VariableBitsVisitor<UnboundVariableOrFunction>
    {
        /// <summary>
        /// Visit
        /// </summary>
        public override IEnumerable<UnboundVariableOrFunction> DoVisit(TokenExpressionVariableAccess input)
        {
            yield return new UnboundVariableOrFunction(input.VariableName.ToString(), false);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override IEnumerable<UnboundVariableOrFunction> DoVisit(TokenExpressionFunctionCall input)
        {
            return Enumerable.Repeat<UnboundVariableOrFunction>(new UnboundVariableOrFunction(input.FunctionName, true), 1)
                .Concat(input.Children.SelectMany(x => x.Accept(this)));
        }
    }
}
