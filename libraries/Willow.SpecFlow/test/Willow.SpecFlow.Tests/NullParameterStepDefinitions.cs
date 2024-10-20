namespace Willow.SpecFlow.Tests;

using TechTalk.SpecFlow;
using Xunit;

[Binding]
public class NullParameterStepDefinitions
{
    private object? input;

    [Given(@"a string ""([^""]*)""")]
    public void GivenAString(string? input)
    {
        this.input = input;
    }

    [Given(@"an int (.+)")]
    public void GivenAnInt(int? input)
    {
        this.input = input;
    }

    [Given(@"a double (.+)")]
    public void GivenADouble(double? input)
    {
        this.input = input;
    }

    [Given(@"a decimal (.+)")]
    public void GivenADecimal(decimal? input)
    {
        this.input = input;
    }

    [Given(@"a boolean (.+)")]
    public void GivenABool(bool? input)
    {
        this.input = input;
    }

    [Then(@"the value is null")]
    public void ThenTheValueIsNull()
    {
        Assert.Null(input);
    }
}
