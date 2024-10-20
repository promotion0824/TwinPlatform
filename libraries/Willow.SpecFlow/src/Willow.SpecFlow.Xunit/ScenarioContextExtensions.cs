namespace TechTalk.SpecFlow;

using Willow.SpecFlow;

/// <summary>
/// Extensions for the <see cref="ScenarioContext"/> class.
/// </summary>
public static class ScenarioContextExtensions
{
    /// <summary>
    /// Adds a common result to the context, to be consumed by the <see cref="SimpleAssertionSteps" />.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="context">The <see cref="ScenarioContext"/> object that this method extends.</param>
    /// <param name="result">The result to add.</param>
    public static void AddResult<T>(this ScenarioContext context, T? result)
        => context.Add(SimpleAssertionSteps.ResultKey, result);
}
