namespace Willow.SpecFlow;

using TechTalk.SpecFlow;

/// <summary>
/// Reusable assertion steps for simple types.
/// </summary>
/// <param name="context">The current scenario context.</param>
[Binding]
public class SimpleAssertionSteps(ScenarioContext context)
{
    /// <summary>
    /// The key used to store the result in the context.
    /// </summary>
    public const string ResultKey = "Result";

    /// <summary>
    /// Asserts whether the result is equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    [Then(@"the string value ""([^""]*)"" is returned")]
    [Then(@"the string value '([^']*)' is returned")]
    public void ThenTheStringValueIsReturned(string? expected) => AssertValue(expected);

    /// <summary>
    /// Asserts whether the result is equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    [Then(@"the integer value (.*) is returned")]
    public void ThenTheIntegerValueIsReturned(int? expected) => AssertValue(expected);

    /// <summary>
    /// Asserts whether the result is equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    [Then(@"the double value (.*) is returned")]
    public void ThenTheDoubleValueIsReturned(double? expected) => AssertValue(expected);

    /// <summary>
    /// Asserts whether the result is equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    [Then(@"the decimal value (.*) is returned")]
    public void ThenTheDecimalValueIsReturned(decimal? expected) => AssertValue(expected);

    /// <summary>
    /// Asserts whether the result is equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    [Then(@"the date ""([^""]*)"" is returned")]
    [Then(@"the date '([^']*)' is returned")]
    public void ThenTheDateIsReturned(DateTime? expected) => AssertValue(expected);

    /// <summary>
    /// Asserts whether the result is equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    [Then(@"the boolean value (.*) is returned")]
    public void ThenTheBooleanValueIsReturned(bool? expected) => AssertValue(expected);

    /// <summary>
    /// Asserts whether the result is <see langword="null" />.
    /// </summary>
    [Then(@"the value is null")]
    public void ThenTheValueIsNull()
    {
        var result = context.Get<object?>(ResultKey);
        Assert.Null(result);
    }

    private void AssertValue<T>(T? expected)
    {
        var result = context.Get<T>(ResultKey);

        Assert.Equal(expected, result);
    }
}
