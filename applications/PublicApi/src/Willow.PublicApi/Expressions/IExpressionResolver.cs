namespace Willow.PublicApi.Expressions;

internal interface IExpressionResolver
{
    IReadOnlyDictionary<string, QueryResult?> Expressions { get; }

    void ResolveExpression(string clientId, string? expression);
}
