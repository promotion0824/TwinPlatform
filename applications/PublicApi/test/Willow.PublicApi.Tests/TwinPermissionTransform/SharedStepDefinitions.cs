namespace Willow.PublicApi.Tests.TwinPermissionTransform;

using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Willow.PublicApi.Authorization;
using Willow.PublicApi.Transforms;

[Binding]
public class SharedStepDefinitions(ScenarioContext context)
{
    public const string ResultKey = "Result";

    private readonly Dictionary<string, string> transformValues = [];

    [Given(@"I have permission to the following twin IDs")]
    public void IHavePermissionToTheFollowingTwinIDs(Table table)
    {
        var allowedTwinIds = table.Rows.Select<TableRow, (string TwinId, string ExternalId)>(row => (row["Twin ID"], row["External ID"])).ToArray();

        Mock<IResourceChecker> resourceCheckerMock = new();

        resourceCheckerMock.Setup(mock => mock.HasTwinPermission(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string twinId, CancellationToken cancellationToken) => allowedTwinIds.Any(t => t.TwinId == twinId));

        resourceCheckerMock.Setup(mock => mock.FilterTwinPermission(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<string> twinIds, CancellationToken cancellationToken) => twinIds.Where(t => allowedTwinIds.Any(at => at.TwinId == t)));

        resourceCheckerMock.Setup(mock => mock.GetAllowedTwins(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowedTwinIds);

        context.Set(resourceCheckerMock);
    }

    [Given(@"I have a transform of type ""([^""]*)"" and value ""([^""]*)""")]
    public void GivenIHaveATransformOfTypeAndValue(string type, string value)
    {
        transformValues[type] = value;
    }

    [When(@"I validate the transform")]
    public void WhenIValidateTheTransform()
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, null, null);
        var result = twinPermissionsTransform.Validate(null, transformValues);
        context.Set(result, ResultKey);
    }

    [Then(@"[T,t]he result will be (.*)")]
    public void ThenTheResultWillBe(bool result)
    {
        var actual = context.Get<bool>(ResultKey);
        Assert.Equal(result, actual);
    }

    [Then(@"the response status code will be (\d+)")]
    public void ThenTheResponseStatusCodeWillBeStatusCode(string statusCode)
    {
        Assert.Equal(int.Parse(statusCode), context.Get<int>(ResultKey));
    }

    [StepArgumentTransformation]
    public string[] ArrayofStrings(string values) => values.Split(',');
}
