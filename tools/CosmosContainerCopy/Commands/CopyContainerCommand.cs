using System.CommandLine;
using System.CommandLine.Invocation;
using Azure.Identity;
using CosmosContainerCopy.Repositories;
using Microsoft.Azure.Cosmos;

namespace CosmosContainerCopy.Commands;

internal class CopyContainerCommand : Command
{
	public CopyContainerCommand() : base("copycontainer", "Copy contents of one CosmosDb container into another")
	{
		AddOption(new Option<string>("--cosmosDbEndpointUri", description: "Endpoint uri of the CosmosDb")
		{
			IsRequired = true
		});
		AddOption(new Option<string>("--database", description: "CosmosDb Database")
		{
			IsRequired = true,
		});
		AddOption(new Option<string>("--from", description: "Source CosmosDb Container")
		{
			IsRequired = true
		});
		AddOption(new Option<string>("--to", description: "Destination CosmosDb Container")
		{
			IsRequired = true
		});
		AddOption(new Option<string>("--partitionKey", description: "CosmosDb Container Partition Key")
		{
			IsRequired = true
		});
	}

	public new class Handler : ICommandHandler
	{
        /// <summary>
        /// Gets the CosmosDb endpoint uri.
        /// </summary>
		public string CosmosDbEndpointUri { get; init; } = string.Empty;

        /// <summary>
        /// Gets the CosmosDb database.
        /// </summary>
		public string Database { get; init; } = string.Empty;

        /// <summary>
        /// Gets the source CosmosDb container.
        /// </summary>
		public string From { get; init; } = string.Empty;

        /// <summary>
        /// Gets the destination CosmosDb container.
        /// </summary>
		public string To { get; init; } = string.Empty;

        /// <summary>
        /// Gets the partition key.
        /// </summary>
		public string PartitionKey { get; init; } = string.Empty;

        public int Invoke(InvocationContext context)
        {
            throw new NotImplementedException();
        }

        public async Task<int> InvokeAsync(InvocationContext context)
		{
			var client = CreateCosmosClient();

			var fromRepository = new RecordRepository(client, Database, From);
			var sourceItems = await fromRepository.GetAll(whereClause: null, itemCount: null, cancellationToken: context.GetCancellationToken());

			var toRepository = new RecordRepository(client, Database, To);
			var existingItems = await toRepository.GetAll(whereClause: null, itemCount: null, cancellationToken: context.GetCancellationToken());

			var dupes = sourceItems.Items.Where(i => 
				existingItems.Items.Any(ei => 
					ei["id"].ToString() == i["id"].ToString())).ToList();

			var items = sourceItems.Items.ToList();
			foreach (var dupe in dupes)
			{
				items.Remove(dupe);
			}

			foreach (var item in items)
			{
				await toRepository.Upsert(item, item[PartitionKey].ToString(), context.GetCancellationToken());
			}

			return 0;
		}

		private CosmosClient CreateCosmosClient()
		{
			var cosmosOptions = new CosmosClientOptions()
			{
				SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
			};
			var client = new CosmosClient(CosmosDbEndpointUri, new DefaultAzureCredential(), cosmosOptions);
			return client;
		}
	}
}
