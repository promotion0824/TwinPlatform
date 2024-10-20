namespace Willow.LiveData.TelemetryStreaming.Tests;

using System.Dynamic;
using Azure;
using Azure.Data.Tables;
using LazyCache.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TechTalk.SpecFlow;
using Willow.LiveData.TelemetryStreaming.Models;
using Willow.LiveData.TelemetryStreaming.Services;

[Binding]
public class StorageTablesSubscriptionServiceStepDefinitions
{
    private readonly List<TableEntity> tableStorage = [];
    private readonly Mock<TableServiceClient> tableServiceClientMock = new();
    private readonly Mock<TableClient> tableClientMock = new();

    private Subscription[] results = [];

    [BeforeScenario]
    public void Reset()
    {
        tableStorage.Clear();
    }

    [Given(@"I have a table storage with the following subscriptions:")]
    public void GivenIHaveATableStorageWithTheFollowingSubscriptions(Table table)
    {
        foreach (var row in table.Rows)
        {
            var entity = new TableEntity(row["PartitionKey"], Guid.NewGuid().ToString())
            {
                { "odata.etag", Guid.NewGuid().ToString() },
            };

            table.Header.Where(h => h != "PartitionKey").ToList().ForEach(h => entity.Add(h, row[h]));

            tableStorage.Add(entity);
        }
    }

    [When(@"I call GetSubscriptions with connector ID '([^']*)' and external ID '([^']*)'")]
    public async Task WhenICallGetSubscriptionsWithConnectorIDAndExternalID(string connectorId, string externalId)
    {
        Mock<Response> responseMock = new();
        responseMock.SetupGet(r => r.Status).Returns(200);
        Page<TableEntity> page = Page<TableEntity>.FromValues(tableStorage, null, responseMock.Object);

        tableServiceClientMock.Setup(m => m.GetTableClient(It.IsAny<string>())).Returns(tableClientMock.Object);
        tableClientMock.Setup(m => m.QueryAsync<TableEntity>((string)null, null, null, default)).Returns(AsyncPageable<TableEntity>.FromPages([page]));

        IOptions<TableConfig> config = Options.Create<TableConfig>(new() { StorageAccountUri = new Uri("https://example.com"), StorageTableName = "Table", CacheExpirationMinutes = 1 });

        ILogger<StorageTablesSubscriptionService> logger = new LoggerFactory().CreateLogger<StorageTablesSubscriptionService>();

        StorageTablesSubscriptionService service = new(config, new MockCachingService(), tableServiceClientMock.Object, logger);

        results = await service.GetSubscriptions(connectorId, externalId);
    }

    [Then(@"I should get the following subscriptions:")]
    public void ThenIShouldGetTheFollowingSubscriptions(Table table)
    {
        var expectedSubscriptions = table.Rows.Select(r =>
        {
            var sub = new Subscription
            {
                ConnectorId = r["ConnectorId"],
                ExternalId = r["ExternalId"],
                SubscriberId = r["SubscriberId"],
                Metadata = null,
            };

            return sub;
        }).ToArray();

        Action<Subscription>[] actions =
        expectedSubscriptions.Select<Subscription, Action<Subscription>>(expectedSub =>
        {
            return (Subscription sub) =>
            {
                Assert.Equal(expectedSub.ConnectorId, sub.ConnectorId);
                Assert.Equal(expectedSub.ExternalId, sub.ExternalId);
                Assert.Equal(expectedSub.SubscriberId, sub.SubscriberId);
            };
        }).ToArray();

        Assert.Collection(results, actions);
    }

    [Then(@"the metadata object will be null")]
    public void ThenTheMetadataObjectWillBeNull()
    {
        var sub = Assert.Single(results);
        Assert.Null(sub.Metadata);
    }

    [Then(@"the metadata object will be have the following properties:")]
    public void ThenTheMetadataObjectWillBeHaveTheFollowingProperties(Table table)
    {
        var expectedMetadata = table.Rows.Select(row =>
        {
            ExpandoObject expectedMetadata = new();
            foreach (var header in table.Header)
            {
                expectedMetadata.TryAdd(header, row[header]);
            }

            return expectedMetadata;
        }).ToList();

        Action<Subscription>[] actions =
        expectedMetadata.Select<ExpandoObject, Action<Subscription>>(expectedMetadata =>
        {
            return (Subscription sub) =>
            {
                Assert.NotNull(sub.Metadata);
                foreach (var header in table.Header)
                {
                    Assert.Equal(sub.Metadata.Select(m => m.Key == header), expectedMetadata.Select(m => m.Key == header));
                }
            };
        }).ToArray();

        Assert.Collection(results, actions);
    }
}
