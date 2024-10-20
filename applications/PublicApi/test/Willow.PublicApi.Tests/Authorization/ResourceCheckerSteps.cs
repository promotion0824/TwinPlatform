namespace Willow.PublicApi.Tests.Authorization;

using Azure.DigitalTwins.Core;
using LazyCache;
using LazyCache.Mocks;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.PublicApi.Authorization;
using Willow.PublicApi.Expressions;
using Willow.PublicApi.Services;

[Binding]
[Scope(Feature = "Resource Checker")]
public class ResourceCheckerSteps(ScenarioContext scenarioContext)
{
    private Mock<IExpressionResolver> expressionResolverMock;
    private Mock<IClientIdAccessor> clientIdAccessorMock;
    private Mock<ITwinsClient> twinsClientMock;
    private IAppCache cacheMock;
    private ResourceChecker resourceChecker;
    private bool permissionResult;
    private IEnumerable<string> filteredIds;
    private IEnumerable<(string TwinId, string ExternalId)> allowedTwins;

    [Given(@"a client ID ""(.*)""")]
    public void GivenAClientID(string clientId)
    {
        clientIdAccessorMock = new Mock<IClientIdAccessor>();
        clientIdAccessorMock.Setup(c => c.GetClientId()).Returns(clientId);
    }

    [Given(@"an expression resolver with expressions for the client ID")]
    public void GivenAnExpressionResolverWithExpressionsForTheClientID()
    {
        expressionResolverMock = new Mock<IExpressionResolver>();
        var queryResult = new QueryResult(new GetTwinsInfoRequest() { LocationId = "location-123" });
        expressionResolverMock.Setup(e => e.Expressions).Returns(new Dictionary<string, QueryResult> { { "client-123", queryResult } });
    }

    [Given(@"a twins client with twin data")]
    public void GivenATwinsClientWithTwinData(Table table)
    {
        var expectedTwins = table.Rows.Select(row => new BasicDigitalTwin { Id = row["Twin ID"], Contents = new Dictionary<string, object> { { "externalID", row["External ID"] } } }).ToList();

        twinsClientMock = new Mock<ITwinsClient>();

        twinsClientMock.Setup(c => c.GetTwinByIdAsync(It.IsAny<string>(), null, null, default)).ReturnsAsync(new TwinWithRelationships { Twin = expectedTwins.First() }); //  new BasicDigitalTwin { Id = "location-123" } });
        twinsClientMock.Setup(c => c.QueryTwinsAsync(It.IsAny<GetTwinsInfoRequest>(), null, null, null, default)).ReturnsAsync(new Page<TwinWithRelationships> { Content = expectedTwins.Skip(1).Select(t => new TwinWithRelationships { Twin = t }) });
    }

    [Given(@"a cache service")]
    public void GivenACacheService()
    {
        cacheMock = new MockCachingService();
    }

    [Given(@"a twin ID ""(.*)""")]
    public void GivenATwinID(string twinId)
    {
        scenarioContext["TwinId"] = twinId;
    }

    [Given(@"an external ID ""(.*)""")]
    public void GivenAnExternalID(string externalId)
    {
        scenarioContext["ExternalId"] = externalId;
    }

    [Given(@"a list of twin IDs")]
    public void GivenAListOfTwinIDs(Table table)
    {
        scenarioContext["TwinIds"] = table.Rows.Select(row => row["Twin ID"]).ToList();
    }

    [Given(@"a list of external IDs")]
    public void GivenAListOfExternalIDs(Table table)
    {
        scenarioContext["ExternalIds"] = table.Rows.Select(row => row["External ID"]).ToList();
    }

    [When(@"I check for twin permission")]
    public async Task WhenICheckForTwinPermission()
    {
        var twinId = scenarioContext["TwinId"].ToString();
        resourceChecker = new ResourceChecker(expressionResolverMock.Object, clientIdAccessorMock.Object, twinsClientMock.Object, cacheMock);
        permissionResult = await resourceChecker.HasTwinPermission(twinId);
    }

    [When(@"I check for external ID permission")]
    public async Task WhenICheckForExternalIDPermission()
    {
        var externalId = scenarioContext["ExternalId"].ToString();
        resourceChecker = new ResourceChecker(expressionResolverMock.Object, clientIdAccessorMock.Object, twinsClientMock.Object, cacheMock);
        permissionResult = await resourceChecker.HasExternalIdPermission(externalId);
    }

    [When(@"I filter twin IDs based on permissions")]
    public async Task WhenIFilterTwinIDsBasedOnPermissions()
    {
        var twinIds = (IEnumerable<string>)scenarioContext["TwinIds"];
        resourceChecker = new ResourceChecker(expressionResolverMock.Object, clientIdAccessorMock.Object, twinsClientMock.Object, cacheMock);
        filteredIds = await resourceChecker.FilterTwinPermission(twinIds);
    }

    [When(@"I filter external IDs based on permissions")]
    public async Task WhenIFilterExternalIDsBasedOnPermissions()
    {
        var externalIds = (IEnumerable<string>)scenarioContext["ExternalIds"];
        resourceChecker = new ResourceChecker(expressionResolverMock.Object, clientIdAccessorMock.Object, twinsClientMock.Object, cacheMock);
        filteredIds = await resourceChecker.FilterExternalIdPermission(externalIds);
    }

    [When(@"I get allowed twins")]
    public async Task WhenIGetAllowedTwins()
    {
        resourceChecker = new ResourceChecker(expressionResolverMock.Object, clientIdAccessorMock.Object, twinsClientMock.Object, cacheMock);
        allowedTwins = await resourceChecker.GetAllowedTwins();
    }

    [Then(@"the result should be true")]
    public void ThenTheResultShouldBeTrue()
    {
        Assert.True(permissionResult);
    }

    [Then(@"the result should contain")]
    public void ThenTheResultShouldContain(Table table)
    {
        var expectedIds = table.Rows.Select(row => row[0]).ToList();
        Assert.Equal(expectedIds, filteredIds);
    }

    [Then(@"the result should contain twin IDs and external IDs")]
    public void ThenTheResultShouldContainTwinIDsAndExternalIDs(Table table)
    {
        var expectedTwins = table.Rows.Select(row => (row["Twin ID"], row["External ID"])).ToList();
        Assert.Equal(expectedTwins, allowedTwins);
    }
}
