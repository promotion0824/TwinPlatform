namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// Rebind a variable to a different TokenExpression, e.g. change a variable to a constant value
    /// </summary>
    public class TokenExpressionRebinder
        : TokenExpressionVisitor
    {
        private readonly Env env;

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionRebinder"/> class
        /// </summary>
        public TokenExpressionRebinder(Env env)
        {
            this.env = env;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionVariableAccess variableAccess)
        {
            if (env.IsDefined(variableAccess.VariableName))
            {
                // TODO: What about Sets and Ranges containing the variable? (not allowed yet)
                return TokenExpressionConstant.Create(env.Get(variableAccess.VariableName)!);
            }
            return variableAccess;
        }
    }
}
