namespace Willow.SpecFlow.Tests;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;

[Binding]
public class TestDateTimeExpressionsValueRetrieverStepDefinitions
{
    private IEnumerable<TestObject> objects = [];
    private IEnumerable<TestOffsetObject> offsetObjects = [];

    [Given(@"I have test objects")]
    public void GivenIHaveTestObjects(Table table)
    {
        objects = table.CreateSet<TestObject>();
    }

    [Then(@"the objects' CurrentDateTime should be")]
    public void ThenTheObjectsResult(Table table)
    {
        var expected = table.CreateSet<TestObject>();

        var inspectors = expected.Select<TestObject, Action<TestObject>>((e) => (TestObject a) => Assert.Equal(e.CurrentDateTime, a.CurrentDateTime)).ToArray();

        Assert.Collection(objects, inspectors);
    }

    [Given(@"I have test offset objects")]
    public void GivenIHaveTestOffsetObjects(Table table)
    {
        offsetObjects = table.CreateSet<TestOffsetObject>();
    }

    [Then(@"the offset objects' CurrentDateTime should be")]
    public void ThenTheOffsetObjectsResult(Table table)
    {
        var expected = table.CreateSet<TestOffsetObject>();

        var inspectors = expected.Select<TestOffsetObject, Action<TestOffsetObject>>((e) => (TestOffsetObject a) => Assert.Equal(e.CurrentDateTime, a.CurrentDateTime)).ToArray();

        Assert.Collection(offsetObjects, inspectors);
    }
}

public class TestObject
{
    public DateTime? CurrentDateTime { get; set; }
}

public class TestOffsetObject
{
    public DateTimeOffset? CurrentDateTime { get; set; }
}
