namespace Willow.SpecFlow;

using TechTalk.SpecFlow.Assist.ValueRetrievers;

/// <summary>
/// A Value Retriever for DateTime using Willow Expressions.
/// </summary>
/// <remarks>
/// Value retrievers are used by SpecFlow to convert table values to objects.
/// </remarks>
public class DateTimeExpressionValueRetriever : DateTimeValueRetriever
{
    /// <inheritdoc/>
    protected override DateTime GetNonEmptyValue(string value) =>
        DateTimeExpression.Parse(value)!.Value;
}

/// <summary>
/// A Value Retriever for DateTimeOffset using Willow Expressions.
/// </summary>
/// <remarks>
/// Value retrievers are used by SpecFlow to convert table values to objects.
/// </remarks>
public class DateTimeOffsetExpressionValueRetriever : DateTimeOffsetValueRetriever
{
    /// <inheritdoc/>
    protected override DateTimeOffset GetNonEmptyValue(string value) =>
        DateTimeOffsetExpression.Parse(value)!.Value;
}
