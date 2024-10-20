namespace Willow.PublicApi.Tests.Expressions;

using TechTalk.SpecFlow;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.PublicApi.Expressions;

[Binding]
public class UnderExpressionStepDefinitions
{
    private TokenExpression expression;
    private QueryResult queryResult;

    [Given(@"I have an expression ""([^""]*)""")]
    public void GivenIHaveAnExpression(string expression)
    {
        this.expression = Parser.Deserialize(expression);
    }

    [When(@"I visit it with the TwinQueryVisitor")]
    public void WhenIVisitItWithTheTwinQueryVisitor()
    {
        TwinQueryVisitor twinQueryVisitor = new TwinQueryVisitor();

        queryResult = twinQueryVisitor.Visit(expression);
    }

    [Then(@"the a QueryResult is returned")]
    public void ThenTheAQueryResultIsReturned()
    {
        Assert.NotNull(queryResult);
    }

    [Then(@"success is true")]
    public void ThenSuccessIsTrue()
    {
        Assert.True(queryResult.Success);
    }

    [Then(@"the query is null")]
    public void ThenTheQueryIsNull()
    {
        Assert.Null(queryResult.Query);
    }

    [Then(@"the Request is not null")]
    public void ThenTheRequestIsNotNull()
    {
        Assert.NotNull(queryResult.Request);
    }

    [Then(@"the Request\.LocationId property is ""([^""]*)""")]
    public void ThenTheRequest_LocationIdPropertyIs(string locationId)
    {
        Assert.Equal(locationId, queryResult.Request.LocationId);
    }
}
