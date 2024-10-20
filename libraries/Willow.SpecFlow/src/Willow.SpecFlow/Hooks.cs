namespace Willow.SpecFlow;

using TechTalk.SpecFlow.Assist;
using TechTalk.SpecFlow.Assist.ValueRetrievers;

/// <summary>
/// Creates SpecFlow value retrievers used when instantiating objects and sets
/// from tables.
/// </summary>
[Binding]
public static class Hooks
{
    /// <summary>
    /// Adds the value retrievers.
    /// </summary>
    /// <remarks>
    /// Executes once before the test run.
    /// </remarks>
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Converts the string "$null" to null.
        Service.Instance.ValueRetrievers.Register(new NullValueRetriever(NullStringRegex));

        // Replaces the standard DateTime retriever with one that can parse Willow expressions.
        Service.Instance.ValueRetrievers.Replace<DateTimeValueRetriever,  DateTimeExpressionValueRetriever>();
        Service.Instance.ValueRetrievers.Replace<DateTimeOffsetValueRetriever, DateTimeOffsetExpressionValueRetriever>();
    }
}
