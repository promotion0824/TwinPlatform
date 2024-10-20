namespace Willow.PublicApi.Expressions;

using Willow.ExpressionParser;

internal class ExpressionResolver : IExpressionResolver
{
    private readonly Dictionary<string, QueryResult?> expressionsInternal = [];

    public IReadOnlyDictionary<string, QueryResult?> Expressions => expressionsInternal;

    /// <summary>
    /// Resolves a Willow expression to an ADT query.
    /// </summary>
    /// <remarks>
    /// Only UNDER([TwinID]) is supported.
    /// A null or empty expression will indicate full permissions.
    /// </remarks>
    /// <param name="clientId">The client ID.</param>
    /// <param name="expression">The expression to parse.</param>
    public void ResolveExpression(string clientId, string? expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            expressionsInternal[clientId] = null;
            return;
        }

        var tokenExpression = Parser.Deserialize(expression);

        TwinQueryVisitor visitor = new();

        var filterResult = visitor.Visit(tokenExpression);

        expressionsInternal[clientId] = filterResult;
    }
}
