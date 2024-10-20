using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Willow.DataAccess.CosmosDb;

namespace CosmosContainerCopy.Repositories;

internal class RecordRepository : CosmosDbRepository<Dictionary<string, JToken>>
{
	public RecordRepository(CosmosClient client, string database, string container) : base(client)
	{
		DatabaseName = database;
		ContainerName = container;
	}

	protected override string DatabaseName { get; }

	protected override string ContainerName { get; }
}
