using Azure;
using Azure.DigitalTwins.Core;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Services.Domain.InMemory.Readers;

public class InMemoryInstanceTwinReader : InMemoryTwinReader
{
    protected IAzureDigitalTwinReader AzureDigitalTwinReader { get; }

    public InMemoryInstanceTwinReader(IAzureDigitalTwinReader azureDigitalTwinReader,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider) : base(azureDigitalTwinModelParser, azureDigitalTwinCacheProvider)
    {
        AzureDigitalTwinReader = azureDigitalTwinReader;
    }

    public override async Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = DefaultPageSize, string continuationToken = null)
    {
        if (query == null)
        {
            throw new Exception("Query cannot be null");
        }

        if (query.Equals("select * from digitaltwins", StringComparison.InvariantCultureIgnoreCase))
            return AzureDigitalTwinCache.TwinCache.Twins.Select(x => x.Value).ToPageModel(GetPageNumber(continuationToken), DefaultPageSize);

        return await AzureDigitalTwinReader.QueryTwinsAsync(query, pageSize, continuationToken);
    }

    public override AsyncPageable<T> QueryAsync<T>(string query)
    {
        return AzureDigitalTwinReader.QueryAsync<T>(query);
    }
}
