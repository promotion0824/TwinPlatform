namespace Willow.SpecFlow.Tests;

using System;
using TechTalk.SpecFlow;
using Willow.Units;
using Xunit;

[Binding]
public class TestDateTimeExpressionsStepDefinitions
{
    private DateTimeOffset? result;

    [When(@"I evaluate the expression '([^']*)'")]
    public void WhenIEvaluateTheExpression(string expression)
    {
        result = DateTimeExpression.Parse(expression);
    }

    [When(@"I parse the expression '([^']*)' to a DateTimeOffset")]
    public void WhenIParseTheExpressionToADateTimeOffset(string expression)
    {
        result = DateTimeOffsetExpression.Parse(expression);
    }

    [Scope(Scenario = "Test DateTime Expressions")]
    [Then(@"the result should be '([^']*)'")]
    public void ThenTheResultShouldBe(DateTime? expected)
    {
        if (expected == null)
        {
            Assert.Null(result);
            return;
        }

        Assert.NotNull(result);
        Assert.Equal(expected, result.Value.DateTime);
    }

    [Scope(Scenario = "Test DateTimeOffset Expressions")]
    [Then(@"the result should be '([^']*)'")]
    public void ThenTheResultShouldBe(DateTimeOffset? expected)
    {
        Assert.Equal(expected, result);
    }
}
