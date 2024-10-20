namespace Willow.SpecFlow.Tests;

using TechTalk.SpecFlow;
using Willow.Units;

[Binding]
public class SharedStepDefinitions
{
    [Given(@"the current time is '([^']*)'")]
    public void GivenTheCurrentTimeIs(DateTimeOffset input)
    {
        Units.TimeProvider.Current = new ManualTimeProvider(input);
    }
}
