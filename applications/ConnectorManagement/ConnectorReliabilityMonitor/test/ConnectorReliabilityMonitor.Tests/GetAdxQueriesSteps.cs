namespace Willow.ConnectorReliabilityMonitor.Tests;

using TechTalk.SpecFlow;
using Xunit;

[Binding]
public class GetAdxQueriesSteps
{
    private List<AdxQueryConfigItem> result;

    [When("I call GetAdxQueries")]
    public void WhenICallGetAdxQueries()
    {
        this.result = AdxQueryExecutor.GetAdxQueries();
    }

    [Then("the result should contain (.*) queries")]
    public void ThenTheResultShouldContainQueries(int p0)
    {
        Assert.NotNull(this.result);
        Assert.Equal(p0, this.result.Count);
    }
}
