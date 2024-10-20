
using System.Linq;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;

namespace Willow.Rules.Services;

public static class ADTClientExtensions
{
	private class CountResult
	{
		public int COUNT { get; set; }
	}

	private static async Task<int> GetCountAsync(DigitalTwinsClient client, string query)
	{
		var queryResult = client.QueryAsync<CountResult>(query);
		var pages = queryResult.AsPages();
		await foreach (var page in pages)
		{
			int count = page.Values.FirstOrDefault()?.COUNT ?? 0;
			return count;
		}
		return 0;
	}

	public static async Task<int> GetTwinsCountAsync(this DigitalTwinsClient client)
	{
		return await GetCountAsync(client, "SELECT COUNT() FROM digitaltwins");
	}

	public static async Task<int> GetRelationshipsCountAsync(this DigitalTwinsClient client)
	{
		return await GetCountAsync(client, "SELECT COUNT() FROM relationships");
	}

}