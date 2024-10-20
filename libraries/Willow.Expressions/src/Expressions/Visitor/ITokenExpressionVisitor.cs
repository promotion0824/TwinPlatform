using Willow.Units;

namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// Visit a TokenExpression (excluding temporal sets)
    /// </summary>
    public interface ITokenExpressionVisitor<out T>
    {
        /// <summary>
        /// Visit
        /// </summary>
        T Visit(TokenExpression source);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionCount input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionAverage input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionAny input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionIdentity input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionEach input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionAll input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionPropertyAccess input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionVariableAccess input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionFunctionCall input);

        /// <summary>
        /// Visit an abstract constant, concrete constants may rely on this
        /// </summary>
        T DoVisit(TokenExpressionConstant input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionConstantNull input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionConstantDateTime input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionConstantString input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionArray input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionConstantBool input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionConstantColor input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenDouble input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionConvertToLocalDateTime input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionAdd input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionMatches input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionDivide input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionUnaryMinus input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionMultiply input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionPower input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionSubtract input);

        // ------------------------------ Boolean result

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionNot input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionAnd input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionOr input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionTernary input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionIntersection input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionSetUnion input);

        // ------------------------------ Comparisons

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionIs input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionEquals input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionGreater input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionGreaterOrEqual input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionLess input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionLessOrEqual input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionNotEquals input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionTuple input);

        // ------------------------------ IEnumerable arguments

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionSum input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionParameter input);

        // ----------------------------
        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionWrapped input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionFirst input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionMin input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionMax input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionTemporal input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionFailed input);

        /// <summary>
        /// Visit
        /// </summary>
        T DoVisit(TokenExpressionTimer input);
    }
}
