namespace Willow.PublicApi.Expressions;

using System;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Model.Requests;

/// <summary>
/// Visits a permission expression and converts it to either a <see cref="GetTwinsInfoRequest"/>
/// or an ADT query that can be run via the ADT API.
/// </summary>
/// <remarks>
/// This visitor only supports UNDER([twinID]) expressions.
/// </remarks>
internal class TwinQueryVisitor : ITokenExpressionVisitor<QueryResult>
{
    public QueryResult Visit(TokenExpression source) => source.Accept(this);

    public QueryResult DoVisit(TokenExpressionCount input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionAverage input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionAny input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionIdentity input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionEach input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionAll input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionPropertyAccess input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionVariableAccess input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionFunctionCall input)
    {
        if (!input.FunctionName.Equals("UNDER", StringComparison.OrdinalIgnoreCase) || input.Children.FirstOrDefault() is not TokenExpressionVariableAccess)
        {
            return QueryResult.Failed;
        }

        string[] ancestorIds = input.Children
            .OfType<TokenExpressionVariableAccess>()
            .Select(v => v.ToString())
            .ToArray();

        if (ancestorIds.Length != 1)
        {
            return QueryResult.Failed;
        }

        return new QueryResult(new GetTwinsInfoRequest
        {
            SourceType = Model.Adt.SourceType.AdtQuery,
            IncludeIncomingRelationships = true,
            LocationId = ancestorIds[0],
        });
    }

    public QueryResult DoVisit(TokenExpressionConstant input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionConstantNull input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionConstantDateTime input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionConstantString input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionArray input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionConstantBool input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionConstantColor input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenDouble input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionConvertToLocalDateTime input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionAdd input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionMatches input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionDivide input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionUnaryMinus input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionMultiply input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionPower input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionSubtract input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionNot input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionAnd input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionOr input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionTernary input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionIntersection input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionSetUnion input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionIs input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionEquals input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionGreater input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionGreaterOrEqual input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionLess input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionLessOrEqual input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionNotEquals input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionTuple input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionSum input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionParameter input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionWrapped input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionFirst input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionMin input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionMax input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionTemporal input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionFailed input) => QueryResult.Failed;

    public QueryResult DoVisit(TokenExpressionTimer input) => QueryResult.Failed;
}
